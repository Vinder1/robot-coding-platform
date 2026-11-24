using MassTransit;
using RobotCodingPlatform.Core.Contracts;

namespace RobotCodingPlatform.Web.Extensions;

public static class MassTransitExtensions
{
    public static WebApplicationBuilder ConfigureMassTransit(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.AddRequestClient<RunScriptCommand>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });
        return builder;
    }
}