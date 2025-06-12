using System.Text.Json.Serialization;
using FrontierSharp.HttpClient.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class FunctionalRouteSegment {
    [JsonPropertyName("deploy_from")] public required string DeployFrom { get; set; }

    [JsonPropertyName("deploy_to")] public required string DeployTo { get; set; }

    [JsonPropertyName("fuel_required")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelRequired { get; set; }

    [JsonPropertyName("distance")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int Distance { get; set; }

    [JsonPropertyName("jumps")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int Jumps { get; set; }

    [JsonPropertyName("ship_path")] public required IEnumerable<ShipPathSegment> ShipPath { get; set; }
}