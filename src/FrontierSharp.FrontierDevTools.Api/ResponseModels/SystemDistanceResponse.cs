using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class SystemDistanceResponse {
    [JsonPropertyName("system_name")] public required string SystemName { get; init; }

    [JsonPropertyName("distance_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal DistanceInLightYears { get; init; }
}