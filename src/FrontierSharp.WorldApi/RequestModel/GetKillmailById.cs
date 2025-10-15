// filepath: c:\source\FrontierSharp\src\FrontierSharp.WorldApi\RequestModel\GetKillmailById.cs
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.WorldApi.RequestModel;

public class GetKillmailById : GetRequestModel<GetKillmailById>, IGetRequestModel {
    public string KillmailId { get; set; } = string.Empty;

    public override string GetCacheKey() {
        return $"WorldApi_Killmail_{KillmailId}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string>();
    }

    public override string GetEndpoint() => $"/v2/killmails/{KillmailId}";
}
