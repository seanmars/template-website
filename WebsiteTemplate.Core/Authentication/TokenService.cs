using Microsoft.EntityFrameworkCore;
using WebsiteTemplate.Core.Data;
using WebsiteTemplate.Core.Models;

namespace WebsiteTemplate.Core.Authentication;

public class TokenService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;
    private readonly JwtHelper _jwtHelper;

    public TokenService(ILogger<TokenService> logger, AppDbContext dbContext, JwtHelper jwtHelper)
    {
        _logger = logger;
        _dbContext = dbContext;
        _jwtHelper = jwtHelper;
    }

    public Task<bool> IsAccessTokenValid(string? token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        return _dbContext.BlockedAccessTokens.AllAsync(x => x.Token != token, ct);
    }

    public async Task<bool> BlockAccessToken(string token, CancellationToken ct = default)
    {
        try
        {
            var expiry = _jwtHelper.GetExpiryFromToken(token);
            if (expiry == null)
            {
                return false;
            }

            _dbContext.BlockedAccessTokens.Add(new BlockedAccessToken
            {
                Token = token,
                ExpiryDate = DateTimeOffset.FromUnixTimeSeconds((long)expiry).UtcDateTime
            });

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to block access token");
            return false;
        }
    }

    public Task<bool> IsRefreshTokenValid(string? token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        return _dbContext.BlockedRefreshTokens.AllAsync(x => x.Token != token, ct);
    }

    public async Task BlockRefreshToken(string token, CancellationToken ct = default)
    {
        _dbContext.BlockedRefreshTokens.Add(new BlockedRefreshToken
        {
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddHours(1)
        });

        await _dbContext.SaveChangesAsync(ct);
    }
}