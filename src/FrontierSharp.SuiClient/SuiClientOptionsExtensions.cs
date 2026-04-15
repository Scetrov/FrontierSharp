namespace FrontierSharp.SuiClient;

public static class SuiClientOptionsExtensions {
    public static SuiClientOptions WithNetwork(this SuiClientOptions options, SuiNetwork network) {
        if (options == null) throw new ArgumentNullException(nameof(options));

        options.GraphQlEndpoint = network switch {
            SuiNetwork.Mainnet => "https://graphql.mainnet.sui.io/graphql",
            SuiNetwork.Testnet => "https://graphql.testnet.sui.io/graphql",
            SuiNetwork.Devnet => "https://graphql.devnet.sui.io/graphql",
            _ => throw new ArgumentOutOfRangeException(nameof(network), network, "Unsupported Sui network.")
        };

        return options;
    }

    public static SuiClientOptions WithDefaultWorldPackageAddress(this SuiClientOptions options, string worldPackageAddress) {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(worldPackageAddress))
            throw new ArgumentException("World package address cannot be null or empty.", nameof(worldPackageAddress));

        options.DefaultWorldPackageAddress = worldPackageAddress;
        return options;
    }
}