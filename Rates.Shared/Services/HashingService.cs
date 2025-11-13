using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace Rates.Shared.Services;

public class HashingService : IHashingService
{
    public byte[] ComputeHash(string input)
    {
        var rented = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(input));
        var resultingBytes = new byte[32];
        try
        {
            Encoding.UTF8.GetBytes(input, 0, input.Length, rented, 0);
            SHA256.HashData(rented, resultingBytes);
            return resultingBytes;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}