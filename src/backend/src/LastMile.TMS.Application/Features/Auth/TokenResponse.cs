namespace LastMile.TMS.Application.Features.Auth;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    long ExpiresIn)
{
    public static TokenResponse Empty => new(string.Empty, string.Empty, "Bearer", 0);
}