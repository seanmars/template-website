namespace WebsiteTemplate.Core.Endpoints.ResponseResults;

public class ProblemResponse : BaseResponse
{
    public string Details { get; set; } = string.Empty;
}