using Carter;
using RobotCodingPlatform.Web.Extensions;

namespace RobotCodingPlatform.Web.Features.Code;

public class CodeEndpoints : ICarterModule
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
            return ToHttpResult(res);
        })
        .RequireAuthorization()
        .RequireRateLimiting(RateLimitingExtensions.CodeExecutionPolicy)
        .WithSummary("Run given code with given params");

        group.MapPost("/run/challenge",
            async (RunChallengeCodeParams request,
            IRunCodeService service,
            CancellationToken token) =>
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return ToHttpResult(RunCodeResult.CompilationError("Код не может быть пустым."));
            }

            var res = await service.RunChallenge(request.ChallengeId, request.Code, token);

            if (res is null)
            {
                return Results.NotFound(new { message = "Испытание не найдено." });
            }

            return ToHttpResult(res);
        })
        .RequireAuthorization()
        .RequireRateLimiting(RateLimitingExtensions.CodeExecutionPolicy)
        .WithSummary("Run code for a saved challenge by its id");
    }

    private static IResult ToHttpResult(RunCodeResult result) => result.ErrorKind switch
    {
        RunCodeErrorKind.Compilation or RunCodeErrorKind.InvalidMap =>
            Results.BadRequest(result),
        RunCodeErrorKind.Timeout =>
            Results.Json(result, statusCode: StatusCodes.Status504GatewayTimeout),
        RunCodeErrorKind.WorkerError =>
            Results.Json(result, statusCode: StatusCodes.Status502BadGateway),
        _ => Results.Ok(result)
    };
}
