using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using FrontierSharp.SuiClient.JsonConverters;

namespace FrontierSharp.SuiClient.Models;

[ExcludeFromCodeCoverage]
public class Assembly {
    [JsonPropertyName("key")] public TenantItemId Key { get; set; } = new();

    [JsonPropertyName("type_id")]
    [JsonConverter(typeof(SuiU64Converter))]
    public ulong TypeId { get; set; }

    [JsonPropertyName("owner_cap_id")] public string OwnerCapId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [JsonConverter(typeof(MoveEnumConverter<AssemblyStatus>))]
    public AssemblyStatus Status { get; set; }

    [JsonPropertyName("location")]
    [JsonConverter(typeof(LocationHashStringConverter))]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("energy_source_id")] public string? EnergySourceId { get; set; }
}