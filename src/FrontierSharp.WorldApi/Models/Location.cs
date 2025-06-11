using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

public class Location {
    [JsonPropertyName("x")]
    public decimal X { get; set; }

    [JsonPropertyName("y")]
    public decimal Y { get; set; }

    [JsonPropertyName("z")]
    public decimal Z { get; set; }
}