using Carter;
using RobotCodingPlatform.Web.Extensions;
using RobotCodingPlatform.Web.Features.SwaggerScalar;
using FluentValidation;
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

builder.Services.AddCarter();

var app = builder.Build();

app.Migrate();
app.UseCustomSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hi!");
app.MapSwaggerAndScalar();
app.MapCarter();

app.Run();
