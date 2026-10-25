using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RobotCodingPlatform.Web.Features.User.SignIn;

namespace RobotCodingPlatform.Web.Features.User;

public class ProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/profile").WithTags("User");

        group.MapPost("/create", async (
            [FromBody] SignInRequest request,
            HttpContext httpContext,
            SignInService service) =>
        {
            var res = await service.SignInAsync(request);
            if (!res.Success)
            {
                return Results.BadRequest(res);
            }
            return Results.Ok(res);
        }).WithSummary("Sign In");
    }
}