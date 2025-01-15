using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteTemplate.Core.Authentication;
using WebsiteTemplate.Core.Endpoints.ResponseResults;

namespace WebsiteTemplate.Core.Endpoints;

public class UserInfoEndpoint : EndpointWithoutRequest<
    Results<Ok<DataResponse<Dictionary<string, string>>>, ProblemResponse>>
{
    public override void Configure()
    {
        Get("user/info");
        Roles(RoleDefines.User);
    }

    public override Task<Results<Ok<DataResponse<Dictionary<string, string>>>, ProblemResponse>> ExecuteAsync(
        CancellationToken ct)
    {
        var user = HttpContext.User;
        var dict = new Dictionary<string, string>();
        foreach (var claim in user.Claims)
        {
            dict[claim.Type] = claim.Value;
        }

        return Task.FromResult<Results<Ok<DataResponse<Dictionary<string, string>>>, ProblemResponse>>(
            TypedResponse.OkData(dict));
    }
}