using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetConfig : GetRequestModel<GetConfig>, IGetRequestModel {
    public override string GetCacheKey() {
        return "WorldApi_Config";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return "/config";
    }
}