using System.Globalization;
using System.Xml;
using LinqToDB;
using Rates.Background.Data;
using Rates.Background.Models;

namespace Rates.Background.Services;

public class BackgroundService(BackgroundDataContext backgroundDataContext) : IBackgroundService
{
    public async Task<CbrEntry[]> ParseCbrRecords(Stream stream, CancellationToken cancellationToken)
    {
        var xmlReader = XmlReader.Create(stream, new XmlReaderSettings()
        {
            Async = true
        });
        
        await xmlReader.MoveToContentAsync();
        var charCode = string.Empty;
        var enteredCharCode = false;
        var enteredValue = false;
        var items = new List<CbrEntry>();

        while (await xmlReader.ReadAsync())
        {
            // Помечаем как вошли в тег
            if (xmlReader is { NodeType: XmlNodeType.Element, Name: "CharCode" })
            {
                enteredCharCode = true;
            }
            // Помечаем как вошли в тег
            else if (xmlReader is { NodeType: XmlNodeType.Element, Name: "Value" })
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
                items.Add(new CbrEntry(charCode, converted));
                enteredValue = false;

                charCode = string.Empty;
            }
        }

        return items.ToArray();
    }

    public async Task ProcessNewRecords(CbrEntry[] records, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var itemsAsQueryable = records.AsQueryable(backgroundDataContext);

        await backgroundDataContext.Currencies.LeftJoin(itemsAsQueryable,
                (currency, newData) => currency.Name == newData.Code,
                (currency, newData) => new { currency, newData })
            .AsUpdatable()
            .Set(x => x.currency.Rate, p => (decimal)p.newData.Amount)
            .UpdateAsync(cancellationToken);
    }
}