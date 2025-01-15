namespace WebsiteTemplate.Core.Endpoints.ResponseResults;

public class OkResponse : BaseResponse
{
    public OkResponse()
    {
        Success = true;
    }
}