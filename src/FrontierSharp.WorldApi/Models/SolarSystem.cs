using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SolarSystem {
    [JsonPropertyName("id")] public long Id { get; set; } = 0;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("location")] public Location Location { get; set; } = new();
}