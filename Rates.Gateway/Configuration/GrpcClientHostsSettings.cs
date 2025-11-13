namespace Rates.Gateway.Configuration;

public class GrpcClientHostsSettings
{
    public string UsersService { get; set; }
    
    public string MigrationService { get; set; }
    
    public string CurrencyService { get; set; }
}