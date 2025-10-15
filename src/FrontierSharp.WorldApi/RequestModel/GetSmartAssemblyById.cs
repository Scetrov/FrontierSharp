using System.Numerics;
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetSmartAssemblyById : GetRequestModel<GetSmartAssemblyById>, IGetRequestModel {
    public BigInteger SmartObjectId { get; set; }

    // optional format (json, pod)
    public string? Format { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_SmartAssembly_{SmartObjectId}{(string.IsNullOrEmpty(Format) ? string.Empty : $"_fmt={Format}")}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Format)) d["format"] = Format!;
        return d;
    }

    public override string GetEndpoint() {
        return $"/v2/smartassemblies/{SmartObjectId}";
    }
}