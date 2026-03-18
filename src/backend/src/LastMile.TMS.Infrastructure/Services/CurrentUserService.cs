using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;

namespace LastMile.TMS.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

    public string? UserName =>
        httpContextAccessor.HttpContext?.User.FindFirst(OpenIddictConstants.Claims.Name)?.Value;
}