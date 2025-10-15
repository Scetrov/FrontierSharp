using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetConfig : GetRequestModel<GetConfig>, IGetRequestModel {
    public override string GetCacheKey() => "WorldApi_Config";
    public override Dictionary<string, string> GetQueryParams() => new();
    public override string GetEndpoint() => "/config";
}
