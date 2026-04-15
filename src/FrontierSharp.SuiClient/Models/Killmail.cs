using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using FrontierSharp.SuiClient.JsonConverters;

namespace FrontierSharp.SuiClient.Models;

[ExcludeFromCodeCoverage]
public class Killmail {
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")] public TenantItemId Key { get; set; } = new();

    [JsonPropertyName("killer_id")]
    [JsonConverter(typeof(TenantItemIdItemIdConverter))]
    public ulong KillerId { get; set; }

    [JsonPropertyName("victim_id")]
    [JsonConverter(typeof(TenantItemIdItemIdConverter))]
    public ulong VictimId { get; set; }

    [JsonPropertyName("reported_by_character_id")]
    [JsonConverter(typeof(TenantItemIdItemIdConverter))]
    public ulong ReportedByCharacterId { get; set; }

    [JsonPropertyName("kill_timestamp")]
    [JsonConverter(typeof(UnixSecondsDateTimeOffsetConverter))]
    public DateTimeOffset KillTimestamp { get; set; }

    [JsonPropertyName("loss_type")]
    [JsonConverter(typeof(MoveEnumConverter<LossType>))]
    public LossType LossType { get; set; }

    [JsonPropertyName("solar_system_id")]
    [JsonConverter(typeof(TenantItemIdItemIdConverter))]
    public ulong SolarSystemId { get; set; }
}