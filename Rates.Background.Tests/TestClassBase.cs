using Bogus;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Rates.Background.Tests;

public class TestClassBase(WebApplicationFactory<Startup> factory) : IClassFixture<WebApplicationFactory<Startup>>
{
    private WebApplicationFactory<Startup> _factory = factory;

    public WebApplicationFactory<Startup> Factory => _factory;

    public Faker Faker = new Faker();

    protected void ConfigureFactory(Action<IServiceCollection>? configure = null)
    {
        _factory = new WebApplicationFactory<Startup>()
            .WithWebHostBuilder(cfg =>
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
                if (configure != null)
                {
                    cfg.ConfigureServices(configure);
                }

                cfg.UseSetting("ConnectionStrings:Default", "Data Source=Application.db;Cache=Shared");
            });
    }

    protected void MigrateUp()
    {
        var migrationRunner = Factory.Services.GetRequiredService<IMigrationRunner>();
        migrationRunner.MigrateUp();
    }
}