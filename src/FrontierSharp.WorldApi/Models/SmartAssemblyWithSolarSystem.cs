using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SmartAssemblyWithSolarSystem : SmartAssembly {
    [JsonPropertyName("solarSystem")]
    public SolarSystem SolarSystem { get; set; } = new();
}