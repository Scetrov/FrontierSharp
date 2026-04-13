using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrontierSharp.SuiClient.GraphQl;

[ExcludeFromCodeCoverage]
public class ObjectsQueryData {
    [JsonPropertyName("objects")] public ObjectConnection Objects { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class ObjectConnection {
    [JsonPropertyName("nodes")] public List<ObjectNode> Nodes { get; set; } = [];

    [JsonPropertyName("pageInfo")] public PageInfo PageInfo { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class ObjectNode {
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;

    [JsonPropertyName("asMoveObject")] public MoveObjectData? AsMoveObject { get; set; }
}

[ExcludeFromCodeCoverage]
public class MoveObjectData {
    [JsonPropertyName("contents")] public MoveContents? Contents { get; set; }
}

[ExcludeFromCodeCoverage]
public class MoveContents {
    [JsonPropertyName("json")] public JsonElement Json { get; set; }

    [JsonPropertyName("type")] public MoveTypeInfo? Type { get; set; }
}

[ExcludeFromCodeCoverage]
public class MoveTypeInfo {
    [JsonPropertyName("repr")] public string Repr { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
public class PageInfo {
    [JsonPropertyName("hasNextPage")] public bool HasNextPage { get; set; }

    [JsonPropertyName("endCursor")] public string? EndCursor { get; set; }
}

