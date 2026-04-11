using System.Threading.RateLimiting;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace LastMile.TMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services
            .AddHttpClient<IGeocodingService, NominatimGeocodingService>()
            .AddResilienceHandler("nominatim-pipeline", builder =>
            {
                builder.AddRateLimiter(new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1,
                    Window = TimeSpan.FromSeconds(1),
                    QueueLimit = 10,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));

                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(500),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                                        || r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
                });

                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailService, SendGridEmailService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();
        services.AddScoped<IDbSeeder, DbSeeder>();
        services.AddScoped<ILabelService, LabelService>();

        return services;
    }
}
