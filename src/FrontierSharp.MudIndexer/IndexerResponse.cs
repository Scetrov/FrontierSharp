using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.HttpClient;

[ExcludeFromCodeCoverage]
public class IndexerResponse {
    public long BlockHeight { get; set; }
    public required IEnumerable<QueryResult> Result { get; set; }
}