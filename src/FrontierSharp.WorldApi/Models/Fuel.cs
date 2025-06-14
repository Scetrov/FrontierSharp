using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class Fuel {
    [JsonPropertyName("type")] public GameType Type { get; set; } = new();
    [JsonPropertyName("efficiency")] public byte Efficiency { get; set; } = 0;
}