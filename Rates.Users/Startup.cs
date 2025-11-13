using FluentMigrator.Runner;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Rates.Migration.Shared;
using Rates.Shared;
using Rates.Shared.Data;
using Rates.Shared.Grpc;
using Rates.Users.Data;

namespace Rates.Users;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddGrpc(static options =>
        {
            options.Interceptors.Add<IncomingRequestInterceptor>();
            options.Interceptors.Add<ExceptionInterceptor>();
        });
        services.AddGrpcReflection();
        var connectionString = Configuration.GetConnectionString("Default");
        DataConnection.DefaultSettings = new PosgresConnectionSettings(connectionString);
        services.AddScoped<UsersDataContext>();
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
            endpoints.MapGrpcService<Services.UsersService>();
            endpoints.MapGrpcReflectionService();
        });
    }
}