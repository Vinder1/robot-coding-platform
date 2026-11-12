namespace RobotCodingPlatform.Web.Features.Auth;

public static class HttpContextExtensions
{
    private const string CookiesPath = "/";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <param name="access"></param>
    /// <param name="refresh"></param>
    /// <param name="jwtSettings"></param>
    public static void SetTokensCookie(
        this HttpResponse response,
        NewToken access,
        NewToken refresh,
        JwtSettings jwtSettings
    )
    {
        response.Cookies.Append(jwtSettings.AccessTokenCookieName, access.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiesPath,
            Expires = refresh.ExpiresAt
        });

        response.Cookies.Append(jwtSettings.RefreshTokenCookieName, refresh.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiesPath,
            Expires = refresh.ExpiresAt
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <param name="jwtSettings"></param>
    public static void ClearTokensCookie(this HttpResponse response, JwtSettings jwtSettings)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiesPath
        };

        response.Cookies.Delete(jwtSettings.AccessTokenCookieName, cookieOptions);
        response.Cookies.Delete(jwtSettings.RefreshTokenCookieName, cookieOptions);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="jwtSettings"></param>
    /// <returns></returns>
    public static string? GetRefreshTokenCookie(this HttpRequest request, JwtSettings jwtSettings)
    {
        return request.Cookies[jwtSettings.RefreshTokenCookieName];
    }
}