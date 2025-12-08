using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SolarSystemDetail : SolarSystem {
    [JsonPropertyName("smartAssemblies")] public IEnumerable<SmartAssembly> SmartAssemblies { get; set; } = [];

    [JsonPropertyName("regionId")] public long RegionId { get; set; } = 0;
}