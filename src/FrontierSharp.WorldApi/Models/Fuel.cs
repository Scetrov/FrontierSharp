using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class Fuel {
    [JsonPropertyName("type")] public GameType Type { get; set; } = new();
    [JsonPropertyName("efficiency")] public byte Efficiency { get; set; } = 0;
}