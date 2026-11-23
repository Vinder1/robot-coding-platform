using Carter;
using RobotCodingPlatform.Web.Features.Challenge.Create;

namespace RobotCodingPlatform.Web.Features.Challenge;

public class ChallengeEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/challenges").WithTags("Challenges");

        group.MapGet("/", async (
            IChallengeRepository repository,
            CancellationToken ct) =>
        {
            var challenges = await repository.GetAllAsync(ct);
            return Results.Ok(challenges.Select(c => c.ToSummaryDto()));
        })
        .WithSummary("Get all challenges");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IChallengeRepository repository,
            CancellationToken ct) =>
        {
            var challenge = await repository.GetByIdAsync(id, ct);

            return challenge is null
                ? Results.NotFound()
                : Results.Ok(challenge.ToDto());
        })
        .WithSummary("Get challenge by id");

        group.MapPost("/", async (
            CreateChallengeRequest request,
            ICreateChallengeService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);

            return result.Success
                ? Results.Created($"/api/challenges/{result.Challenge!.Id}", result.Challenge)
                : Results.BadRequest(new { result.Errors });
        })
        .RequireAuthorization("RequireAdminRole")
        .WithSummary("Create challenge (admin only)");
    }
}
