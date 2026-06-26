using Rates.Currency.Data;
using Rates.Shared;
using Rates.Shared.Data;
using Rates.Shared.Extensions;
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

        services.AddTransient<IDataOptionsProvider, DefaultDataOptions>();
        services.AddScoped<CurrencyDataContext>();
        services.AddSharedServices();
        services.AddMigration(Configuration);
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