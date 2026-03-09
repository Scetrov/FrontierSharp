using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SolarSystemDetail : SolarSystem {
    [JsonPropertyName("smartAssemblies")] public IEnumerable<SolarSystemSmartAssembly> SmartAssemblies { get; set; } = [];

    [JsonPropertyName("regionId")] public long RegionId { get; set; } = 0;
}

[ExcludeFromCodeCoverage]
public class SolarSystemSmartAssembly {
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
}
