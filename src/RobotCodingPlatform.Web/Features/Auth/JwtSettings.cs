namespace RobotCodingPlatform.Web.Features.Auth;

/// <summary>
/// 
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 
    /// </summary>
    public const string SectionName = "Jwt";
    /// <summary>
    /// 
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string AccessTokenCookieName { get; set; } = "access_token";
    /// <summary>
    /// 
    /// </summary>
    public string RefreshTokenCookieName { get; set; } = "refresh_token";
    /// <summary>
    /// 
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    /// <summary>
    /// 
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}