using System.Security.Cryptography;

namespace WebsiteTemplate.StartApp;

public static class JwtHelper
{
    public static string GenerateJwtSecret()
    {
        var secret = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(secret);
        return Convert.ToBase64String(secret);
    }
}