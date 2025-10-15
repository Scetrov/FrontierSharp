using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetTribeById : GetRequestModel<GetTribeById>, IGetRequestModel {
    public long TribeId { get; set; }

    // optional format (json, pod)
    public string? Format { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_Tribe_{TribeId}{(string.IsNullOrEmpty(Format) ? string.Empty : $"_fmt={Format}")}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Format)) d["format"] = Format!;
        return d;
    }

    public override string GetEndpoint() => $"/v2/tribes/{TribeId}";
}
