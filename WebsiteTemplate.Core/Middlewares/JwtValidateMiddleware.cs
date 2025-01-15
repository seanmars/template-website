using FastEndpoints;
using WebsiteTemplate.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using WebsiteTemplate.Core.Authentication;

namespace WebsiteTemplate.Core.Middlewares;

public class JwtValidateMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    private readonly TokenService _tokenService;

    public JwtValidateMiddleware(ILogger<JwtValidateMiddleware> logger, TokenService tokenService)
    {
        _logger = logger;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.OfType<IAllowAnonymous>().Any() is null or true)
        {
            await next(context);
            return;
        }

        var token = context.GetBearerToken();
        if (!await JwtTokenIsValidAsync(token, context.RequestAborted))
        {
            await SendTokenRevokedResponseAsync(context, context.RequestAborted);
        }

        await next(context);
    }

    private Task<bool> JwtTokenIsValidAsync(string? token, CancellationToken ct = default)
    {
        return _tokenService.IsAccessTokenValid(token, ct);
    }

    private Task SendTokenRevokedResponseAsync(HttpContext context, CancellationToken ct = default)
    {
        return context.Response.SendUnauthorizedAsync(ct);
    }
}