using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetListOfSmartAssemblies : GetRequestModel<GetListOfSmartAssemblies>, IGetRequestModel, IWorldApiEnumerableEndpoint {
    public override string GetCacheKey() {
        var baseKey = this.GenerateCacheKey();
        return string.IsNullOrEmpty(Type?.ToString()) ? baseKey : baseKey + $"_type={Type}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        var d = this.GenerateParams();
        if (Type.HasValue) d["type"] = Type.Value.ToString();
        return d;
    }

    public override string GetEndpoint() {
        return "/v2/smartassemblies";
    }

    public long Limit { get; set; }
    public long Offset { get; set; }

    // optional filter: type
    public long? Type { get; set; }
}