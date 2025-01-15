using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using WebsiteTemplate.Core.Authentication;
using WebsiteTemplate.Core.Endpoints.ResponseResults;
using WebsiteTemplate.Core.Models;

namespace WebsiteTemplate.Core.Endpoints;

public class UserSignInEndpoint : Endpoint<UserSignInEndpoint.Request, TokenResponse>
{
    public struct Request
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    private readonly ILogger _logger;
    private readonly UserManager<AppIdentityUser> _userManager;

    public UserSignInEndpoint(ILogger<UserSignInEndpoint> logger,
        UserManager<AppIdentityUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public override void Configure()
    {
        Post("user/signin");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(req.UserName);
        if (user == null)
        {
            await SendResultAsync(TypedResponse.Problem("Invalid username or password"));
            return;
        }

        var result = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!result)
        {
            await SendResultAsync(TypedResponse.Problem("Invalid username or password"));
            return;
        }

        var tokenResponse = await CreateTokenWith<RefreshTokenService>(user.Id, userPrivileges =>
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            userPrivileges.Roles.AddRange(_userManager.GetRolesAsync(user).Result);
            userPrivileges.Claims.AddRange(claims);
        });

        await SendResultAsync(TypedResponse.OkData(tokenResponse));
    }
}