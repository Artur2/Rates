using Microsoft.Extensions.DependencyInjection;
using Rates.Shared.Data;
using Rates.Shared.Services;

namespace Rates.Shared;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddScoped<SharedDataContext>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHashingService, HashingService>();

        return services;
    }
}