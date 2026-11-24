using Carter;

namespace RobotCodingPlatform.Web.Features.Code;

public class ProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/code").WithTags("Code");

        group.MapPost("/run",
            async (HttpContext httpContext,
            RunCodeParams request,
            IRunCodeService service,
            CancellationToken token) =>
        {
            var res = await service.Run(request, token);
            return res.Compiled ? Results.Ok(res) : Results.BadRequest(res);
        })
        .RequireAuthorization()
        .WithSummary("Run given code with given params");

        group.MapPost("/run/challenge",
            async (RunChallengeCodeParams request,
            IRunCodeService service,
            CancellationToken token) =>
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return Results.BadRequest(new RunCodeResult
                {
                    Compiled = false,
                    Success = false,
                    Message = "Код не может быть пустым."
                });
            }

            var res = await service.RunChallenge(request.ChallengeId, request.Code, token);

            if (res is null)
            {
                return Results.NotFound(new { message = "Испытание не найдено." });
            }

            return res.Compiled ? Results.Ok(res) : Results.BadRequest(res);
        })
        .RequireAuthorization()
        .WithSummary("Run code for a saved challenge by its id");
    }
}
