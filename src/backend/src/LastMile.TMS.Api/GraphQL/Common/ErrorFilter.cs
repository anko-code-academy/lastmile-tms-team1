using HotChocolate;
using HotChocolate.Execution;
using Serilog;

namespace LastMile.TMS.Api.GraphQL.Common;

public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        // Skip errors already handled by a domain-specific filter (they have a custom code set)
        if (error.Exception is not null && string.IsNullOrEmpty(error.Code))
        {
            var exceptionType = error.Exception.GetType().Name;
            var traceId = Guid.NewGuid().ToString("N")[..8];

            Log.Error(
                error.Exception,
                "GraphQL error {TraceId}: {ExceptionType} - {Message}",
                traceId,
                exceptionType,
                error.Exception.Message);

            // DomainExceptionErrorFilter sets error.Code for domain/business rule exceptions.
            // These are safe to expose to clients as they contain business logic validation messages
            // (e.g., "Cannot edit parcel in status Loaded") rather than internal implementation details.
            if (!string.IsNullOrEmpty(error.Code))
            {
                return error.WithMessage($"{error.Message}. Reference: {traceId}");
            }

            // For unhandled exceptions (no Code), mask the message to avoid exposing internal details
            return error.WithMessage($"An error occurred. Reference: {traceId}").WithCode("INTERNAL_ERROR");
        }

        return error;
    }
}
