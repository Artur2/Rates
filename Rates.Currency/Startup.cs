using FluentMigrator.Runner;
using LinqToDB.Data;
using Rates.Currency.Data;
using Rates.Migration.Shared;
using Rates.Shared;
using Rates.Shared.Data;
using Rates.Shared.Grpc;

namespace Rates.Currency;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc(static options =>
        {
            options.Interceptors.Add<IncomingRequestInterceptor>();
            options.Interceptors.Add<ExceptionInterceptor>();
        });
        services.AddGrpcReflection();

        var connectionString = Configuration.GetConnectionString("Default");
        DataConnection.DefaultSettings = new PosgresConnectionSettings(connectionString);
        services.AddScoped<CurrencyDataContext>();
        services.AddSharedServices();

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test")
        {
            services.AddFluentMigratorCore()
                .ConfigureRunner(runner =>
                {
                    runner.AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(MarkingClass).Assembly).For.All();
                })
                .AddLogging(lb => lb.AddFluentMigratorConsole());
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<Rates.Currency.Services.CurrencyService>();
            endpoints.MapGrpcReflectionService();
        });
    }
}