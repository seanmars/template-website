using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using WebsiteTemplate.Core.Authentication;
using WebsiteTemplate.Core.Endpoints.ResponseResults;
using WebsiteTemplate.Core.Models;

namespace WebsiteTemplate.Core.Endpoints;

public class UserSignUpEndpoint : Endpoint<UserSignUpEndpoint.Request,
    Results<Ok<OkResponse>, BadRequest<ProblemResponse>>>
{
    public struct Request
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    private readonly ILogger _logger;

    private readonly RoleManager<AppIdentityRole> _roleManager;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IUserStore<AppIdentityUser> _userStore;

    public UserSignUpEndpoint(ILogger<UserSignUpEndpoint> logger,
        RoleManager<AppIdentityRole> roleManager,
        UserManager<AppIdentityUser> userManager,
        IUserStore<AppIdentityUser> userStore)
    {
        _logger = logger;
        _roleManager = roleManager;
        _userManager = userManager;
        _userStore = userStore;
    }

    public override void Configure()
    {
        Post("user/signup");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<OkResponse>, BadRequest<ProblemResponse>>> ExecuteAsync(Request req,
        CancellationToken ct)
    {
        _logger.LogInformation("UserRegisterEndpoint: {@Request}", req);

        if (!await _roleManager.RoleExistsAsync(RoleDefines.User))
        {
            await _roleManager.CreateAsync(new AppIdentityRole { Name = RoleDefines.User });
        }

        var user = new AppIdentityUser();
        await _userStore.SetUserNameAsync(user, req.UserName, ct);

        var result = await _userManager.CreateAsync(user, req.Password);
        await _userManager.AddToRoleAsync(user, RoleDefines.User);

        if (!result.Succeeded)
        {
            return TypedResponse.Problem("Failed to create user");
        }

        return TypedResponse.Ok();
    }
}