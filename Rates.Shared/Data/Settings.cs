using LinqToDB;
using LinqToDB.Configuration;

namespace Rates.Shared.Data;

public class ConnectionStringSettings : IConnectionStringSettings
{
    public string ConnectionString { get; set; }
    public string Name { get; set; }
    public string ProviderName { get; set; }

    public bool IsGlobal => false;
}

public class PosgresConnectionSettings(string connectionString) : ILinqToDBSettings
{
    public IEnumerable<IDataProviderSettings> DataProviders
        => Enumerable.Empty<IDataProviderSettings>();

    public string DefaultConfiguration => "PostgreSQL";
    public string DefaultDataProvider => "PostgreSQL";

    public IEnumerable<IConnectionStringSettings> ConnectionStrings
    {
        get
        {
            yield return
                new ConnectionStringSettings
                {
                    Name = "Rates",
                    ProviderName = ProviderName.PostgreSQL,
                    ConnectionString = connectionString
                };
        }
    }
}