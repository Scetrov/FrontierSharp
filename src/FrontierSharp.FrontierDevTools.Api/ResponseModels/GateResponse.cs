using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class GateResponse {
    [JsonPropertyName("from_system")] public required string FromSystem { get; set; }

    [JsonPropertyName("to_system")]
    [JsonConverter(typeof(NullableStringConverter))]
    public string? ToSystem { get; set; }

    [JsonPropertyName("owner")] public required string Owner { get; set; }

    [JsonPropertyName("fuel_amount")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int FuelAmount { get; set; }

    [JsonPropertyName("fromIsOnline")]
    [JsonConverter(typeof(StringifiedBooleanConverter))]
    public bool FromIsOnline { get; set; }

    [JsonPropertyName("toIsOnline")]
    [JsonConverter(typeof(NullableStringifiedBooleanConverter))]
    public bool? ToIsOnline { get; set; }
}