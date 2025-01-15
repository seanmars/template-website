using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;

namespace WebsiteTemplate.Core.Authentication;

public class JwtHelper
{
    private readonly ILogger _logger;
    private readonly JwtOption _jwtOption;
    private readonly JwtSecurityTokenHandler _jwtHandler;

    public JwtHelper(ILogger<JwtHelper> logger, IOptions<JwtOption> jwtOption)
    {
        _logger = logger;
        _jwtOption = jwtOption.Value;
        _jwtHandler = new JwtSecurityTokenHandler();
    }

    public JwtSecurityToken DecodeToken(string token)
    {
        return _jwtHandler.ReadJwtToken(token);
    }

    public long? GetExpiryFromToken(string token, string key = "exp")
    {
        try
        {
            var jwtSecurityToken = DecodeToken(token);
            return (long)jwtSecurityToken.Payload[key];
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get expiry from token");
            return null;
        }
    }
}