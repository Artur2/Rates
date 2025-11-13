using FluentMigrator.Runner;
using Rates.Migration.Shared;

namespace Rates.Migration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        builder.Services.AddFluentMigratorCore()
            .ConfigureRunner(runner =>
            {
                runner.AddPostgres()
                    .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Default"))
                    .ScanIn(typeof(MarkingClass).Assembly).For.All();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<Services.MigrationService>();
        app.MapGrpcReflectionService();

        app.Run();
    }
}