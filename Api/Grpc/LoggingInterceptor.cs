using Grpc.Core;
using Grpc.Core.Interceptors;
using Application.Exceptions;
using FluentValidation;

namespace Api.Grpc;

public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("gRPC {Method} request: {Request}", context.Method, request);

        try
        {
            var response = await continuation(request, context);
            logger.LogInformation("gRPC {Method} response: {Response}", context.Method, response);
            return response;
        }
        catch (ValidationException exception)
        {
            var message = string.Join("; ", exception.Errors.Select(error => error.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, message));
        }
        catch (ProductNotFoundException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Request cancelled."));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled gRPC error in {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred."));
        }
    }
}
