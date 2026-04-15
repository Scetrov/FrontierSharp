using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.SuiClient.GraphQl;

[ExcludeFromCodeCoverage]
public class GraphQlQueryOptions {
    public bool BypassCache { get; set; }
}