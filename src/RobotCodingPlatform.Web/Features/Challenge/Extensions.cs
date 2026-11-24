using RobotCodingPlatform.Web.Features.Challenge.Create;

namespace RobotCodingPlatform.Web.Features.Challenge;

public static class ChallengeExtensions
{
    public static WebApplicationBuilder AddChallengeServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
        builder.Services.AddScoped<ICreateChallengeService, CreateChallengeService>();

        return builder;
    }
}
