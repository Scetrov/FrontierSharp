using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetListOfTypes : GetRequestModel<GetListOfTypes>, IGetRequestModel, IWorldApiEnumerableEndpoint {

    public override string GetCacheKey() {
        return this.GenerateCacheKey();
    }

    public override Dictionary<string, string> GetQueryParams() {
        return this.GenerateParams();
    }

    public override string GetEndpoint() {
        return "/v2/types";
    }

    public long Limit { get; set; }
    public long Offset { get; set; }
}