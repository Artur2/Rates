using LinqToDB;
using Rates.Background.Data;

namespace Rates.Background.Services;

public class BackgroundService(BackgroundDataContext backgroundDataContext) : IBackgroundService
{
    public async Task ProcessNewRecords((string charCode, double amount)[] records, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var itemsAsQueryable = records.AsQueryable(backgroundDataContext);

        await backgroundDataContext.Currencies.LeftJoin(itemsAsQueryable,
                (currency, newData) => currency.Name == newData.charCode,
                (currency, newData) => new {currency, newData})
            .AsUpdatable()
            .Set(x => x.currency.Rate, p => (decimal) p.newData.amount)
            .UpdateAsync(token);
    }
}