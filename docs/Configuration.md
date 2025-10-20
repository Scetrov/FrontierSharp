# Configuration API

The Configuration API provides access to EVE Frontier game configuration, including blockchain network details, contract addresses, and game constants.

## Overview

The configuration endpoint returns critical game infrastructure information:
- Blockchain network configuration (chain ID, RPC URLs)
- Smart contract addresses (World, EVE Token, Forwarder, etc.)
- API endpoints (metadata, IPFS, indexer, wallet)
- Game constants (cycle dates, exchange rates)
- System mappings and item type IDs

## Available Methods

### GetConfig

Fetches the complete game configuration.

**Signature:**
```csharp
Task<Result<IEnumerable<WorldApiConfig>>> GetConfig(
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `cancellationToken`: Optional cancellation token

**Returns:** Collection containing the configuration object(s)

**Example:**
```csharp
var result = await client.GetConfig();
if (result.IsSuccess) {
    var config = result.Value.First();
    Console.WriteLine($"Chain: {config.Name} (ID: {config.ChainId})");
    Console.WriteLine($"World Contract: {config.Contracts.World.Address}");
    Console.WriteLine($"RPC: {config.RpcUrls.Default.Http}");
}
```

**Response Model:**
```csharp
public class WorldApiConfig {
    public long ChainId { get; set; }
    public string Name { get; set; }
    public NativeCurrency NativeCurrency { get; set; }
    public Contracts Contracts { get; set; }
    public RpcUrls RpcUrls { get; set; }
    public string BlockExplorerUrl { get; set; }
    public string MetadataApiUrl { get; set; }
    public string IpfsApiUrl { get; set; }
    public string IndexerUrl { get; set; }
    public string VaultDappUrl { get; set; }
    public string WalletApiUrl { get; set; }
    public string BaseDappUrl { get; set; }
    public Dictionary<string, string> Systems { get; set; }
    public ItemTypeIds ItemTypeIDs { get; set; }
    public string ExchangeWalletAddress { get; set; }
    public long EVEToLuxExchangeRate { get; set; }
    public string PodPublicSigningKey { get; set; }
    public DateTimeOffset CycleStartDate { get; set; }
}

