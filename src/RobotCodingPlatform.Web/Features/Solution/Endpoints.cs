using System.Security.Claims;
using Carter;
using RobotCodingPlatform.Web.Features.Solution.Create;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Solution;

public class SolutionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/solutions")
            .WithTags("Solutions")
            .RequireAuthorization();

        group.MapPost("/", async (
            HttpContext httpContext,
            CreateSolutionAttemptRequest request,
            ICreateSolutionAttemptService service,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var result = await service.CreateAsync(userId.Value, request, ct);

            return result.Status switch
            {
                CreateSolutionAttemptStatus.Created =>
                    Results.Created($"/api/solutions/{result.Attempt!.Id}", result.Attempt),
                CreateSolutionAttemptStatus.ChallengeNotFound =>
                    Results.NotFound(new { message = "Испытание не найдено." }),
                _ => Results.BadRequest(new { errors = result.Errors })
            };
        })
        .WithSummary("Submit a solution attempt for a challenge");

        group.MapGet("/", async (
            HttpContext httpContext,
            Guid? challengeId,
            bool? all,
            ISolutionAttemptRepository repository,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var isAdmin = httpContext.User.IsInRole(Roles.Admin);

            IReadOnlyList<SolutionAttempt> attempts;

            if (challengeId is null)
            {
                attempts = await repository.GetForUserAsync(null, userId.Value, ct);
            }
            else if (all == true && isAdmin)
            {
                attempts = await repository.GetForChallengeAsync(challengeId.Value, ct);
            }
            else
            {
                attempts = await repository.GetForUserAsync(challengeId, userId.Value, ct);
            }

            return Results.Ok(attempts.Select(a => a.ToSummaryDto()));
        })
        .WithSummary("List solution attempts of the current user (admins may request all for a challenge)");

        group.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            ISolutionAttemptRepository repository,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var attempt = await repository.GetByIdAsync(id, ct);
            if (attempt is null)
            {
                return Results.NotFound();
            }

            var isAdmin = httpContext.User.IsInRole(Roles.Admin);
            if (attempt.UserId != userId.Value && !isAdmin)
            {
                return Results.Forbid();
            }

            return Results.Ok(attempt.ToDto());
        })
        .WithSummary("Get a solution attempt by id");
    }

    private static Guid? GetUserId(HttpContext httpContext)
    {
        var value = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
