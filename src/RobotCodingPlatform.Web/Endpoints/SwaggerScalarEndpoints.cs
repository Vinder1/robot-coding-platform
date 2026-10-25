using Scalar.AspNetCore;

namespace RobotCodingPlatform.Web.Endpoints;

public static class SwaggerScalarEndpoints
{
    public static WebApplication MapSwaggerAndScalar(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // swagger
            // available at /swagger
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                options.RoutePrefix = "swagger";
            });
            // scalar
            // available at /scalar
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