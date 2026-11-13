using Scalar.AspNetCore;

namespace RobotCodingPlatform.Web.Features.SwaggerScalar;

public static class SwaggerScalarUIEndpoints
{
    public static WebApplication MapSwaggerAndScalar(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                options.RoutePrefix = "swagger";
            });
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("My API Documentation");
                options.WithTheme(ScalarTheme.Default);
            });
        }
        return app;
    }
}