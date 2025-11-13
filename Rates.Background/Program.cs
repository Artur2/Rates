using System.Text;
using System.Text.Json;
using LinqToDB.Data;
using Quartz;
using Rates.Background.Data;
using Rates.Background.Jobs;
using Rates.Shared.Data;

namespace Rates.Background;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("Default");
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("cbr", client => client.BaseAddress = new Uri("https://www.cbr.ru"));
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        builder.Services.AddQuartz(qrtz =>
        {
            qrtz.UsePersistentStore(ps =>
            {
                ps.UsePostgres(connectionString);
                ps.UseNewtonsoftJsonSerializer();
            });

            qrtz.AddJob<FetchCurrencyJob>(job =>
            {
                job.WithIdentity(nameof(FetchCurrencyJob));
                job.StoreDurably();
            });

            qrtz.AddTrigger(trigger =>
            {
                trigger.StartNow()
                    .WithSimpleSchedule(sc => sc.WithIntervalInMinutes(1)
                        .RepeatForever())
                    .WithIdentity(nameof(FetchCurrencyJob))
                    .ForJob(nameof(FetchCurrencyJob));
            });
        });

        builder.Services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
        builder.Services.AddSingleton<FetchCurrencyJob>();
        DataConnection.DefaultSettings = new PosgresConnectionSettings(connectionString);
        builder.Services.AddTransient<BackgroundDataContext>();

        var app = builder.Build();

        app.Run();
    }
}