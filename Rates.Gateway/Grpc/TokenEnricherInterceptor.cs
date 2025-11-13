using Grpc.Core;
using Grpc.Core.Interceptors;
using Rates.Shared.Services;

namespace Rates.Gateway.Grpc;

public class TokenEnricherInterceptor(IHttpContextAccessor httpContextAccessor) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        if (context.Options.Headers?.GetValue(ITokenService.SkipTokenVerificationKey) != null)
        {
            return base.AsyncUnaryCall(request, context, continuation);
        }

        if (!httpContextAccessor.HttpContext.Request.Headers.TryGetValue(ITokenService.TokenKey, out var values))
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Missing token"));
        }

        var token = values[0];
        if (token == null)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Missing token"));
        }

        var options = context.Options.WithHeaders(new Metadata()
        {
            {ITokenService.TokenKey, token}
        });

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);

        return base.AsyncUnaryCall(request, newContext, continuation);
    }
}