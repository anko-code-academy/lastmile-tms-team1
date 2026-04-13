using FluentValidation;
using LastMile.TMS.Application.Common.Behaviors;
using LastMile.TMS.Application.Features.Bins.Services;
using LastMile.TMS.Application.Features.Routes.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<Domain.Services.IDeliveryDateCalculator, Domain.Services.DeliveryDateCalculator>();
        services.AddScoped<IRouteStopOptimizer, RouteStopOptimizer>();
        services.AddScoped<IBinAssignmentService, BinAssignmentService>();

        return services;
    }
}
