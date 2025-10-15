using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetSolarSystemById : GetRequestModel<GetSolarSystemById>, IGetRequestModel {
    public long SolarSystemId { get; set; }

    // optional format (json, pod)
    public string? Format { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_SolarSystem_{SolarSystemId}{(string.IsNullOrEmpty(Format) ? string.Empty : $"_fmt={Format}")}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Format)) d["format"] = Format!;
        return d;
    }

    public override string GetEndpoint() {
        return $"/v2/solarsystems/{SolarSystemId}";
    }
}