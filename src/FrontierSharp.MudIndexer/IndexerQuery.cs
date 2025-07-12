using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.MudIndexer;

[ExcludeFromCodeCoverage]
public class IndexerQuery {
    [JsonPropertyName("address")]
    public required string Address { get; init; }

    [JsonPropertyName("query")]
    public required string Query { get; init; }
}