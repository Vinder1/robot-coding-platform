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
                var config = context.GetRequiredService<IConfiguration>();
                var rabbitMqHost = config
                    .GetValue<string>("RabbitMQ:Host");

                var rabbitMqUsername = config
                    .GetValue<string>("RabbitMQ:Username");

                var rabbitMqPassword = config
                    .GetValue<string>("RabbitMQ:Password");

                if (string.IsNullOrWhiteSpace(rabbitMqHost) ||
                    string.IsNullOrWhiteSpace(rabbitMqUsername) ||
                    string.IsNullOrWhiteSpace(rabbitMqPassword))
                {
                    throw new InvalidOperationException(
                        "RabbitMQ configuration is missing.");
                }

                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
        return builder;
    }
}
