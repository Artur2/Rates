using System.Buffers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using LinqToDB;
using Microsoft.IdentityModel.Tokens;
using Rates.Shared.Data;

namespace Rates.Shared.Services;

/// <summary>
/// Сервис по работе с токенами
/// </summary>
/// <remarks>
/// Приветный ключ для создания, публичный для валидации
/// По хорошему хранить это в другом месте
/// </remarks>
public class TokenService(SharedDataContext sharedDataContext) : ITokenService
{
    public async Task<(string token, string identifier, string refreshToken)> GenerateToken(string name)
    {
        var rsaDataText = await File.ReadAllTextAsync("key");
        var identifier = Guid.NewGuid().ToString();
        using var privateKey = RSA.Create();
        privateKey.ImportFromPem(rsaDataText);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, name),
            new Claim(JwtRegisteredClaimNames.Jti, identifier)
        };

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: "Rates",
            audience: "Rates",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: new SigningCredentials(new RsaSecurityKey(privateKey), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory
                {
                    CacheSignatureProviders = false
                }
            }
        );

        // конвертируем токен в строку
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        using var rngCryptoServiceProvider = RandomNumberGenerator.Create();
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            rngCryptoServiceProvider.GetNonZeroBytes(buffer);
            var refreshToken = Convert.ToHexStringLower(buffer);
            return (token, identifier, refreshToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task<bool> IsValidToken(string token)
    {
        var rsaData = await File.ReadAllTextAsync("key.pub.pem");
        using var publicKey = RSA.Create();
        publicKey.ImportFromPem(rsaData); // Лучше конечно складывать в кеш, а лучше в vault

        var tokenValidationParameters = CreateParameters(publicKey);

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        _ = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
        return validatedToken.ValidTo > DateTime.UtcNow;
    }

    public async Task<bool> IsRevoked(string identifier)
    {
        return await sharedDataContext.LoginItems.AnyAsync(x => x.Identifier == identifier && x.IsRevoked == true);
    }

    public async Task<string> GetEncryptedClaim(string token, string claim)
    {
        var rsaData = await File.ReadAllTextAsync("key.pub.pem");
        using var publicKey = RSA.Create();
        publicKey.ImportFromPem(rsaData); // Лучше конечно складывать в кеш, а лучше в vault

        var tokenValidationParameters = CreateParameters(publicKey);

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var result = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out _);

        return result?.Claims.FirstOrDefault(c => c.Type == claim)?.Value ??
               throw new InvalidOperationException("Cannot decrypt claim");
    }

    private static TokenValidationParameters CreateParameters(RSA key) => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Rates",
        ValidAudience = "Rates",
        IssuerSigningKey = new RsaSecurityKey(key),
        CryptoProviderFactory = new CryptoProviderFactory
        {
            CacheSignatureProviders = false
        }
    };
}