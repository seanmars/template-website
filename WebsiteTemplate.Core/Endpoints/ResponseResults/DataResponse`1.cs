namespace WebsiteTemplate.Core.Endpoints.ResponseResults;

public class DataResponse<T> : BaseResponse
{
    /// <summary>
    /// 回應資料
    /// </summary>
    public T Data { get; set; } = default!;
}