using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RobotCodingPlatform.Web.Features.Auth;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Extensions;

public static class AuthExtensions
{
    public static void AddCustomAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()!;
            
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });
        
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => 
                policy.RequireRole(Roles.Admin));
    }
}
