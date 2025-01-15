using System.Text.Json.Serialization;
using FastEndpoints;

namespace WebsiteTemplate.Core.Endpoints.ResponseResults;

public class BaseResponse : IResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 訊息碼
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        return httpContext.Response.SendAsync(this, Success
            ? StatusCodes.Status200OK
            : StatusCodes.Status400BadRequest);
    }
}