public class NativeCurrency {
    public int Decimals { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
}

public class Contracts {
    public string ContractsVersion { get; set; }
    public ContractAddress World { get; set; }
    public ContractAddress EveToken { get; set; }
    public ContractAddress Forwarder { get; set; }
    public ContractAddress LensSeller { get; set; }
}

public class ContractAddress {
    public string Address { get; set; }
}

public class RpcUrls {
    public RpcUrl Default { get; set; }
    public RpcUrl Public { get; set; }
}

public class RpcUrl {
    public string Http { get; set; }
    public string WebSocket { get; set; }
}

public class ItemTypeIds {
    public long Fuel { get; set; }
}
```

## API Endpoints

- **Config:** `/v2/config`

## Caching

Configuration queries are cached as the data is relatively static.

## Configuration Sections

### Blockchain Network

```csharp
var config = (await client.GetConfig()).Value.First();

Console.WriteLine($"Network: {config.Name}");
Console.WriteLine($"Chain ID: {config.ChainId}");
Console.WriteLine($"Native Currency: {config.NativeCurrency.Symbol} ({config.NativeCurrency.Decimals} decimals)");
Console.WriteLine($"RPC HTTP: {config.RpcUrls.Default.Http}");
Console.WriteLine($"RPC WebSocket: {config.RpcUrls.Default.WebSocket}");
Console.WriteLine($"Block Explorer: {config.BlockExplorerUrl}");
```

### Smart Contracts

```csharp
var config = (await client.GetConfig()).Value.First();

Console.WriteLine("Contract Addresses:");
Console.WriteLine($"  Version: {config.Contracts.ContractsVersion}");
Console.WriteLine($"  World: {config.Contracts.World.Address}");
Console.WriteLine($"  EVE Token: {config.Contracts.EveToken.Address}");
Console.WriteLine($"  Forwarder: {config.Contracts.Forwarder.Address}");
Console.WriteLine($"  Lens Seller: {config.Contracts.LensSeller.Address}");
```

### API Endpoints

```csharp
var config = (await client.GetConfig()).Value.First();

Console.WriteLine("API Endpoints:");
Console.WriteLine($"  Metadata: {config.MetadataApiUrl}");
Console.WriteLine($"  IPFS: {config.IpfsApiUrl}");
Console.WriteLine($"  Indexer: {config.IndexerUrl}");
Console.WriteLine($"  Wallet: {config.WalletApiUrl}");
Console.WriteLine($"  Vault dApp: {config.VaultDappUrl}");
Console.WriteLine($"  Base dApp: {config.BaseDappUrl}");
```

## Common Use Cases

### Initialize Web3 Provider

```csharp
var config = (await client.GetConfig()).Value.First();

// Use configuration to connect to blockchain
var web3Provider = new Web3(config.RpcUrls.Default.Http);
Console.WriteLine($"Connected to {config.Name} (Chain ID: {config.ChainId})");
```

### Interact with Smart Contracts

```csharp
var config = (await client.GetConfig()).Value.First();

// Get World contract address for on-chain interactions
string worldContractAddress = config.Contracts.World.Address;
string eveTokenAddress = config.Contracts.EveToken.Address;

Console.WriteLine($"World Contract: {worldContractAddress}");
Console.WriteLine($"EVE Token: {eveTokenAddress}");
```

### System ID Lookup

```csharp
var config = (await client.GetConfig()).Value.First();

// Systems dictionary maps system names to IDs or other metadata
foreach (var system in config.Systems) {
    Console.WriteLine($"{system.Key}: {system.Value}");
}
```

### Exchange Rate Calculations

```csharp
var config = (await client.GetConfig()).Value.First();

long eveAmount = 1000;
long luxAmount = eveAmount * config.EVEToLuxExchangeRate;

Console.WriteLine($"{eveAmount} EVE = {luxAmount} LUX");
Console.WriteLine($"Exchange rate: {config.EVEToLuxExchangeRate} LUX per EVE");
Console.WriteLine($"Exchange wallet: {config.ExchangeWalletAddress}");
```

### Cycle Information

```csharp
var config = (await client.GetConfig()).Value.First();

var cycleStart = config.CycleStartDate;
var now = DateTimeOffset.UtcNow;
var elapsed = now - cycleStart;

Console.WriteLine($"Cycle started: {cycleStart:yyyy-MM-dd HH:mm:ss UTC}");
Console.WriteLine($"Days since cycle start: {elapsed.TotalDays:F1}");
```

### Fuel Type ID

```csharp
var config = (await client.GetConfig()).Value.First();

long fuelTypeId = config.ItemTypeIDs.Fuel;
Console.WriteLine($"Default fuel type ID: {fuelTypeId}");

// Use with Game Types API
var fuelType = await client.GetGameTypeById((int)fuelTypeId);
if (fuelType.IsSuccess) {
    Console.WriteLine($"Default fuel: {fuelType.Value.Name}");
}
```

### Pod Signature Verification

```csharp
var config = (await client.GetConfig()).Value.First();

string publicKey = config.PodPublicSigningKey;
Console.WriteLine($"Pod public signing key: {publicKey}");

// Use for verifying pod-signed transactions or messages
```

### Configuration-Based Client Setup

```csharp
var config = (await client.GetConfig()).Value.First();

// Use configuration to initialize other services
var metadataClient = new HttpClient { 
    BaseAddress = new Uri(config.MetadataApiUrl) 
};

var ipfsClient = new HttpClient { 
    BaseAddress = new Uri(config.IpfsApiUrl) 
};

Console.WriteLine($"Metadata API: {config.MetadataApiUrl}");
Console.WriteLine($"IPFS API: {config.IpfsApiUrl}");
```

## Configuration Properties

### ChainId
The blockchain network identifier. Used for transaction signing and network validation.

### Name
Human-readable name of the blockchain network (e.g., "EVE Frontier Testnet").

### NativeCurrency
Details about the blockchain's native currency (name, symbol, decimals).

### Contracts
Addresses of all deployed smart contracts. Critical for on-chain interactions.

### RpcUrls
HTTP and WebSocket endpoints for blockchain RPC communication.

### Systems
Dictionary mapping system names or identifiers to metadata values.

### ItemTypeIDs
Known item type IDs for common game items (e.g., default fuel type).

### ExchangeWalletAddress
Wallet address for currency exchange operations.

### EVEToLuxExchangeRate
Conversion rate from EVE tokens to LUX (the native currency).

### PodPublicSigningKey
Public key used to verify pod-signed operations.

### CycleStartDate
Start date of the current game cycle. Used for time-based mechanics.

## Notes

- Configuration is relatively static but may change between game updates
- Always fetch fresh configuration when connecting to a new environment
- Contract addresses differ between testnet and mainnet
- RPC URLs should be used for all on-chain interactions
- Configuration includes multiple API endpoints for different services

## Performance Considerations

- Configuration changes infrequently - safe to cache for application lifetime
- Fetch once at application startup
- Avoid repeated calls within the same session
- Consider fetching on reconnect or environment change

## Error Handling

Common errors:
- **Network errors**: API unavailable or timeout
- **Deserialization errors**: Unexpected response format (rare for config)

Example:
```csharp
var result = await client.GetConfig();
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
    // Fall back to default configuration or exit
}
```

## Related APIs

- Configuration data supports all other API operations
- Contract addresses are used for on-chain transactions
- Item type IDs reference [Game Types API](./GameTypes.md)
- System mappings relate to [Solar Systems API](./SolarSystems.md)
- RPC URLs enable direct blockchain queries alongside World API calls

## Best Practices

1. **Fetch once**: Get configuration at application startup and cache it
2. **Validate chain ID**: Ensure transactions target the correct network
3. **Use RPC endpoints**: Prefer configured RPC URLs over hardcoded values
4. **Check contract version**: Verify your code is compatible with the deployed contracts
5. **Handle updates**: Be prepared for configuration changes between environments
6. **Secure keys**: Handle `PodPublicSigningKey` appropriately for signature verification
