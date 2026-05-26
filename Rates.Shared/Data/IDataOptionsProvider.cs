using LinqToDB;

namespace Rates.Shared.Data;

public interface IDataOptionsProvider
{
     DataOptions GetDataOptions();
}