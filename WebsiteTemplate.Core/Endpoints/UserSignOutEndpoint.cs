using FastEndpoints;
using WebsiteTemplate.Core.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteTemplate.Core.Authentication;
using WebsiteTemplate.Core.Endpoints.ResponseResults;

namespace WebsiteTemplate.Core.Endpoints;

public class UserSignOutEndpoint : EndpointWithoutRequest<
    Results<Ok<OkResponse>, BadRequest<ProblemResponse>>>
{
    private readonly TokenService _tokenService;

    public UserSignOutEndpoint(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override void Configure()
    {
        Post("user/sign-out");
        Roles(RoleDefines.User);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var token = HttpContext.GetBearerToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            await SendResultAsync(TypedResponse.Problem("Invalid token"));
        }

        if (!await _tokenService.BlockAccessToken(token!, ct))
        {
            await SendResultAsync(TypedResponse.Problem("Failed to sign out"));
        }

        await SendResultAsync(TypedResponse.Ok());
    }
}