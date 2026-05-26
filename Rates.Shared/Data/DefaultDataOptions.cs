using LinqToDB;
using Microsoft.Extensions.Configuration;
using Rates.Shared.Extensions;

namespace Rates.Shared.Data;

public class DefaultDataOptions(IConfiguration configuration) : IDataOptionsProvider
{
    public DataOptions GetDataOptions()
    {
        var connectionString = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException();
        var isTestEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

        if (!isTestEnv)
        {
            return new DataOptions()
                .UsePostgreSQL(connectionString);
        }

        return new DataOptions()
            .UseSQLite(connectionString);
    }
}