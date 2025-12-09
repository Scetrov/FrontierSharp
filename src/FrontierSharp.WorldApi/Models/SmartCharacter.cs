using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SmartCharacter {
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
}