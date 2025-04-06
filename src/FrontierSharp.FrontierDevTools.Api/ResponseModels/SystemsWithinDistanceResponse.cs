using System.Text.Json.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class SystemsWithinDistanceResponse {
    [JsonPropertyName("reference_system")]
    public required string ReferenceSystem { get; set; }
    
    [JsonPropertyName("nearby_systems")]
    public IEnumerable<SystemDistanceResponse> NearbySystems { get; set; } = [];
}