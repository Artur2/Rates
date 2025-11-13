using LinqToDB.Data;
using Rates.Shared;
using Rates.Shared.Data;
using Rates.Shared.Grpc;
using Rates.Users.Data;

namespace Rates.Users;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(defaults => { defaults.UseStartup<Startup>(); })
            .Build()
            .Run();
    }
}