using Rates.Background.Models;

namespace Rates.Background.Services;

public interface IBackgroundService
{
    Task ProcessNewRecords(CbrEntry[] records, CancellationToken token);

    Task<CbrEntry[]> ParseCbrRecords(Stream stream, CancellationToken cancellationToken);
}