namespace LastMile.TMS.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetTokenAsync(string email, string resetToken, CancellationToken ct = default);
    Task SendUserCreatedAsync(string email, string temporaryPassword, CancellationToken ct = default);
}
