using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class Killmail {
    [JsonPropertyName("victim")]
    public SmartCharacter Victim { get; set; } = new();

    [JsonPropertyName("killer")]
    public SmartCharacter Killer { get; set; } = new();

    [JsonPropertyName("solarSystemId")]
    public long SolarSystemId { get; set; }

    [JsonPropertyName("lossType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LossType LossType { get; set; } = LossType.Ship;

    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }
}