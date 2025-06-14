using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.FrontierDevTools.Api.RequestModels;

public class GetGateNetworkRequest : GetRequestModel<GetGateNetworkRequest>, IGetRequestModel {
    public string Identifier { get; init; } = string.Empty;

    public override string GetCacheKey() {
        return $"{nameof(GetGateNetworkRequest)}_{Identifier}";
    }

    public override Dictionary<string, string> GetQueryParams() {
        return new Dictionary<string, string> {
            {
                "identifier", Identifier
            }
        };
    }

    public override string GetEndpoint() {
        return "/get_gate_network";
    }
}