using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.MudIndexer;

[ExcludeFromCodeCoverage]
public class IndexerResponse {
    public long BlockHeight { get; init; }
    public required IEnumerable<QueryResult> Result { get; init; }
}