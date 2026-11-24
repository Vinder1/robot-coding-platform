using MassTransit;
using RobotCodingPlatform.Core.Contracts;
using RobotCodingPlatform.Web.Features.Solution;
using RobotCodingPlatform.Web.Repositories;

namespace RobotCodingPlatform.Web.Extensions;

public static class MassTransitExtensions
{
    public static WebApplicationBuilder ConfigureMassTransit(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.AddRequestClient<RunScriptCommand>();
            x.AddConsumer<SolutionAttemptCompletedConsumer>();

            x.AddEntityFrameworkOutbox<AppDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });
        return builder;
    }
}
