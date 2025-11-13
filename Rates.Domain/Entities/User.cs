namespace Rates.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;
}