using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class GateNetworkResponse {
    [JsonPropertyName("gate_network")] public IEnumerable<GateResponse> GateNetwork { get; set; } = [];
}