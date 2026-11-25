using Carter;
using RobotCodingPlatform.Web.Extensions;
using RobotCodingPlatform.Web.Features.SwaggerScalar;
using FluentValidation;
using RobotCodingPlatform.Web.Features.Users;
using RobotCodingPlatform.Web.Features.Auth;
using RobotCodingPlatform.Web.Features.Profile;
using RobotCodingPlatform.Web.Features.Admin;
using RobotCodingPlatform.Web.Features.Challenge;
using RobotCodingPlatform.Web.Features.Code;
using RobotCodingPlatform.Web.Features.Solution;
using RobotCodingPlatform.Web.Features.Statistics;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSwagger();
builder.AddCustomScalar();
builder.AddCustomAuthentication();
builder.AddCustomCors();
builder.AddCustomRateLimiting();
builder.AddCustomSerilog();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.ConfigureDatabase();
builder.ConfigureMassTransit();

builder.AddUserServices();
builder.AddAdminServices();
builder.AddAuthServices();
builder.AddProfileServices();
builder.AddCodeServices();
builder.AddChallengeServices();
builder.AddSolutionServices();
builder.AddStatisticsServices();

builder.Services.AddCarter();

var app = builder.Build();

app.Migrate();
await app.InitializeOpenSearchAsync();
app.UseCustomSerilogRequestLogging();

app.UseCustomCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseCustomRateLimiting();

app.UseStaticFiles();
app.MapGet("/", () => Results.File("index.html", "text/html"));

app.MapSwaggerAndScalar();
app.MapCarter();

app.Run();
