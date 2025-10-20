# Smart Assemblies API

The Smart Assemblies API provides access to ships and structures in the EVE Frontier game world.

## Overview

Smart Assemblies are blockchain-based entities representing:
- Player ships
- Stations and structures
- Other deployable assets

Each assembly has:
- Unique blockchain ID (BigInteger)
- Name and type
- State (active, destroyed, etc.)
- Owner address
- Location (solar system)
- Manufacturing information

## Available Methods

### GetAllSmartAssemblies

Fetches all smart assemblies with their solar system information.

**Signature:**
```csharp
Task<Result<IEnumerable<SmartAssemblyWithSolarSystem>>> GetAllSmartAssemblies(
    long limit = 100, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 100)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all assemblies with solar system data

**Example:**
```csharp
var result = await client.GetAllSmartAssemblies();
if (result.IsSuccess) {
    foreach (var assembly in result.Value) {
        Console.WriteLine($"{assembly.Name} in {assembly.SolarSystem.Name}");
    }
}
```

**Response Model:**
```csharp
public class SmartAssemblyWithSolarSystem : SmartAssembly {
    public SolarSystem SolarSystem { get; set; }
}

public class SmartAssembly {
    public BigInteger SmartObjectId { get; set; }
    public string Name { get; set; }
    public long TypeId { get; set; }
    public string OwnerAddress { get; set; }
    public SmartAssemblyState State { get; set; }
    public SmartAssemblyType SmartAssemblyType { get; set; }
    public long SolarSystemId { get; set; }
}
```

---

### GetSmartAssemblyPage

Fetches a single page of smart assemblies for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<SmartAssemblyWithSolarSystem>>> GetSmartAssemblyPage(
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
var result = await client.GetSmartAssemblyPage(limit: 50, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total assemblies: {result.Value.Metadata.Total}");
    foreach (var assembly in result.Value.Data) {
        Console.WriteLine($"- {assembly.Name} ({assembly.SmartAssemblyType})");
    }
}
```

---

### GetSmartAssemblyById

Fetches detailed information about a specific smart assembly.

**Signature:**
```csharp
Task<Result<SmartAssemblyDetail>> GetSmartAssemblyById(
    BigInteger id, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `id`: Smart assembly blockchain ID (BigInteger)
- `cancellationToken`: Optional cancellation token

**Returns:** Detailed assembly information including type details and manufacturing info

**Example:**
```csharp
var assemblyId = BigInteger.Parse("12345678901234567890");
var result = await client.GetSmartAssemblyById(assemblyId);
if (result.IsSuccess) {
    var assembly = result.Value;
    Console.WriteLine($"Name: {assembly.Name}");
    Console.WriteLine($"Type: {assembly.SmartAssemblyType}");
    Console.WriteLine($"State: {assembly.State}");
    Console.WriteLine($"Owner: {assembly.OwnerAddress}");
    Console.WriteLine($"Type Details: {assembly.TypeDetails.Name}");
    
    if (assembly.Manufacturing != null) {
        Console.WriteLine($"\nManufacturing:");
        Console.WriteLine($"  Input: {assembly.Manufacturing.Input}");
        Console.WriteLine($"  Output: {assembly.Manufacturing.Output}");
    }
}
```

**Response Model:**
```csharp
public class SmartAssemblyDetail : SmartAssembly {
    public DateTimeOffset CreatedAt { get; set; }
    public GameType TypeDetails { get; set; }
    public Manufacturing? Manufacturing { get; set; }
}

public enum SmartAssemblyState {
    Unknown = 0,
    Online = 1,
    Anchored = 2,
    Offline = 3,
    Destroyed = 4
}

