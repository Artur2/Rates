namespace Rates.Background.Services;

public interface IBackgroundService
{
    Task ProcessNewRecords((string charCode, double amount)[] records, CancellationToken token);
}