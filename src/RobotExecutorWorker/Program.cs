using MassTransit;
using RobotCodingPlatform.Core;
using RobotExecutorWorker;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var executionLimits = context.Configuration
            .GetSection("Execution")
            .Get<ExecutionLimits>() ?? ExecutionLimits.Default;

        services.AddSingleton(executionLimits);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<RunScriptConsumer>();
            x.AddConsumer<RunSolutionConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var config = ctx.GetRequiredService<IConfiguration>();

                var rabbitMqHost = config.GetValue<string>("RabbitMQ:Host");
                var rabbitMqUsername = config.GetValue<string>("RabbitMQ:Username");
                var rabbitMqPassword = config.GetValue<string>("RabbitMQ:Password");

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

                cfg.ConfigureEndpoints(ctx);
            });
        });
    })
    .Build();

await host.RunAsync();