using Carter;
using RobotCodingPlatform.Web.Features.Profile.Get;
using RobotCodingPlatform.Web.Features.Profile.SoftDelete;
using RobotCodingPlatform.Web.Features.Profile.Update;
using System.Security.Claims;

namespace RobotCodingPlatform.Web.Features.Profile;

public class ProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var meGroup = app.MapGroup("api/me").WithTags("Me");
        var profileGroup = app.MapGroup("api/profile").WithTags("Profile");

        meGroup.MapGet("/", async (HttpContext httpContext, IProfileService service) =>
        {
            if (!TryGetUserId(httpContext, out var userId))
            {
                return Results.Unauthorized();
            }

            var user = await service.GetUserByIdAsync(userId);

            return user != null
                ? Results.Ok(new
                {
                    user.Name,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithSummary("get current logged user");

        meGroup.MapPost("/update-name",
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

        meGroup.MapPost("/update-role",
            async (HttpContext httpContext,
                UpdateRoleRequest request,
                IUpdateRoleService service) =>
        {
            if (!httpContext.User.Identity!.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            if (!TryGetUserId(httpContext, out var userId))
            {
                return Results.Forbid();
            }

            // TODO: убрать, вынести в админ-панель

            var result = await service.UpdateRoleAsync(userId, request.Role);

            return result
                ? Results.Ok(new { Success = true })
                : Results.Forbid();
        })
        .RequireAuthorization()
        .WithSummary("Update user role (Admin only)");

        meGroup.MapDelete("/delete", async (HttpContext httpContext, ISoftDeleteService service) =>
        {
            if (!httpContext.User.Identity!.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            if (!TryGetUserId(httpContext, out var userId))
            {
                return Results.Forbid();
            }

            var result = await service.DeleteAsync(userId);

            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization()
        .WithSummary("Delete current user (soft delete)");

        profileGroup.MapGet("{userId}", async (
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
