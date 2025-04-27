using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class ShipPathSegment {
    [JsonPropertyName("from")] public required string From { get; set; }

    [JsonPropertyName("to")] public required string To { get; set; }

    [JsonPropertyName("jumpGate")]
    [JsonConverter(typeof(NullableStringifiedBooleanConverter))]
    public bool? JumpGate { get; set; }

    [JsonPropertyName("distance")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int? Distance { get; set; }
}