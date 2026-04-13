using System.Diagnostics.CodeAnalysis;

namespace FrontierSharp.SuiClient;

[ExcludeFromCodeCoverage]
public class SuiClientOptions {
    public string HttpClientName { get; set; } = "SuiGraphQl";
    public string GraphQlEndpoint { get; set; } = "https://graphql.testnet.sui.io/graphql";
    public string WorldPackageAddress { get; set; } = "0xd2fd1224f881e7a705dbc211888af11655c315f2ee0f03fe680fc3176e6e4780";
    public TimeSpan GraphQlCacheDuration { get; set; } = TimeSpan.FromSeconds(30);
}

