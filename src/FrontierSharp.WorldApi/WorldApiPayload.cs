using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi;

public class WorldApiPayload<T> {
    [JsonPropertyName("data")] public IEnumerable<T> Data { get; set; } = [];
    [JsonPropertyName("metadata")] public WorldApiMetadata Metadata { get; set; } = new();
}