using HotChocolate;
using HotChocolate.Execution;
using Serilog;

namespace LastMile.TMS.Api.GraphQL.Common;

public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is not null)
        {
            var exceptionType = error.Exception.GetType().Name;
            var traceId = Guid.NewGuid().ToString("N")[..8];

            Log.Error(
                error.Exception,
                "GraphQL error {TraceId}: {ExceptionType} - {Message}",
                traceId,
                exceptionType,
                error.Exception.Message);

            return error.WithMessage($"An error occurred. Reference: {traceId}");
        }

        return error;
    }
}
