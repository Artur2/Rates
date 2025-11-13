using Microsoft.AspNetCore.Mvc;
using Rates.Gateway.Dtos;
using Rates.Gateway.Services;

namespace Rates.Gateway.Controllers;

[ApiController]
[Route("api/v1/currencies")]
public class CurrenciesController(CurrenciesService currenciesService) : ControllerBase
{
    [HttpGet]
    public async Task<string[]> Get()
    {
        return await currenciesService.ListCurrencies();
    }

    [HttpPost]
    public async Task AddFavorite([FromBody] string name)
    {
        await currenciesService.AddFavorite(name);
    }

    [HttpPost("remove")]
    public async Task RemoveFavorite(string name)
    {
        await currenciesService.RemoveFavorite(name);
    }

    [HttpGet("favorites")]
    public async Task<FavoriteEntryDto[]> GetFavorites()
    {
        return await currenciesService.GetFavorites();
    }
}