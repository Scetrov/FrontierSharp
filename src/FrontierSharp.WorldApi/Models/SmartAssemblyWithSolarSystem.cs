using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class SmartAssemblyWithSolarSystem : SmartAssembly {
    [JsonPropertyName("solarSystem")]
    public SolarSystem SolarSystem { get; set; } = new();
}