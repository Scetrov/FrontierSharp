using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetListOfFuels : GetRequestModel<GetListOfFuels>, IGetRequestModel, IWorldApiEnumerableEndpoint {
    public override string GetCacheKey() {
        return this.GenerateCacheKey();
    }

    public override Dictionary<string, string> GetQueryParams() {
        return this.GenerateParams();
    }

    public override string GetEndpoint() {
        return "/v2/fuels";
    }

    public long Limit { get; set; }
    public long Offset { get; set; }
}