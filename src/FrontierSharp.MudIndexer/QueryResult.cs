using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.HttpClient;

[ExcludeFromCodeCoverage]
public class QueryResult {
    public string[] Headers { get; set; } = [];
    public IEnumerable<object?[]> Rows { get; set; } = [];
}