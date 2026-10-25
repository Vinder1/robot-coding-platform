using Carter;
using RobotCodingPlatform.Web.Extensions;
using RobotCodingPlatform.Web.Features.SwaggerScalar;
using FluentValidation;
using RobotCodingPlatform.Web.Features.User;
var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSwagger();
builder.AddCustomScalar();
builder.AddCustomAuthentication();
builder.AddCustomSerilog();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.ConfigureDatabase();
builder.Services.AddUserServices();

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
