using FluentValidation;
using HotChocolate;
using HotChocolate.Execution;
using LastMile.TMS.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Common;

public class DomainExceptionErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        var exception = error.Exception;

        return exception switch
        {
            ValidationException validationEx => error.WithCode("VALIDATION_ERROR")
                .WithMessage(string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),

            System.Collections.Generic.KeyNotFoundException => error.WithCode("NOT_FOUND")
                .WithMessage("The requested resource was not found"),

            InvalidOperationException => error.WithCode("INVALID_OPERATION")
                .WithMessage(exception.Message),

            DbUpdateException => error.WithCode("DATABASE_ERROR")
                .WithMessage("A database error occurred"),

            UnauthorizedAccessException => error.WithCode("UNAUTHORIZED")
                .WithMessage("You are not authorized to perform this action"),

            _ => error
        };
    }
}
