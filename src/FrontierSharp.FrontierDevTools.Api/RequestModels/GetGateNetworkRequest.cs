using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetGateNetworkRequest : GetRequestModel<GetGateNetworkRequest> {
    public string Identifier { get; init; } = string.Empty;

    public override string GetCacheKey() =>
        $"{nameof(GetGateNetworkRequest)}_{Identifier}";

    public override Dictionary<string, string> GetQueryParams() =>
        new() {
            { "identifier", Identifier }
        };

    public override string GetEndpoint() =>
        "/get_gate_network";
}