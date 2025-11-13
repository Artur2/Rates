namespace Rates.Domain.Entities;

public class LoginItem
{
    public int Id { get; set; }

    public string Identifier { get; set; } = string.Empty;

    public int UserId { get; set; }
    
    public bool IsRevoked { get; set; }

    public string RefreshToken { get; set; } = null!;
}