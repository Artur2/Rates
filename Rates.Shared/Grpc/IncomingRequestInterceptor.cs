using System.IdentityModel.Tokens.Jwt;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Rates.Shared.Services;

namespace Rates.Shared.Grpc;

public class IncomingRequestInterceptor(ITokenService tokenService) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)

    {
        if (context.RequestHeaders?.GetValue(ITokenService.SkipTokenVerificationKey) == bool.TrueString)
        {
            return await base.UnaryServerHandler(request, context, continuation);
        }

        if (context.RequestHeaders?.Get(ITokenService.TokenKey) == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthenticated"));
        }

        var token = context.RequestHeaders.Get(ITokenService.TokenKey);
        if (token == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthenticated"));
        }

        var isValidToken = await tokenService.IsValidToken(token.Value);
        if (!isValidToken)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthenticated"));
        }

        var handler = new JwtSecurityTokenHandler();
        var deserializedToken = handler.ReadJwtToken(token.Value);
        if (await tokenService.IsRevoked(deserializedToken.Payload.Jti))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Revoked"));
        }

        return await base.UnaryServerHandler(request, context, continuation);
    }
}