using Carter;
using RobotCodingPlatform.Web.Extensions;
using RobotCodingPlatform.Web.Features.SwaggerScalar;
using FluentValidation;
using MassTransit;
using RobotCodingPlatform.Core.Contracts;
using RobotCodingPlatform.Web.Features.Users;
using RobotCodingPlatform.Web.Features.Auth;
using RobotCodingPlatform.Web.Features.Profile;
using RobotCodingPlatform.Web.Features.Admin;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSwagger();
builder.AddCustomScalar();
builder.AddCustomAuthentication();
builder.AddCustomSerilog();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.ConfigureDatabase();
builder.AddUserServices();
builder.AddAdminServices();
builder.AddAuthServices();
builder.AddProfileServices();
builder.AddCodeServices();

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

builder.Services.AddCarter();

var app = builder.Build();

app.Migrate();
app.UseCustomSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapGet("/", () => Results.File("index.html", "text/html"));

app.MapSwaggerAndScalar();
app.MapCarter();

app.Run();
