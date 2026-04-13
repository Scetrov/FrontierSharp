using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.GraphQl;

[ExcludeFromCodeCoverage]
public class GraphQlResponse<T> {
    [JsonPropertyName("data")] public T? Data { get; set; }

    [JsonPropertyName("errors")] public List<GraphQlError>? Errors { get; set; }
}

[ExcludeFromCodeCoverage]
public class GraphQlError {
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;

    [JsonPropertyName("locations")] public List<GraphQlErrorLocation>? Locations { get; set; }
}

[ExcludeFromCodeCoverage]
public class GraphQlErrorLocation {
    [JsonPropertyName("line")] public int Line { get; set; }

    [JsonPropertyName("column")] public int Column { get; set; }
}

