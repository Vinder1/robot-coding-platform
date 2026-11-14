using Carter;
using RobotCodingPlatform.Web.Features.Profile.Update;
using RobotCodingPlatform.Web.Features.Users;
using System.Security.Claims;

namespace RobotCodingPlatform.Web.Features.Profile;

public class ProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/profile").WithTags("Profile");

        app.MapGet("/api/me", async (HttpContext httpContext, IUserRepository userRepository) =>
        {
            if (!TryGetUserId(httpContext, out var userId))
            {
                return Results.Unauthorized();
            }

            var user = await userRepository.FindAsync(userId);

            return user != null 
                ? Results.Ok(new
                {
                    user.Name,
                    user.Email,
                    user.CreatedAt,
                }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithSummary("get current logged user");

        app.MapPost("/api/me/update-name",
            async (HttpContext httpContext,
            UpdateNameRequest request,
            IUpdateProfileService service) =>
        {
            if (!httpContext.User.Identity!.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            if (!TryGetUserId(httpContext, out var userId))
            {
                return Results.Forbid();
            }

            var updatedUser = await service.UpdateNameAsync(userId, request);

            return updatedUser.Success
                ? Results.NotFound(updatedUser)
                : Results.Ok(updatedUser);
        })
        .RequireAuthorization()
        .WithSummary("Update profile name");

        group.MapGet("{userId}", async (
            Guid userId, 
            HttpContext httpContext, 
            IProfileService service) =>
        {
            var userProfile = await service.GetUserByIdAsync(userId);
            return userProfile != null 
                ? Results.Ok(new
                {
                    userProfile.Name,
                    userProfile.CreatedAt
                }) : Results.NotFound();
        })
        .WithSummary("get someones profile");
    }

    private static bool TryGetUserId(HttpContext httpContext, out Guid userId)
    {
        var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null || !Guid.TryParse(userIdStr, out userId))
        {
            userId = Guid.Empty;
            return false;
        }

        return true;
    }
}
