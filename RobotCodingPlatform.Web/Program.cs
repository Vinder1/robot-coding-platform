using RobotCodingPlatform.Web.Endpoints;
using RobotCodingPlatform.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSwagger();
builder.AddCustomScalar();
builder.AddCustomAuthentication();
builder.AddCustomSerilog();

builder.ConfigureDatabase();

var app = builder.Build();

// app.Migrate();
app.UseCustomSerilog();

app.MapGet("/", () => "Hi!");
app.MapSwaggerAndScalar();

app.Run();
