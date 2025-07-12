using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.MudIndexer;

[ExcludeFromCodeCoverage]
public class MudIndexerOptions {
    public required string WorldAddress { get; init; }
    public required string IndexerAddress { get; init; }

    public string QueryEndpoint { get; init; } = "/q";
}