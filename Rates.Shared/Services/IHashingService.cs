namespace Rates.Shared.Services;

public interface IHashingService
{
    byte[] ComputeHash(string input);
}