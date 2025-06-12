using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class CommonSystemsResponse {
    [JsonPropertyName("system_name")] public string SystemName { get; set; } = string.Empty;

    [JsonPropertyName("distance_from_a_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public required decimal DistanceFromAInLy { get; set; }

    [JsonPropertyName("distance_from_b_ly")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public required decimal DistanceFromBInLy { get; set; }

    [JsonPropertyName("npc_gates")] public required int NpcGates { get; set; }
}