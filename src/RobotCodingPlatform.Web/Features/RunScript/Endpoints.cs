using Carter;
using MassTransit;
using RobotCodingPlatform.Core.Contracts;

namespace RobotCodingPlatform.Web.Features.RunScript;

public class RunScriptEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/scripts").WithTags("Scripts");

        group.MapPost("/run", async (
                RunScriptRequest request,
                IRequestClient<RunScriptCommand> client,
                CancellationToken cancellationToken) =>
            {
                var command = new RunScriptCommand(
                    Code: request.Code,
                    Width: request.Width,
                    Height: request.Height,
                    Goal: (request.GoalX, request.GoalY),
                    Obstacles: request.Obstacles?.Select(o => (o.X, o.Y)).ToList() ?? new(),
                    StartX: request.StartX,
                    StartY: request.StartY
                );

                try
                {
                    var response = await client.GetResponse<RunScriptResult>(
                        command, cancellationToken, RequestTimeout.After(s: 10));

                    return response.Message.Success
                        ? Results.Ok(response.Message)
                        : Results.BadRequest(response.Message);
                }
                catch (RequestTimeoutException)
                {
                    return Results.Problem("Выполнение скрипта истекло по таймауту", statusCode: 504);
                }
                catch (RequestFaultException ex)
                {
                    return Results.Problem($"Ошибка Worker: {ex.Message}", statusCode: 502);
                }
            })
            .WithSummary("Run robot script via Worker")
            .RequireAuthorization(); // при необходимости
    }
}