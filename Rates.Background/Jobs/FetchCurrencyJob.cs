using System.Globalization;
using System.Xml;
using LinqToDB;
using Quartz;
using Rates.Background.Data;
using Rates.Background.Services;

namespace Rates.Background.Jobs;

[DisallowConcurrentExecution]
public class FetchCurrencyJob(IHttpClientFactory httpClientFactory, IBackgroundService backgroundService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var client = httpClientFactory.CreateClient("cbr");
        await using var stream = await client.GetStreamAsync("scripts/XML_daily.asp");
        var items = await backgroundService.ParseCbrRecords(stream, context.CancellationToken);
        await backgroundService.ProcessNewRecords(items, context.CancellationToken);
    }
}