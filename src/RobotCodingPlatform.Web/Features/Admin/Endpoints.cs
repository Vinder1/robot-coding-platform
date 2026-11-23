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

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? search,
            bool? includeDeleted,
            IAdminUserRepository adminRepository,
            CancellationToken ct) =>
        {
            var currentPage = Math.Max(page ?? 1, 1);
            var currentPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

            var (items, total) = await adminRepository.GetUsersAsync(
                currentPage,
                currentPageSize,
                search,
                includeDeleted ?? false,
                ct);

            var totalPages = (int)Math.Ceiling(total / (double)currentPageSize);

            return Results.Ok(new
            {
                Items = items.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    u.CreatedAt,
                    u.UpdatedAt,
                    u.DeletedAt
                }),
                Page = currentPage,
                PageSize = currentPageSize,
                Total = total,
                TotalPages = totalPages
            });
        })
        .WithSummary("List users with pagination (admin only)");

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
