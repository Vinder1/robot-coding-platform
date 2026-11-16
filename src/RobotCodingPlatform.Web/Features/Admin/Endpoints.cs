using System.Security.Claims;
using Carter;
using RobotCodingPlatform.Web.Features.Admin.UpdateRole;

namespace RobotCodingPlatform.Web.Features.Admin;

public class AdminEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/users").WithTags("Admin")
            .RequireAuthorization("RequireAdminRole");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IAdminUserRepository adminRepository,
            HttpContext httpContext) =>
        {
            var user = await adminRepository.FindByIdAsync(id);

            if (user == null)
            {
                return Results.NotFound();
            }

            // Возвращаем полную информацию, включая DeletedAt если есть
            return Results.Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.CreatedAt,
                user.UpdatedAt,
                user.DeletedAt,
                PasswordHash = "****" // Пароль не передаём
            });
        })
        .WithSummary("Find user by ID (admin only)");

        group.MapPost("/restore/{id:guid}", async (
            Guid id,
            IAdminUserRepository adminRepository,
            ILogger<AdminEndpoints> logger) =>
        {
            var user = await adminRepository.RestoreAsync(id);

            if (user == null)
            {
                return Results.NotFound();
            }

            logger.LogInformation("Пользователь {UserId} восстановлен", id);

            return Results.Ok(new
            {
                Success = true,
                Message = "User restored successfully",
                User = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    user.UpdatedAt,
                }
            });
        })
        .WithSummary("Restore deleted user (admin only)");

        group.MapPost("/change-role/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            ChangeRoleRequest request,
            IChangeRoleService service) =>
        {
            if (!httpContext.User.Identity!.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var result = await service.ChangeRoleAsync(id, request.Role);

            return result
                ? Results.Ok(new { Success = true })
                : Results.Forbid();
        })
        .RequireAuthorization()
        .WithSummary("Update user role (Admin only)");
    }
}
