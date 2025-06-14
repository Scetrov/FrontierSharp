using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class SmartAssembly {
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SmartAssemblyType Type { get; set; } = SmartAssemblyType.NetworkNode;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SmartAssemblyState State { get; set; } = SmartAssemblyState.Null;

    [JsonPropertyName("owner")]
    public SmartCharacter Owner { get; set; } = new();

    [JsonPropertyName("energyUsage")]
    public ulong EnergyUsage { get; set; } = 0;

    [JsonPropertyName("typeId")]
    public int TypeId { get; set; } = 0;
}