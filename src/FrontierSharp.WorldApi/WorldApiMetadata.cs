using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi;

[ExcludeFromCodeCoverage]
public class WorldApiMetadata {
    [JsonPropertyName("total")] public long Total { get; set; } = 0;
    [JsonPropertyName("limit")] public long Limit { get; set; } = 100;
    [JsonPropertyName("offset")] public long Offset { get; set; } = 0;
}