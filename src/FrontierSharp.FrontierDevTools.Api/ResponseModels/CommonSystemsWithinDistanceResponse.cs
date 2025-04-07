using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class CommonSystemsWithinDistanceResponse {
    [JsonPropertyName("reference_systems")]
    public IEnumerable<string> ReferenceSystems { get; set; } = [];
    
    [JsonPropertyName("common_systems")]
    public IEnumerable<CommonSystemsResponse> CommonSystems { get; set; } = [];
}