using Grpc.Core;
using LinqToDB;
using Rates.Currency.Data;
using Rates.Domain.Entities;
using Rates.Shared.Grpc;
using Rates.Shared.Services;

namespace Rates.Currency.Services;

public class CurrencyService(CurrencyDataContext currencyDataContext, ITokenService tokenService)
    : Currency.CurrencyService.CurrencyServiceBase
{
    public override async Task<ListCurrenciesResponse> ListCurrencies(ListCurrenciesRequest request,
        ServerCallContext context)
    {
        var currencies = await currencyDataContext.Currencies.ToListAsync();
        return new ListCurrenciesResponse()
        {
            Entries = {currencies.Select(x => x.Name)}
        };
    }

    public override async Task<AddFavoriteResponse> AddFavorite(AddFavoriteRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));
        }

        var currency = await currencyDataContext.Currencies.SingleOrDefaultAsync(x => x.Name == request.Name);
        if (currency == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Currency not found"));
        }

        var callerName = await context.GetCallerName(tokenService);
        var user = currencyDataContext.Users.SingleOrDefault(x => x.Name == callerName);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        await currencyDataContext.FavoriteCurrencies.InsertAsync(() => new FavoriteCurrency()
        {
            CurrencyId = currency.Id,
            UserId = user.Id
        });

        return new AddFavoriteResponse();
    }

    public override async Task<RemoveFavoriteResponse> RemoveFavorite(RemoveFavoriteRequest request,
        ServerCallContext context)
    {
        var currency = await currencyDataContext.Currencies.SingleOrDefaultAsync(x => x.Name == request.Name);
        if (currency == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Currency not found"));
        }

        var callerName = await context.GetCallerName(tokenService);
        var user = currencyDataContext.Users.SingleOrDefault(x => x.Name == callerName);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        await currencyDataContext.FavoriteCurrencies.Where(x => x.UserId == user.Id && x.CurrencyId == currency.Id)
            .DeleteAsync();

        return new RemoveFavoriteResponse();
    }

    public override async Task<GetFavoritesResponse> GetFavorites(GetFavoritesRequest request,
        ServerCallContext context)
    {
        var callerName = await context.GetCallerName(tokenService);
        var user = currencyDataContext.Users.SingleOrDefault(x => x.Name == callerName);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        var favoriteCurrencies = await currencyDataContext.FavoriteCurrencies
            .Where(x => x.UserId == user.Id)
            .InnerJoin(currencyDataContext.Currencies, (fc, c) => fc.CurrencyId == c.Id, (fc, c) => c).ToListAsync();

        return new GetFavoritesResponse()
        {
            Entries =
            {
                favoriteCurrencies.Select(x => new GetFavoritesEntry()
                {
                    Name = x.Name,
                    Rate = (double) x.Rate
                })
            }
        };
    }
}