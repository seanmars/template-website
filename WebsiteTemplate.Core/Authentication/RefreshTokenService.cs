using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebsiteTemplate.Core.Data;
using WebsiteTemplate.Core.Models;

namespace WebsiteTemplate.Core.Authentication;

public class RefreshTokenService : RefreshTokenService<TokenRequest, TokenResponse>
{
    private readonly ILogger _logger;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly TokenService _tokenService;

    public RefreshTokenService(ILogger<RefreshTokenService> logger, IOptions<JwtOption> jwtOption,
        UserManager<AppIdentityUser> userManager,
        AppDbContext dbContext,
        TokenService tokenService)
    {
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;

        var jwtConfig = jwtOption.Value;
        Setup(options =>
        {
            options.TokenSigningKey = jwtConfig.Key;
            options.TokenSigningAlgorithm = SecurityAlgorithms.HmacSha256;
            options.AccessTokenValidity = TimeSpan.FromDays(jwtConfig.ExpiryDays);
            options.RefreshTokenValidity = TimeSpan.FromDays(jwtConfig.RefreshExpiryDays);

            options.Endpoint("/user/refresh-token", ep =>
            {
                ep.Summary(s => s.Summary = "Refresh token endpoint");
            });
        });
    }

    public override Task PersistTokenAsync(TokenResponse response)
    {
        // this method will be called whenever a new access/refresh token pair is being generated.
        // store the tokens and expiry dates however you wish for the purpose of verifying
        // future refresh requests.

        return Task.CompletedTask;
    }

    public override async Task RefreshRequestValidationAsync(TokenRequest req)
    {
        // validate the incoming refresh request by checking the token and expiry against the
        // previously stored data. if the token is not valid and a new token pair should
        // not be created, simply add validation errors using the AddError() method.
        // the failures you add will be sent to the requesting client. if no failures are added,
        // validation passes and a new token pair will be created and sent to the client.

        var isBlocked = await _tokenService.IsRefreshTokenValid(req.RefreshToken);
        if (!isBlocked)
        {
            AddError("The token is invalid");
        }
    }

    public override async Task SetRenewalPrivilegesAsync(TokenRequest request, UserPrivileges privileges)
    {
        // specify the user privileges to be embedded in the jwt when a refresh request is
        // received and validation has passed. this only applies to renewal/refresh requests
        // received to the refresh endpoint and not the initial jwt creation.

        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                AddError("Invalid request");
                _logger.LogWarning("User not found, but the token is valid");
                return;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            privileges.Roles.AddRange(_userManager.GetRolesAsync(user).Result);
            privileges.Claims.AddRange(claims);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ErrorMessage}", e.Message);
            AddError("An error occurred while processing the request");
        }
    }

    public override async Task OnAfterRenewalTokenCreationAsync(TokenRequest? request, TokenResponse response)
    {
        try
        {
            await _tokenService.BlockRefreshToken(request!.RefreshToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on OnAfterRenewalTokenCreationAsync");
        }

        await base.OnAfterRenewalTokenCreationAsync(request, response);
    }
}