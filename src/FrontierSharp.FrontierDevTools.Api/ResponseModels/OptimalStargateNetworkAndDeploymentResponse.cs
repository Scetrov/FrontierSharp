using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class OptimalStargateNetworkAndDeploymentResponse {
    [JsonPropertyName("results")] public required RouteResults Results { get; set; }
}