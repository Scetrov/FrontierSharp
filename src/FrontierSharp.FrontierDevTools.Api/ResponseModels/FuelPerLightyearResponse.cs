using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class FuelPerLightyearResponse {
    [JsonPropertyName("fuel_per_lightyear")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelPerLightyear { get; set; }
}