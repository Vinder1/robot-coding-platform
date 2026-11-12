using System.Security.Claims;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using RobotCodingPlatform.Web.Features.Auth.LogIn;
using RobotCodingPlatform.Web.Features.Auth.Logout;
using RobotCodingPlatform.Web.Features.Auth.SignIn;

namespace RobotCodingPlatform.Web.Features.Auth;

public class AuthEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth").WithTags("Auth");

        group.MapPost("/signin", async (
            [FromBody] SignInRequest request,
            HttpContext httpContext,
            ISignInService service,
            IOptions<JwtSettings> jwtSettings) =>
        {
            var res = await service.SignInAsync(request);
            if (!res.Success)
            {
                return Results.BadRequest(res);
            }
            httpContext.Response.SetTokensCookie(res.NewAccessToken, res.NewRefreshToken, jwtSettings.Value);
            return Results.Ok(res);
        }).WithSummary("Log In");

        group.MapPost("/login", async (
            [FromBody] LogInRequest request,
            HttpContext httpContext,
            ILogInService service,
            IOptions<JwtSettings> jwtSettings) =>
        {
            var res = await service.LoginAsync(request);
            if (!res.Success)
            {
                return Results.BadRequest(res);
            }
            httpContext.Response.SetTokensCookie(res.NewAccessToken, res.NewRefreshToken, jwtSettings.Value);
            return Results.Ok(res);
        }).WithSummary("Sign In");

        group.MapPost("/logout", async (
            HttpContext httpContext,
            ILogoutService service,
            ClaimsPrincipal userClaims,
            IOptions<JwtSettings> jwtSettings,
            CancellationToken ct) =>
        {
            var claimId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !Guid.TryParse(claimId, out var userId))
            {
                return Results.Unauthorized();
            }

            var refreshToken = httpContext.Request.GetRefreshTokenCookie(jwtSettings.Value);
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await service.LogoutAsync(refreshToken, userId, ct);
            }
            httpContext.Response.ClearTokensCookie(jwtSettings.Value);
            return Results.NoContent();
        }).WithSummary("Sign In");
    }
}