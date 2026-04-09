using FluentAssertions;
using FluentValidation.TestHelper;
using LastMile.TMS.Application.Features.Routes.Commands;
using LastMile.TMS.Application.Features.Routes.Validators;

namespace LastMile.TMS.Application.Tests;

public class OptimizeRouteStopOrderCommandValidatorTests
{
    private readonly OptimizeRouteStopOrderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_NoErrors()
    {
        var command = new OptimizeRouteStopOrderCommand(Guid.NewGuid());
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_EmptyRouteId_HasValidationError()
    {
        var command = new OptimizeRouteStopOrderCommand(Guid.Empty);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.RouteId);
    }
}
