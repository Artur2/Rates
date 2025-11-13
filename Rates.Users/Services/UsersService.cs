using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Grpc.Core;
using LinqToDB;
using Rates.Domain.Entities;
using Rates.Shared.Services;
using Rates.Users.Data;

namespace Rates.Users.Services;

public class UsersService(UsersDataContext usersDataContext, ITokenService tokenService, IHashingService hashingService)
    : Users.UsersService.UsersServiceBase
{
    public override async Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Password))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid username or password."));
        }

        await usersDataContext.Users.InsertWithIdentityAsync(() => new User
        {
            Name = request.Name,
            PasswordHash = hashingService.ComputeHash(request.Password),
        });

        return new RegisterReply();
    }

    public override async Task<AuthenticateResponse> Authenticate(AuthenticateRequest request,
        ServerCallContext context)
    {
        var passwordHash = hashingService.ComputeHash(request.Password);
        var user = await usersDataContext.Users.SingleOrDefaultAsync(u =>
            u.PasswordHash == passwordHash && u.Name == request.Name);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid username or password"));
        }

        var (token, identifier, refreshToken) = await tokenService.GenerateToken(request.Name);


        await usersDataContext.LoginItems.InsertWithIdentityAsync(() => new LoginItem
        {
            Identifier = identifier,
            UserId = user.Id,
            IsRevoked = false,
            RefreshToken = refreshToken
        });

        return new AuthenticateResponse
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public override async Task<RevokeTokenResponse> RevokeToken(RevokeTokenRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid token."));
        }

        var handler = new JwtSecurityTokenHandler();
        var deserializedToken = handler.ReadJwtToken(request.Token);
        var nameClaim = await tokenService.GetEncryptedClaim(request.Token, ClaimTypes.NameIdentifier);
        if (nameClaim == null)
        {
            throw new InvalidOperationException("Invalid payload");
        }

        var user = await usersDataContext.Users.SingleOrDefaultAsync(u => u.Name == nameClaim);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid username");
        }

        await usersDataContext.LoginItems
            .Where(i => i.Identifier == deserializedToken.Payload.Jti && i.UserId == user.Id)
            .Set(set => set.IsRevoked, true)
            .UpdateAsync();

        return new RevokeTokenResponse();
    }

    public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid refresh token."));
        }

        var requestedUser = await usersDataContext.LoginItems.Where(x =>
                x.RefreshToken == request.RefreshToken &&
                x.IsRevoked == false)
            .LeftJoin(usersDataContext.Users, (loginItem, user) => loginItem.UserId == user.Id,
                (loginItem, user) => user)
            .FirstOrDefaultAsync();

        if (requestedUser == null)
        {
            throw new InvalidOperationException("Invalid refresh token");
        }

        var (token, identifier, refreshToken) = await tokenService.GenerateToken(requestedUser.Name);

        var transaction = await usersDataContext.BeginTransactionAsync();
        try
        {
            await usersDataContext.LoginItems.InsertWithIdentityAsync(() => new LoginItem
            {
                Identifier = identifier,
                UserId = requestedUser.Id,
                IsRevoked = false,
                RefreshToken = refreshToken
            });

            await usersDataContext.LoginItems.Where(x => x.RefreshToken == request.RefreshToken)
                .AsUpdatable()
                .Set(x => x.IsRevoked, true)
                .UpdateAsync();

            await transaction.CommitAsync();

            return new RefreshTokenResponse()
            {
                Token = token,
                RefreshToken = refreshToken,
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}