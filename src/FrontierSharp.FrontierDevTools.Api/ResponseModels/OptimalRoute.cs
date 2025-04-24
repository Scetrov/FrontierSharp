using System.Text.Json.Serialization;
using FrontierSharp.FrontierDevTools.Api.Serialization;

namespace FrontierSharp.FrontierDevTools.Api.ResponseModels;

public class OptimalRoute {
    [JsonPropertyName("start_system")] public required string StartSystem { get; set; }

    [JsonPropertyName("end_system")] public required string EndSystem { get; set; }

    [JsonPropertyName("total_gate_distance")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int TotalGateDistance { get; set; }

    [JsonPropertyName("ship")] public required string Ship { get; set; }

    [JsonPropertyName("fuel_type")] public required string FuelType { get; set; }

    [JsonPropertyName("fuel_volume")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelVolume { get; set; }

    [JsonPropertyName("fuel_used")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelUsed { get; set; }

    [JsonPropertyName("fuel_volume_used")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelVolumeUsed { get; set; }

    [JsonPropertyName("fuel_cost")]
    [JsonConverter(typeof(StringifiedDecimalConverter))]
    public decimal FuelCost { get; set; }

    [JsonPropertyName("systems_reached")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int SystemsReached { get; set; }

    [JsonPropertyName("gates_deployed")]
    [JsonConverter(typeof(StringifiedInt32Converter))]
    public int GatesDeployed { get; set; }
}