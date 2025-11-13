using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Rates.Shared.Grpc;

public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException ex)
        {
            throw new RpcException(ex.Status, ex.Message);
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred on the server.", ex));
        }
    }
}