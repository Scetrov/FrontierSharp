using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.Models;

[ExcludeFromCodeCoverage]
public class CharacterMetadata {
    [JsonPropertyName("assembly_id")] public string? AssemblyId { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("url")] public string? Url { get; set; }

    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    [JsonIgnore] public string? RawValue { get; set; }
}

