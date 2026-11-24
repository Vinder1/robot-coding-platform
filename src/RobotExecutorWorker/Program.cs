using MassTransit;
using RobotExecutorWorker;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RunScriptConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest"); //потом нужно изменить в appsettings
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });
    })
    .Build();

await host.RunAsync();