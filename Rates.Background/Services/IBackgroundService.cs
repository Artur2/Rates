using Rates.Background.Models;

namespace Rates.Background.Services;

public interface IBackgroundService
{
    Task ProcessNewRecords(CbrEntry[] records, CancellationToken cancellationToken);

    Task<CbrEntry[]> ParseCbrRecords(Stream stream, CancellationToken cancellationToken);
}