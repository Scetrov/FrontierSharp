using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.HttpClient;

[ExcludeFromCodeCoverage]
public class MudIndexerOptions {
    public required string WorldAddress { get; set; }
    public required string IndexerAddress { get; set; }

    public string QueryEndpoint { get; set; } = "/q";

    public string IndexerUrl => $"{IndexerAddress}{QueryEndpoint}";
}