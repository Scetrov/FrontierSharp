using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class TravelLogEntry {
    [JsonPropertyName("from")] public required string From { get; set; }

    [JsonPropertyName("to")] public required string To { get; set; }

    [JsonPropertyName("jumps")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int Jumps { get; set; }

    [JsonPropertyName("fuel_used")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelUsed { get; set; }

    [JsonPropertyName("distance")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public required int Distance { get; set; }
}