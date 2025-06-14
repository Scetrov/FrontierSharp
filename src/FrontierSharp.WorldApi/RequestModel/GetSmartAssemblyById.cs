using System.Numerics;
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetSmartAssemblyById : GetRequestModel<GetSmartAssemblyById>, IGetRequestModel {
    public BigInteger SmartObjectId { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_Type_{SmartObjectId}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() {
        return $"/v2/smartassemblies/{SmartObjectId}";
    }
}