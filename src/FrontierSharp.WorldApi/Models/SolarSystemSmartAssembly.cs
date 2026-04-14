using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SolarSystemSmartAssembly {
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
}