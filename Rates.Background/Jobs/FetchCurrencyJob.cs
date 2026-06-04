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
        var xmlReader = XmlReader.Create(stream);
        await xmlReader.MoveToContentAsync();
        var charCode = string.Empty;
        var enteredCharCode = false;
        var enteredValue = false;
        var items = new List<(string charCode, double value)>();

        while (await xmlReader.ReadAsync())
        {
            // Помечаем как вошли в тег
            if (xmlReader is {NodeType: XmlNodeType.Element, Name: "CharCode"})
            {
                enteredCharCode = true;
            }
            // Помечаем как вошли в тег
            else if (xmlReader is {NodeType: XmlNodeType.Element, Name: "Value"})
            {
                enteredValue = true;
            }

            if (xmlReader.NodeType == XmlNodeType.Text && enteredCharCode)
            {
                charCode = xmlReader.Value;
                enteredCharCode = false;
            }

            if (xmlReader.NodeType == XmlNodeType.Text && enteredValue)
            {
                var converted = Convert.ToDouble(xmlReader.Value, CultureInfo.GetCultureInfo("Ru-ru"));
                items.Add((charCode, converted));
                enteredValue = false;

                charCode = string.Empty;
            }
        }

        await backgroundService.ProcessNewRecords(items.ToArray(), context.CancellationToken);
    }
}