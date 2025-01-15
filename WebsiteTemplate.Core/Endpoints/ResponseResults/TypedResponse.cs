using Microsoft.AspNetCore.Http.HttpResults;

namespace WebsiteTemplate.Core.Endpoints.ResponseResults;

public static class TypedResponse
{
    public static Ok<OkResponse> Ok() =>
        TypedResults.Ok(new OkResponse());

    public static BadRequest<ProblemResponse> Problem(string details, string? code = null) =>
        TypedResults.BadRequest(
            new ProblemResponse
            {
                Success = false,
                Code = code,
                Details = details
            });

    public static Ok<DataResponse<T>> OkData<T>(T data) =>
        TypedResults.Ok(new DataResponse<T>
        {
            Success = true,
            Data = data
        });
}