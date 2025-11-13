namespace Rates.Currency;

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