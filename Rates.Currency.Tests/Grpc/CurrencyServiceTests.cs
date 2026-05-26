using Grpc.Core;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rates.Currency;
using Rates.Currency.Data;
using Rates.Shared.Services;
using Xunit;

namespace Rates.Users.Tests.Grpc;

public class CurrencyServiceTests(WebApplicationFactory<Startup> factory) : TestClassBase(factory)
{
    [Fact]
    public async Task Should_List_Currencies()
    {
        ConfigureFactory();
        MigrateUp();

        var tokenService = Factory.Services.GetRequiredService<ITokenService>();
        var (token, _, _) = await tokenService.GenerateToken(Faker.Random.String(10));
        var dataContext = Factory.Services.GetRequiredService<CurrencyDataContext>();
        var client = CreateClient();

        var name = Faker.Random.String2(10);
        var rate = Faker.Random.Decimal();
        await dataContext.Currencies.InsertWithIdentityAsync(() => new Domain.Entities.Currency()
        {
            Name = name,
            Rate = rate,
        });

        var currencies = await client.ListCurrenciesAsync(new ListCurrenciesRequest(), new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        Assert.NotNull(currencies);
        Assert.True(currencies.Entries.Count > 1);
    }

    [Fact]
    public async Task Should_Add_Favorite_Currency()
    {
        ConfigureFactory();
        MigrateUp();

        var userName = Faker.Random.String2(10);
        var tokenService = Factory.Services.GetRequiredService<ITokenService>();
        var (token, _, _) = await tokenService.GenerateToken(userName);
        var dataContext = Factory.Services.GetRequiredService<CurrencyDataContext>();
        var client = CreateClient();

        var name = Faker.Random.String2(10);
        var rate = Faker.Random.Decimal();
        await dataContext.Currencies.InsertWithIdentityAsync(() => new Domain.Entities.Currency()
        {
            Name = name,
            Rate = rate,
        });

        var id = await dataContext.Users.InsertWithIdentityAsync(() => new Domain.Entities.User()
        {
            Name = userName,
            PasswordHash = new byte[] {0, 0, 0}
        });

        await client.AddFavoriteAsync(new AddFavoriteRequest()
        {
            Name = name
        }, new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        var favorites = await client.GetFavoritesAsync(new GetFavoritesRequest(), new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        Assert.NotNull(favorites);
        Assert.True(favorites.Entries.Count > 0);
    }

    [Fact]
    public async Task Should_Not_Add_Duplicate_Favorites()
    {
        ConfigureFactory();
        MigrateUp();

        var userName = Faker.Random.String2(10);
        var tokenService = Factory.Services.GetRequiredService<ITokenService>();
        var (token, _, _) = await tokenService.GenerateToken(userName);
        var dataContext = Factory.Services.GetRequiredService<CurrencyDataContext>();
        var client = CreateClient();

        var name = Faker.Random.String2(10);
        var rate = Faker.Random.Decimal();
        await dataContext.Currencies.InsertWithIdentityAsync(() => new Domain.Entities.Currency()
        {
            Name = name,
            Rate = rate,
        });

        await dataContext.Users.InsertWithIdentityAsync(() => new Domain.Entities.User()
        {
            Name = userName,
            PasswordHash = new byte[] {0, 0, 0}
        });

        await client.AddFavoriteAsync(new AddFavoriteRequest()
        {
            Name = name
        }, new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.AddFavoriteAsync(new AddFavoriteRequest()
            {
                Name = name
            }, new Metadata()
            {
                {ITokenService.TokenKey, token}
            });
        });
    }

    [Fact]
    public async Task Should_Remove_Favorite_Currency()
    {
        ConfigureFactory();
        MigrateUp();

        var userName = Faker.Random.String2(10);
        var tokenService = Factory.Services.GetRequiredService<ITokenService>();
        var (token, _, _) = await tokenService.GenerateToken(userName);
        var dataContext = Factory.Services.GetRequiredService<CurrencyDataContext>();
        var client = CreateClient();

        var name = Faker.Random.String2(10);
        var rate = Faker.Random.Decimal();
        await dataContext.Currencies.InsertWithIdentityAsync(() => new Domain.Entities.Currency()
        {
            Name = name,
            Rate = rate,
        });

        await dataContext.Users.InsertWithIdentityAsync(() => new Domain.Entities.User()
        {
            Name = userName,
            PasswordHash = new byte[] {0, 0, 0}
        });

        await client.AddFavoriteAsync(new AddFavoriteRequest()
        {
            Name = name
        }, new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        var favorites = await client.GetFavoritesAsync(new GetFavoritesRequest(), new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        Assert.NotNull(favorites);
        Assert.True(favorites.Entries.Count > 0);

        await client.RemoveFavoriteAsync(new RemoveFavoriteRequest()
        {
            Name = name
        }, new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        var favoritesAfterRemoving = await client.GetFavoritesAsync(new GetFavoritesRequest(), new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        Assert.NotNull(favoritesAfterRemoving);
        Assert.Empty(favoritesAfterRemoving.Entries);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    public async Task AddFavorite_Should_Respect_Name(string name)
    {
        ConfigureFactory();
        MigrateUp();

        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.AddFavoriteAsync(new AddFavoriteRequest()
            {
                Name = name
            }, new Metadata()
            {
                {ITokenService.SkipTokenVerificationKey, bool.TrueString}
            });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    public async Task RemoveFavorite_Should_Respect_Name(string name)
    {
        ConfigureFactory();
        MigrateUp();

        var client = CreateClient();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.RemoveFavoriteAsync(new RemoveFavoriteRequest()
            {
                Name = name
            }, new Metadata()
            {
                {ITokenService.SkipTokenVerificationKey, bool.TrueString}
            });
        });

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
}