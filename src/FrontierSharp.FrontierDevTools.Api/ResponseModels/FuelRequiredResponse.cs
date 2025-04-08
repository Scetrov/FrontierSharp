using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class FuelRequiredResponse {
    [JsonPropertyName("fuel_required")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelRequired { get; set; }
}