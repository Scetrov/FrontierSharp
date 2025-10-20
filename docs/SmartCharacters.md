# Smart Characters API

The Smart Characters API provides access to player character information stored on the blockchain.

## Overview

Smart Characters represent player avatars in EVE Frontier. Each character has:
- Unique blockchain address
- Character name
- Blockchain ID
- Associated tribe membership (in detail view)
- Token balances (in detail view)
- Owned smart assemblies (in detail view)

## Available Methods

### GetAllSmartCharacters

Fetches all smart characters in the game.

**Signature:**
```csharp
Task<Result<IEnumerable<SmartCharacter>>> GetAllSmartCharacters(
    long limit = 100, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 100)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all characters with basic information

**Example:**
```csharp
var result = await client.GetAllSmartCharacters();
if (result.IsSuccess) {
    foreach (var character in result.Value) {
        Console.WriteLine($"{character.Name} - {character.Address}");
    }
}
```

**Response Model:**
```csharp
public class SmartCharacter {
    public string Address { get; set; }  // Wallet address (e.g., "0x...")
    public string Name { get; set; }     // Character name
    public string Id { get; set; }       // Blockchain ID
}
```

---

### GetSmartCharacterPage

Fetches a single page of smart characters for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<SmartCharacter>>> GetSmartCharacterPage(
    long limit = 100, 
    long offset = 0, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items to fetch (default: 100, max: 100)
- `offset`: Number of items to skip (default: 0)
- `cancellationToken`: Optional cancellation token

**Returns:** Paginated response with metadata

**Example:**
```csharp
var result = await client.GetSmartCharacterPage(limit: 20, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total characters: {result.Value.Metadata.Total}");
    foreach (var character in result.Value.Data) {
        Console.WriteLine($"- {character.Name} ({character.Address})");
    }
}
```

---

### GetSmartCharacterById

Fetches detailed information about a specific character by their wallet address.

**Signature:**
```csharp
Task<Result<SmartCharacterDetail>> GetSmartCharacterById(
    string address, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `address`: Character's wallet address (e.g., "0x1234...")
- `cancellationToken`: Optional cancellation token

**Returns:** Detailed character information including tribe, balances, and assets

**Example:**
```csharp
var result = await client.GetSmartCharacterById("0xb40b47e10a771cb0d997d866440459baab32df9c");
if (result.IsSuccess) {
    var character = result.Value;
    Console.WriteLine($"Name: {character.Name}");
    Console.WriteLine($"Tribe ID: {character.TribeId}");
    Console.WriteLine($"EVE Balance: {character.EveBalanceInWei}");
    Console.WriteLine($"Gas Balance: {character.GasBalanceInWei}");
    Console.WriteLine($"Portrait: {character.PortraitUrl}");
    
    Console.WriteLine($"\nOwned Assemblies ({character.SmartAssemblies.Count()}):");
    foreach (var assembly in character.SmartAssemblies) {
        Console.WriteLine($"  - {assembly.Name} (Type: {assembly.TypeId})");
    }
}
```

**Response Model:**
```csharp
public class SmartCharacterDetail : SmartCharacter {
    public int TribeId { get; set; }
    public BigInteger EveBalanceInWei { get; set; }
    public BigInteger GasBalanceInWei { get; set; }
    public IEnumerable<SmartAssembly> SmartAssemblies { get; set; }
    public string PortraitUrl { get; set; }
}
```

## API Endpoints

- **List:** `/v2/smartcharacters` (paginated)
- **Detail:** `/v2/smartcharacters/{address}`

## Caching

All character queries are cached:
- **List queries**: Cached by limit and offset parameters
- **Detail queries**: Cached by character address

Cache key format: `WorldApi_SmartCharacter_{Address}`

## Common Use Cases

### Finding a Character by Name

```csharp
var allCharacters = await client.GetAllSmartCharacters();
if (allCharacters.IsSuccess) {
    var character = allCharacters.Value
        .FirstOrDefault(c => c.Name.Equals("Scetrov", StringComparison.OrdinalIgnoreCase));
    
    if (character != null) {
        // Get full details
        var details = await client.GetSmartCharacterById(character.Address);
    }
}
```

### Checking Character Balances

```csharp
var result = await client.GetSmartCharacterById("0xb40b47e10a771cb0d997d866440459baab32df9c");
if (result.IsSuccess) {
    var character = result.Value;
    
    // Convert Wei to Ether (divide by 10^18)
    var eveBalance = (decimal)character.EveBalanceInWei / 1_000_000_000_000_000_000m;
    var gasBalance = (decimal)character.GasBalanceInWei / 1_000_000_000_000_000_000m;
    
    Console.WriteLine($"EVE: {eveBalance:N2}");
    Console.WriteLine($"GAS: {gasBalance:N2}");
}
```

### Listing All Characters in a Tribe

```csharp
// First get tribe details
var tribeResult = await client.GetTribeById(98000314);
if (tribeResult.IsSuccess) {
    var memberAddresses = tribeResult.Value.Members.Select(m => m.Address);
    
    // Get detailed info for each member
    foreach (var address in memberAddresses) {
        var charResult = await client.GetSmartCharacterById(address);
        if (charResult.IsSuccess) {
            Console.WriteLine($"{charResult.Value.Name}: {charResult.Value.SmartAssemblies.Count()} ships");
        }
    }
}
```

### Finding Characters with Specific Assets

```csharp
var characters = await client.GetAllSmartCharacters();
if (characters.IsSuccess) {
    foreach (var character in characters.Value.Take(10)) {  // Sample first 10
        var details = await client.GetSmartCharacterById(character.Address);
        if (details.IsSuccess && details.Value.SmartAssemblies.Any()) {
            Console.WriteLine($"{character.Name}: {details.Value.SmartAssemblies.Count()} assemblies");
        }
    }
}
```

## Address Format

Character addresses are Ethereum-style wallet addresses:
- Format: `0x` followed by 40 hexadecimal characters
- Example: `0xb40b47e10a771cb0d997d866440459baab32df9c`
- Case-insensitive but typically lowercase

## Balance Information

Balances are stored as `BigInteger` in Wei (1 Ether = 10^18 Wei):
- **EVE Balance**: In-game EVE tokens
- **Gas Balance**: Network gas tokens

Convert to Ether:
```csharp
var ether = (decimal)weiAmount / 1_000_000_000_000_000_000m;
```

## Notes

- Character addresses are unique identifiers
- Portrait URLs may point to external image services
- Smart assemblies list shows all owned ships/structures
- TribeId of 0 means the character is not in a tribe
- Balances are in Wei (very large numbers)

## Error Handling

Common errors:
- **Character not found**: Invalid or non-existent address
- **Invalid address format**: Malformed wallet address
- **Network errors**: API unavailable or timeout

Example:
```csharp
var result = await client.GetSmartCharacterById("invalid_address");
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

## Related APIs

- Use [Tribes API](./Tribes.md) to get tribe information for a character's `TribeId`
- Use [Smart Assemblies API](./SmartAssemblies.md) to get details about owned assemblies
