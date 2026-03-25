namespace LastMile.TMS.Application.Common.Interfaces;

public interface ITokenRevocationService
{
    Task RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
