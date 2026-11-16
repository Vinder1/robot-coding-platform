using Carter;
using RobotCodingPlatform.Web.Extensions;
using RobotCodingPlatform.Web.Features.SwaggerScalar;
using FluentValidation;
using RobotCodingPlatform.Web.Features.Users;
using RobotCodingPlatform.Web.Features.Auth;
using RobotCodingPlatform.Web.Features.Profile;
using RobotCodingPlatform.Web.Features.Admin;
using RobotCodingPlatform.Web.Features.Code;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSwagger();
builder.AddCustomScalar();
builder.AddCustomAuthentication();
builder.AddCustomSerilog();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.ConfigureDatabase();
builder.ConfigureMassTransit();

builder.AddUserServices();
builder.AddAdminServices();
builder.AddAuthServices();
builder.AddProfileServices();
builder.AddCodeServices();

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
