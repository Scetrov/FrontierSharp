using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetListOfSolarSystems : GetRequestModel<GetListOfSolarSystems>, IGetRequestModel, IWorldApiEnumerableEndpoint {
    public override string GetCacheKey() {
        return this.GenerateCacheKey();
    }

    public override Dictionary<string, string> GetQueryParams() {
        return this.GenerateParams();
    }

    public override string GetEndpoint() {
        return "/v2/solarsystems";
    }

    public long Limit { get; set; }
    public long Offset { get; set; }
}