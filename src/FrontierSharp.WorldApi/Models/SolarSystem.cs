using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class SolarSystem {
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new Location();
}