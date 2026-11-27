using Carter;
using RobotCodingPlatform.Web.Features.Challenge.Create;
using RobotCodingPlatform.Web.Features.Challenge.Rating;

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

        group.MapGet("/{id:guid}/rating", async (
            Guid id,
            int? limit,
            IChallengeRatingRepository repository,
            CancellationToken ct) =>
        {
            var rating = await repository.GetAsync(id, limit ?? 10, ct);

            return rating is null
                ? Results.NotFound()
                : Results.Ok(rating);
        })
        .WithSummary("Get challenge rating: top users by the fewest interpreter iterations");

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
