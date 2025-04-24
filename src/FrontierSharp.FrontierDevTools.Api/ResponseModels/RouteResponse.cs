using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class RouteResponse {
    [JsonPropertyName("route")] public IEnumerable<JumpResponse> Route { get; set; } = [];
}