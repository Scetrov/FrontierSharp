using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontierSharp.WorldApi.Models;

[ExcludeFromCodeCoverage]
public class WorldApiConfig {
    [JsonPropertyName("chainId")] public long ChainId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("nativeCurrency")] public NativeCurrency NativeCurrency { get; set; } = new();
    [JsonPropertyName("contracts")] public Contracts Contracts { get; set; } = new();
    [JsonPropertyName("rpcUrls")] public RpcUrls RpcUrls { get; set; } = new();
    [JsonPropertyName("blockExplorerUrl")] public string BlockExplorerUrl { get; set; } = string.Empty;
    [JsonPropertyName("metadataApiUrl")] public string MetadataApiUrl { get; set; } = string.Empty;
    [JsonPropertyName("ipfsApiUrl")] public string IpfsApiUrl { get; set; } = string.Empty;
    [JsonPropertyName("indexerUrl")] public string IndexerUrl { get; set; } = string.Empty;
    [JsonPropertyName("vaultDappUrl")] public string VaultDappUrl { get; set; } = string.Empty;
    [JsonPropertyName("walletApiUrl")] public string WalletApiUrl { get; set; } = string.Empty;
    [JsonPropertyName("baseDappUrl")] public string BaseDappUrl { get; set; } = string.Empty;
    [JsonPropertyName("systems")] public Dictionary<string, string> Systems { get; set; } = new();
    [JsonPropertyName("itemTypeIDs")] public ItemTypeIds ItemTypeIDs { get; set; } = new();
    [JsonPropertyName("exchangeWalletAddress")] public string ExchangeWalletAddress { get; set; } = string.Empty;
    [JsonPropertyName("EVEToLuxExchangeRate")] public long EVEToLuxExchangeRate { get; set; }
    [JsonPropertyName("podPublicSigningKey")] public string PodPublicSigningKey { get; set; } = string.Empty;
    [JsonPropertyName("cycleStartDate")] public DateTimeOffset CycleStartDate { get; set; }
}

[ExcludeFromCodeCoverage]
public class NativeCurrency {
    [JsonPropertyName("decimals")] public int Decimals { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("symbol")] public string Symbol { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
public class Contracts {
    [JsonPropertyName("contractsVersion")] public string ContractsVersion { get; set; } = string.Empty;
    [JsonPropertyName("world")] public ContractAddress World { get; set; } = new();
    [JsonPropertyName("eveToken")] public ContractAddress EveToken { get; set; } = new();
    [JsonPropertyName("forwarder")] public ContractAddress Forwarder { get; set; } = new();
    [JsonPropertyName("lensSeller")] public ContractAddress LensSeller { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class ContractAddress {
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
public class RpcUrls {
    [JsonPropertyName("default")] public RpcUrl Default { get; set; } = new();
    [JsonPropertyName("public")] public RpcUrl Public { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class RpcUrl {
    [JsonPropertyName("http")] public string Http { get; set; } = string.Empty;
    [JsonPropertyName("webSocket")] public string WebSocket { get; set; } = string.Empty;
}

[ExcludeFromCodeCoverage]
public class ItemTypeIds {
    [JsonPropertyName("fuel")] public long Fuel { get; set; }
}
