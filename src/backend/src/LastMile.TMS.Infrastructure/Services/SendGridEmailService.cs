using SendGrid;
using SendGrid.Helpers.Mail;
using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LastMile.TMS.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
    {
        var apiKey = configuration["Email:SendGridApiKey"]
            ?? throw new InvalidOperationException("Email:SendGridApiKey is not configured");
        _client = new SendGridClient(apiKey);
        _senderEmail = configuration["Email:SenderEmail"] ?? "noreply@lastmile.com";
        _senderName = configuration["Email:SenderName"] ?? "Last Mile TMS";
        _logger = logger;
    }

    public async Task SendPasswordResetTokenAsync(string email, string resetToken, CancellationToken ct = default)
    {
        var subject = "Password Reset - Last Mile TMS";
        var htmlContent = $@"
            <h1>Password Reset Request</h1>
            <p>You requested a password reset for your Last Mile TMS account.</p>
            <p>Your reset token: <strong>{resetToken}</strong></p>
            <p>Enter this token in the application to set a new password.</p>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br/>Last Mile TMS Team</p>";

        await SendEmailAsync(email, subject, htmlContent, ct);
    }

    public async Task SendUserCreatedAsync(string email, string temporaryPassword, CancellationToken ct = default)
    {
        var subject = "Welcome to Last Mile TMS - Account Created";
        var htmlContent = $@"
            <h1>Welcome to Last Mile TMS</h1>
            <p>Your account has been created successfully.</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
            <p>Please login and change your password immediately.</p>
            <p>Best regards,<br/>Last Mile TMS Team</p>";

        await SendEmailAsync(email, subject, htmlContent, ct);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken ct)
    {
        var from = new EmailAddress(_senderEmail, _senderName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);

        var response = await _client.SendEmailAsync(msg, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        else
        {
            var statusCode = (int)response.StatusCode;
            var body = await response.Body.ReadAsStringAsync(ct);
            _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Body: {Body}",
                toEmail, statusCode, body);
        }
    }
}
