using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.GraphQl;

[ExcludeFromCodeCoverage]
public class GraphQlRequest {
    [JsonPropertyName("query")] public string Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")] public Dictionary<string, object?>? Variables { get; set; }
}

