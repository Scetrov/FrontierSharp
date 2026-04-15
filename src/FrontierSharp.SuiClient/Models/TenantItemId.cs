using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using FrontierSharp.SuiClient.JsonConverters;

namespace FrontierSharp.SuiClient.Models;

[ExcludeFromCodeCoverage]
public class TenantItemId {
    [JsonPropertyName("item_id")]
    [JsonConverter(typeof(SuiU64Converter))]
    public ulong ItemId { get; set; }

    [JsonPropertyName("tenant")] public string Tenant { get; set; } = string.Empty;
}