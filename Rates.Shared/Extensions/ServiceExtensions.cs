using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rates.Migration.Shared;

namespace Rates.Shared.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddMigration(this IServiceCollection services, IConfiguration configuration)
    {
        var isTestEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
        
        services.AddFluentMigratorCore()
            .ConfigureRunner(runner =>
            {
                var connectionString = configuration.GetConnectionString("Default");
                if (isTestEnv)
                {
                    runner.AddSQLite()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(MarkingClass).Assembly).For.All();
                }
                else
                {
                    runner.AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(MarkingClass).Assembly).For.All();
                }
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}