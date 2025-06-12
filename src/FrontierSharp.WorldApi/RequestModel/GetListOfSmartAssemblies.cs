using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetListOfSmartAssemblies : GetRequestModel<GetListOfSmartAssemblies>, IGetRequestModel, IWorldApiEnumerableEndpoint {

    public override string GetCacheKey() {
        return this.GenerateCacheKey();
    }

    public override Dictionary<string, string> GetQueryParams() {
        return this.GenerateParams();
    }

    public override string GetEndpoint() {
        return "/v2/smartassemblies";
    }

    public long Limit { get; set; }
    public long Offset { get; set; }
}