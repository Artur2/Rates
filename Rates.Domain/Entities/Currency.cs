namespace Rates.Domain.Entities;

public class Currency
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    
    public decimal Rate { get; set; }
}