public enum SmartAssemblyType {
    Unknown = 0,
    Ship = 1,
    Station = 2,
    Structure = 3
}
```

## API Endpoints

- **List:** `/v2/smartassemblies` (paginated)
- **Detail:** `/v2/smartassemblies/{id}`

## Caching

All assembly queries are cached:
- **List queries**: Cached by limit and offset parameters
- **Detail queries**: Cached by smart object ID

Cache key format: `WorldApi_Type_{SmartObjectId}`

## Common Use Cases

### Finding Ships in a Solar System

```csharp
var assemblies = await client.GetAllSmartAssemblies();
if (assemblies.IsSuccess) {
    var systemId = 30012580L;
    var shipsInSystem = assemblies.Value
        .Where(a => a.SolarSystemId == systemId && a.SmartAssemblyType == SmartAssemblyType.Ship)
        .ToList();
    
    foreach (var ship in shipsInSystem) {
        Console.WriteLine($"{ship.Name} owned by {ship.OwnerAddress}");
    }
}
```

### Listing Active Ships by Owner

```csharp
var assemblies = await client.GetAllSmartAssemblies();
if (assemblies.IsSuccess) {
    var ownerAddress = "0xb40b47e10a771cb0d997d866440459baab32df9c";
    var ownedShips = assemblies.Value
        .Where(a => a.OwnerAddress.Equals(ownerAddress, StringComparison.OrdinalIgnoreCase))
        .Where(a => a.State == SmartAssemblyState.Online || a.State == SmartAssemblyState.Anchored)
        .ToList();
    
    Console.WriteLine($"Active ships: {ownedShips.Count}");
    foreach (var ship in ownedShips) {
        Console.WriteLine($"  {ship.Name} in {ship.SolarSystem.Name}");
    }
}
```

### Finding Stations and Structures

```csharp
var assemblies = await client.GetAllSmartAssemblies();
if (assemblies.IsSuccess) {
    var structures = assemblies.Value
        .Where(a => a.SmartAssemblyType == SmartAssemblyType.Station || 
                   a.SmartAssemblyType == SmartAssemblyType.Structure)
        .GroupBy(a => a.SolarSystem.Name)
        .ToList();
    
    foreach (var group in structures) {
        Console.WriteLine($"{group.Key}: {group.Count()} structures");
    }
}
```

### Getting Manufacturing Details

```csharp
var assemblyId = BigInteger.Parse("12345678901234567890");
var result = await client.GetSmartAssemblyById(assemblyId);
if (result.IsSuccess && result.Value.Manufacturing != null) {
    var mfg = result.Value.Manufacturing;
    Console.WriteLine("Manufacturing Facility:");
    Console.WriteLine($"  Input: {mfg.Input}");
    Console.WriteLine($"  Output: {mfg.Output}");
    Console.WriteLine($"  Efficiency: {(double)mfg.Output / mfg.Input:P}");
}
```

## Assembly States

- **Unknown**: State not determined
- **Online**: Active and operational
- **Anchored**: Deployed but not active
- **Offline**: Not operational
- **Destroyed**: No longer exists

## Assembly Types

- **Unknown**: Type not determined
- **Ship**: Player-controlled vessel
- **Station**: Large stationary structure
- **Structure**: Deployable structure

## BigInteger IDs

Smart assembly IDs are very large numbers requiring `BigInteger`:
```csharp
// Parse from string
var id = BigInteger.Parse("12345678901234567890");

// Convert to string
var idString = id.ToString();

// Compare
if (assembly.SmartObjectId == id) { /* ... */ }
```

## Notes

- Smart assembly IDs are blockchain-based (very large numbers)
- Owner addresses are Ethereum-style wallet addresses
- State transitions are tracked on the blockchain
- Type details include game-specific information about the assembly class
- Manufacturing data is only available for certain structure types
- Solar system information is automatically included in list queries

## Error Handling

Common errors:
- **Assembly not found**: Invalid or non-existent ID
- **Invalid ID format**: Cannot parse BigInteger
- **Network errors**: API unavailable or timeout

Example:
```csharp
var result = await client.GetSmartAssemblyById(BigInteger.Zero);
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

## Related APIs

- Use [Solar Systems API](./SolarSystems.md) to get location details
- Use [Smart Characters API](./SmartCharacters.md) to get owner information
- Use [Game Types API](./GameTypes.md) to understand type IDs
