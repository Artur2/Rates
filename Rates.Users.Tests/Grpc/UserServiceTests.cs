using Grpc.Core;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rates.Shared.Services;
using Rates.Users.Data;
using Xunit;

namespace Rates.Users.Tests.Grpc;

public class UserServiceTests(WebApplicationFactory<Startup> factory) : TestClassBase(factory)
{
    [Fact]
    public async Task Should_Successfully_Register()
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();
        var name = Faker.Random.String2(10);

        await userService.RegisterAsync(new RegisterRequest()
        {
            Name = name,
            Password = "password"
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        var dataContext = Factory.Services.GetRequiredService<UsersDataContext>();
        Assert.True(await dataContext.Users
            .AnyAsync(x => x.Name == name));
    }

    [Fact]
    public async Task Should_Authenticate_Successfully()
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();
        var name = Faker.Random.String2(10);
        var password = Faker.Random.String2(10);

        await userService.RegisterAsync(new RegisterRequest()
        {
            Name = name,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        var response = await userService.AuthenticateAsync(new AuthenticateRequest()
        {
            Name = name,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
    }

    [Fact]
    public async Task Should_Successfully_Revoke_Token()
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();
        var name = Faker.Random.String2(10);
        var password = Faker.Random.String2(10);

        await userService.RegisterAsync(new RegisterRequest()
        {
            Name = name,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        var response = await userService.AuthenticateAsync(new AuthenticateRequest()
        {
            Name = name,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        await userService.RevokeTokenAsync(new RevokeTokenRequest()
        {
            Token = response.Token,
        }, new Metadata()
        {
            {ITokenService.TokenKey, response.Token}
        });

        var dataContext = Factory.Services.GetRequiredService<UsersDataContext>();
        var loginItem = await dataContext.Users
            .Where(x => x.Name == name)
            .LeftJoin(dataContext.LoginItems,
                (user, item) => user.Id == item.UserId,
                (user, item) => item)
            .SingleOrDefaultAsync();

        Assert.NotNull(loginItem);
        Assert.True(loginItem!.IsRevoked);
    }

    [Fact]
    public async Task Should_Not_Register_Duplicates()
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();
        var name = Faker.Random.String2(10);

        await userService.RegisterAsync(new RegisterRequest()
        {
            Name = name,
            Password = "password"
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.RegisterAsync(new RegisterRequest()
            {
                Name = name,
                Password = "password"
            }, new Metadata()
            {
                {ITokenService.SkipTokenVerificationKey, bool.TrueString}
            });
        });
    }

    [Fact]
    public async Task Should_Refresh_Token()
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();
        var name = Faker.Random.String2(10);
        var password = Faker.Random.String2(10);

        await userService.RegisterAsync(new RegisterRequest()
        {
            Name = name,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        var response = await userService.AuthenticateAsync(new AuthenticateRequest()
        {
            Name = name,
            Password = password
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        var refreshResponse = await userService.RefreshTokenAsync(new RefreshTokenRequest()
        {
            RefreshToken = response.RefreshToken
        }, new Metadata()
        {
            {ITokenService.SkipTokenVerificationKey, bool.TrueString}
        });

        Assert.NotNull(refreshResponse);
        Assert.NotEqual(response.Token, refreshResponse.Token);

        var dataContext = Factory.Services.GetRequiredService<UsersDataContext>();
        var oldLoginItem =
            await dataContext.LoginItems.SingleOrDefaultAsync(x => x.RefreshToken == response.RefreshToken);
        Assert.NotNull(oldLoginItem);
        Assert.True(oldLoginItem.IsRevoked);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task AuthenticateRequest_Should_Respect_Name(string name)
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.AuthenticateAsync(new AuthenticateRequest()
                {
                    Name = name
                },
                new Metadata()
                {
                    {ITokenService.SkipTokenVerificationKey, bool.TrueString}
                });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task AuthenticateRequest_Should_Respect_Password(string password)
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.AuthenticateAsync(new AuthenticateRequest()
                {
                    Password = password
                },
                new Metadata()
                {
                    {ITokenService.SkipTokenVerificationKey, bool.TrueString}
                });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task RegisterRequest_Should_Respect_Name(string name)
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.RegisterAsync(new RegisterRequest()
                {
                    Name = name
                },
                new Metadata()
                {
                    {ITokenService.SkipTokenVerificationKey, bool.TrueString}
                });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task RegisterRequest_Should_Respect_Password(string password)
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.RegisterAsync(new RegisterRequest()
                {
                    Password = password
                },
                new Metadata()
                {
                    {ITokenService.SkipTokenVerificationKey, bool.TrueString}
                });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RevokeToken_Should_Respect_Token(string token)
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.RevokeTokenAsync(new RevokeTokenRequest()
                {
                    Token = token
                },
                new Metadata()
                {
                    {ITokenService.SkipTokenVerificationKey, bool.TrueString}
                });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RefreshToken_Should_Respect_Token(string token)
    {
        ConfigureFactory(svc => { });
        MigrateUp();
        var userService = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await userService.RefreshTokenAsync(new RefreshTokenRequest()
                {
                    RefreshToken = token
                },
                new Metadata()
                {
                    {ITokenService.SkipTokenVerificationKey, bool.TrueString}
                });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
}