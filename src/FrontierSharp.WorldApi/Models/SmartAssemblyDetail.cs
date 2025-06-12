using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class SmartAssemblyDetail : SmartAssembly {
    [JsonPropertyName("solarSystem")]
    public SolarSystem SolarSystem { get; set; } = new();

    [JsonPropertyName("typeDetails")]
    public GameType TypeDetails { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("dappUrl")]
    public string DappUrl { get; set; } = string.Empty;

    [JsonPropertyName("manufacturing")]
    public Manufacturing Manufacturing { get; set; } = new();

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();
}