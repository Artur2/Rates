using Grpc.Net.ClientFactory;
using Rates.Currency;
using Rates.Gateway.Configuration;
using Rates.Gateway.Grpc;
using Rates.Gateway.Services;
using Rates.Migration;


namespace Rates.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var settings = builder.Configuration.GetSection(nameof(GrpcClientHostsSettings)).Get<GrpcClientHostsSettings>()
                       ?? throw new InvalidOperationException("Configure endpoints");

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();
        builder.Services.AddScoped<TokenEnricherInterceptor>();
        builder.Services.AddScoped<CurrenciesService>();
        builder.Services.AddGrpcClient<Users.UsersService.UsersServiceClient>(opts =>
        {
            opts.Address = new Uri(settings.UsersService);
        })
        .AddInterceptor<TokenEnricherInterceptor>();
        builder.Services.AddGrpcClient<MigrationService.MigrationServiceClient>(opts =>
        {
            opts.Address = new Uri(settings.MigrationService);
        })
        .AddInterceptor<TokenEnricherInterceptor>();
        builder.Services.AddGrpcClient<CurrencyService.CurrencyServiceClient>(opts =>
        {
            opts.Address = new Uri(settings.CurrencyService);
        })
        .AddInterceptor<TokenEnricherInterceptor>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddScoped<Services.UsersService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }
}