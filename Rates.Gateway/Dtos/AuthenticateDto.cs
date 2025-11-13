namespace Rates.Gateway.Dtos;

public class AuthenticateDto
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}