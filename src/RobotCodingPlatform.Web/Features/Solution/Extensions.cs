using RobotCodingPlatform.Web.Features.Solution.Create;

namespace RobotCodingPlatform.Web.Features.Solution;

public static class SolutionExtensions
{
    public static WebApplicationBuilder AddSolutionServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISolutionAttemptRepository, SolutionAttemptRepository>();
        builder.Services.AddScoped<ICreateSolutionAttemptService, CreateSolutionAttemptService>();

        return builder;
    }
}
