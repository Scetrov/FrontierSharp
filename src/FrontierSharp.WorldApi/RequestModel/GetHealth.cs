using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetHealth : GetRequestModel<GetHealth>, IGetRequestModel {
    public override string GetCacheKey() => "WorldApi_Health";

    public override Dictionary<string, string> GetQueryParams() => new Dictionary<string, string>();

    public override string GetEndpoint() => "/health";
}
