using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.HttpClient;

[ExcludeFromCodeCoverage]
public class IndexerQuery {
    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("query")]
    public required string Query { get; set; }
}