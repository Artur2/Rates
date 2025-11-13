using Grpc.Core;
using Rates.Gateway.Dtos;
using Rates.Shared.Services;
using Rates.Users;

namespace Rates.Gateway.Services;

public class UsersService(Users.UsersService.UsersServiceClient client)
{
    public async Task<AuthenticationResponseDto> Authenticate(string username, string password)
    {
        var response = await client.AuthenticateAsync(new AuthenticateRequest()
        {
            Name = username,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        return new AuthenticationResponseDto(response.Token, response.RefreshToken);
    }

    public async Task Register(string username, string password)
    {
        var response = await client.RegisterAsync(new RegisterRequest()
        {
            Name = username,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });
    }

    public async Task RevokeToken(string token)
    {
        _ = await client.RevokeTokenAsync(new RevokeTokenRequest()
        {
            Token = token
        });
    }

    public async Task<AuthenticationResponseDto> RefreshToken(string refreshToken)
    {
        var response = await client.RefreshTokenAsync(new RefreshTokenRequest()
        {
            RefreshToken = refreshToken
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        return new AuthenticationResponseDto(response.Token, response.RefreshToken);
    }
}