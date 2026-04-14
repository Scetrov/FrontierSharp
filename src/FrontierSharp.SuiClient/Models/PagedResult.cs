using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.SuiClient.Models;

[ExcludeFromCodeCoverage]
public class PagedResult<T> {
    public IReadOnlyList<T> Data { get; set; } = [];
    public bool HasNextPage { get; set; }
    public Cursor? EndCursor { get; set; }
}