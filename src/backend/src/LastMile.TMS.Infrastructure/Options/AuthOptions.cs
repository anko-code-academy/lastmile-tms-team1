namespace LastMile.TMS.Infrastructure.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string AccessTokenLifetime { get; set; } = "01:00:00";
    public string RefreshTokenLifetime { get; set; } = "07:00:00";
    public DefaultAdminOptions DefaultAdmin { get; set; } = new();
}

public class DefaultAdminOptions
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}