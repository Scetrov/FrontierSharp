using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetListOfTypes : GetRequestModel<GetListOfTypes>, IGetRequestModel, IWorldApiEnumerableEndpoint {
    public long Limit { get; set; }
    public long Offset { get; set; }

    public override string GetCacheKey() => this.GenerateCacheKey();

    public override Dictionary<string, string> GetQueryParams() => this.GenerateParams();

    public override string GetEndpoint() => "/v2/types";
}