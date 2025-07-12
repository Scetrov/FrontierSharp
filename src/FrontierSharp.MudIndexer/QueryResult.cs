using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.MudIndexer;

[ExcludeFromCodeCoverage]
public class QueryResult {
    public string[] Headers { get; set; } = [];
    public IEnumerable<object?[]> Rows { get; set; } = [];
}