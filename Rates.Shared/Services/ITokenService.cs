namespace Rates.Shared.Services;

public interface ITokenService
{
    public const string TokenKey = "x-u2s-token";

    /// <summary>
    /// Используется только gateway-ем, который определяет, нужно ли проверять токен
    /// </summary>
    public const string SkipTokenVerificationKey = "x-u2s-skip-verification";
    
    Task<(string token, string identifier, string refreshToken)> GenerateToken(string name);

    Task<bool> IsValidToken(string token);

    Task<bool> IsRevoked(string identifier);

    Task<string> GetEncryptedClaim(string token, string claim);
}