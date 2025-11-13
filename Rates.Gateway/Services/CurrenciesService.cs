using Rates.Currency;
using Rates.Gateway.Dtos;

namespace Rates.Gateway.Services;

public class CurrenciesService(CurrencyService.CurrencyServiceClient client)
{
    public async Task<string[]> ListCurrencies()
    {
        var currencies = await client.ListCurrenciesAsync(new ListCurrenciesRequest());
        return currencies.Entries.ToArray();
    }

    public async Task AddFavorite(string name)
    {
        await client.AddFavoriteAsync(new AddFavoriteRequest()
        {
            Name = name,
        });
    }

    public async Task RemoveFavorite(string name)
    {
        await client.RemoveFavoriteAsync(new RemoveFavoriteRequest()
        {
            Name = name
        });
    }

    public async Task<FavoriteEntryDto[]> GetFavorites()
    {
        var favorite = await client.GetFavoritesAsync(new GetFavoritesRequest());
        return favorite.Entries.Select(x => new FavoriteEntryDto()
        {
            Name = x.Name,
            Rate = x.Rate,
        }).ToArray();
    }
}