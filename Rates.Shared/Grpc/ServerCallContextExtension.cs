using System.Security.Claims;
using Grpc.Core;
using Rates.Shared.Services;

namespace Rates.Shared.Grpc;

public static class ServerCallContextExtension
{
    public static async Task<string> GetCallerName(this ServerCallContext context, ITokenService tokenService)
    {
        var token = context.RequestHeaders.GetValue(ITokenService.TokenKey);
        if (token == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthenticated"));
        }

        return await tokenService.GetEncryptedClaim(token, ClaimTypes.NameIdentifier);
    }
}