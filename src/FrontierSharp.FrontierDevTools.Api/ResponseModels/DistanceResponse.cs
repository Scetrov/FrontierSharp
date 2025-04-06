using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class DistanceResponse {
    [JsonPropertyName("system_a")]
    public string SystemA { get; set; } = "Unknown";

    [JsonPropertyName("system_b")]
    public string SystemB { get; set; } = "Unknown";

    [JsonPropertyName("distance_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal DistanceInLightYears { get; set; }
}