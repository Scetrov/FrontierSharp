using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using FrontierSharp.SuiClient.JsonConverters;

namespace FrontierSharp.SuiClient.Models;

[ExcludeFromCodeCoverage]
public class Character {
    [JsonPropertyName("key")] public TenantItemId Key { get; set; } = new();

    [JsonPropertyName("tribe_id")] public uint TribeId { get; set; }

    [JsonPropertyName("character_address")]
    public string CharacterAddress { get; set; } = string.Empty;

    [JsonPropertyName("owner_cap_id")] public string OwnerCapId { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    [JsonConverter(typeof(CharacterMetadataConverter))]
    public CharacterMetadata? Metadata { get; set; }
}