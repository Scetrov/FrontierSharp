using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class JumpResponse {
    [JsonPropertyName("from")] public string From { get; set; } = "Unknown";

    [JsonPropertyName("to")] public string To { get; set; } = "Unknown";

    [JsonPropertyName("distance_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal DistanceInLightYears { get; set; }
}