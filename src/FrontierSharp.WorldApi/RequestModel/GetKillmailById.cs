using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetKillmailById : GetRequestModel<GetKillmailById>, IGetRequestModel {
    public string KillmailId { get; set; } = string.Empty;

    // optional format (json, pod)
    public string? Format { get; set; }

    public override string GetCacheKey() {
        return $"WorldApi_Killmail_{KillmailId}{(string.IsNullOrEmpty(Format) ? string.Empty : $"_fmt={Format}")}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Format)) d["format"] = Format!;
        return d;
    }

    public override string GetEndpoint() => $"/v2/killmails/{KillmailId}";
}
