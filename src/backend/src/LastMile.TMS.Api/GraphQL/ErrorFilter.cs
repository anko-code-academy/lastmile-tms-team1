using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.Logging;

namespace LastMile.TMS.Api.GraphQL;

public class ErrorFilter : IErrorFilter
{
    private readonly ILogger<ErrorFilter> _logger;

    public ErrorFilter(ILogger<ErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        if (error.Exception is not null)
        {
            var exceptionType = error.Exception.GetType().Name;
            var traceId = Guid.NewGuid().ToString("N")[..8];

            _logger.LogError(
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
