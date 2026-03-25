using LastMile.TMS.Application.Common.Interfaces;
using OpenIddict.Abstractions;

namespace LastMile.TMS.Infrastructure.Services;

public class TokenRevocationService(
    IOpenIddictTokenManager tokenManager,
    IOpenIddictAuthorizationManager authorizationManager)
    : ITokenRevocationService
{
    public async Task RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Revoke all tokens for the user
        await tokenManager.RevokeBySubjectAsync(userId.ToString(), cancellationToken);

        // Revoke all authorizations for the user
        await authorizationManager.RevokeBySubjectAsync(userId.ToString(), cancellationToken);
    }
}
