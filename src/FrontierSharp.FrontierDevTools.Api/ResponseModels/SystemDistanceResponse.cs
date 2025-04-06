using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class SystemDistanceResponse {
    [JsonPropertyName("system_name")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("distance_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal DistanceInLightYears { get; init; }
}
