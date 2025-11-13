namespace Rates.Background.Settings;

public class QuartzSettings
{
    public string CronSchedule { get; set; } = null!;
    
    public string JobName { get; set; } = null!;
    
    public string JobGroup { get; set; } = null!;
}