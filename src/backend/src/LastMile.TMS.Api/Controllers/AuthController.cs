using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LastMile.TMS.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace LastMile.TMS.Api.Controllers;

/// <summary>
/// Token endpoint - handles password and refresh token flows.
/// Uses OpenIddict for endpoint handling, but credentials validation
/// and token generation are done here using ASP.NET Core Identity.
/// </summary>
[ApiController]
[Route("connect")]
public class AuthController(
    UserManager<IdentityUser<Guid>> userManager,
    SignInManager<IdentityUser<Guid>> signInManager,
    IOptions<AuthOptions> authOptions) : ControllerBase
{
    /// <summary>
    /// Token endpoint - handles password flow
    /// </summary>
    [HttpPost("token")]
    public async Task<IActionResult> Token(
        [FromForm] string grant_type,
        [FromForm] string? username,
        [FromForm] string? password,
        [FromForm] string? refresh_token,
        [FromForm] string? scope,
        CancellationToken cancellationToken)
    {
        if (grant_type == "password")
        {
            return await HandlePasswordFlowAsync(username, password, scope, cancellationToken);
        }

        if (grant_type == "refresh_token")
        {
            return await HandleRefreshTokenFlowAsync(refresh_token, scope, cancellationToken);
        }

        return BadRequest(new
        {
            error = "unsupported_grant_type",
            error_description = "The grant type is not supported."
        });
    }

    private async Task<IActionResult> HandlePasswordFlowAsync(
        string? username,
        string? password,
        string? scope,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return BadRequest(new
            {
                error = "invalid_request",
                error_description = "Username and password are required."
            });
        }

        var user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            return Unauthorized(new
            {
                error = "invalid_grant",
                error_description = "Invalid credentials."
            });
        }

        var isValid = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!isValid.Succeeded)
        {
            return Unauthorized(new
            {
                error = "invalid_grant",
                error_description = "Invalid credentials."
            });
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var authCfg = authOptions.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authCfg.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: authCfg.Issuer,
            audience: authCfg.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.Parse(authCfg.AccessTokenLifetime)),
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate refresh token (simple random string for now)
        var refreshToken = Guid.NewGuid().ToString("N");

        return Ok(new
        {
            access_token = accessToken,
            refresh_token = refreshToken,
            token_type = "Bearer",
            expires_in = (int)TimeSpan.Parse(authCfg.AccessTokenLifetime).TotalSeconds,
            scope = scope ?? "api"
        });
    }

    private Task<IActionResult> HandleRefreshTokenFlowAsync(
        string? refresh_token,
        string? scope,
        CancellationToken cancellationToken)
    {
        // TODO: Implement refresh token validation and token regeneration
        return Task.FromResult<IActionResult>(Unauthorized(new
        {
            error = "invalid_grant",
            error_description = "Refresh token is not yet implemented."
        }));
    }
}