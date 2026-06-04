using System.Text;
using Quartz;
using Rates.Background.Data;
using Rates.Background.Jobs;
using Rates.Background.Services;
using Rates.Shared.Data;
using Rates.Shared.Extensions;

namespace Rates.Background;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("Default");
        services.AddHttpClient();
        services.AddHttpClient("cbr", client => client.BaseAddress = new Uri("https://www.cbr.ru"));
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var isTestEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

        if (!isTestEnv)
        {
            services.AddQuartz(qrtz =>
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

            services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
        }

        services.AddSingleton<FetchCurrencyJob>();
        services.AddTransient<IDataOptionsProvider, DefaultDataOptions>();
        services.AddTransient<BackgroundDataContext>();
        services.AddTransient<IBackgroundService, Services.BackgroundService>();
        
        services.AddMigration(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}