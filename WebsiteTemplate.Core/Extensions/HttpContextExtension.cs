using Microsoft.Extensions.Primitives;

namespace WebsiteTemplate.Core.Extensions;

public static class HttpContextExtension
{
    private const string Bearer = "Bearer ";

    public static string? GetBearerToken(this HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization;
        if (!StringValues.IsNullOrEmpty(authHeader) && authHeader[0]!.StartsWith(Bearer))
        {
            var token = authHeader.FirstOrDefault()?.Split(Bearer).Last();
            return token;
        }

        return null;
    }
}