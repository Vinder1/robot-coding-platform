using Carter;
using Serilog;

namespace RobotCodingPlatform.Web.Features.Code;

public class ProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/code").WithTags("Code");

        group.MapPost("/run",
            async (HttpContext httpContext,
            RunCodeParams request,
            // ILogger<RunCodeService> logger,
            IRunCodeService service) =>
        {
            // logger.LogInformation(request.Code);
            var res = await service.Run(request);
            return res.Compiled ? Results.Ok(res) : Results.BadRequest(res);
        })
        .RequireAuthorization()
        .WithSummary("Run given code with given params");
    }
}