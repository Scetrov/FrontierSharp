# Tribes API

The Tribes API provides access to player corporations (tribes) and their member information.

## Overview

Tribes are player-run organizations in EVE Frontier. Each tribe has:
- Unique ID (starting with 98000xxx)
- Name and short name
- Member list with detailed information
- Tax rate and founding date
- Optional tribe URL

## Available Methods

### GetAllTribes

Fetches all tribes in the game world.

**Signature:**
```csharp
Task<Result<IEnumerable<Tribe>>> GetAllTribes(
    long limit = 100, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 100)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all tribes with basic information

**Example:**
```csharp
var result = await client.GetAllTribes();
if (result.IsSuccess) {
    foreach (var tribe in result.Value) {
        Console.WriteLine($"{tribe.Name} ({tribe.NameShort}) - {tribe.MemberCount} members");
    }
}
```

**Response Model:**
```csharp
public class Tribe {
    public int Id { get; set; }
    public string Name { get; set; }
    public string NameShort { get; set; }
    public string Description { get; set; }
    public int MemberCount { get; set; }
    public double TaxRate { get; set; }
    public string TribeUrl { get; set; }
    public DateTimeOffset FoundedAt { get; set; }
}
```

---

### GetTribesPage

Fetches a single page of tribes for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<Tribe>>> GetTribesPage(
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
var result = await client.GetTribesPage(limit: 10, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total tribes: {result.Value.Metadata.Total}");
    foreach (var tribe in result.Value.Data) {
        Console.WriteLine($"- {tribe.Name}");
    }
}
```

---

### GetTribeById

Fetches detailed information about a specific tribe, including all members.

**Signature:**
```csharp
Task<Result<TribeDetail>> GetTribeById(
    long id, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `id`: Tribe ID (typically starts with 98000xxx)
- `cancellationToken`: Optional cancellation token

**Returns:** Detailed tribe information including complete member list

**Example:**
```csharp
var result = await client.GetTribeById(98000314);
if (result.IsSuccess) {
    var tribe = result.Value;
    Console.WriteLine($"Tribe: {tribe.Name} ({tribe.NameShort})");
    Console.WriteLine($"Founded: {tribe.FoundedAt:yyyy-MM-dd}");
    Console.WriteLine($"Tax Rate: {tribe.TaxRate:P}");
    Console.WriteLine($"\nMembers ({tribe.Members.Count()}):");
    
    foreach (var member in tribe.Members) {
        Console.WriteLine($"  - {member.Name}");
        Console.WriteLine($"    Address: {member.Address}");
        Console.WriteLine($"    ID: {member.Id}");
    }
}
```

**Response Model:**
```csharp
public class TribeDetail : Tribe {
    public IEnumerable<TribeMember> Members { get; set; }
}

public class TribeMember {
    public string Address { get; set; }      // Wallet address
    public string Name { get; set; }         // Character name
    public BigInteger Id { get; set; }       // Character ID (large number)
}
```

## API Endpoints

- **List:** `/v2/tribes` (paginated)
- **Detail:** `/v2/tribes/{id}`

## Caching

All tribe queries are cached:
- **List queries**: Cached by limit and offset parameters
- **Detail queries**: Cached by tribe ID

Cache key format: `WorldApi_Tribe_{TribeId}`

## Common Use Cases

### Finding a Tribe by Name

```csharp
var allTribes = await client.GetAllTribes();
if (allTribes.IsSuccess) {
    var tribe = allTribes.Value
        .FirstOrDefault(t => t.Name.Contains("REAPERS", StringComparison.OrdinalIgnoreCase));
    
    if (tribe != null) {
        // Get full details
        var details = await client.GetTribeById(tribe.Id);
    }
}
```

### Listing All Tribe Members

```csharp
var result = await client.GetTribeById(98000314);
if (result.IsSuccess) {
    var memberAddresses = result.Value.Members
        .Select(m => m.Address)
        .ToList();
    
    Console.WriteLine($"Member addresses: {string.Join(", ", memberAddresses)}");
}
```

### Finding Tribes by Member Count

```csharp
var tribes = await client.GetAllTribes();
if (tribes.IsSuccess) {
    var largeTribes = tribes.Value
        .Where(t => t.MemberCount >= 10)
        .OrderByDescending(t => t.MemberCount)
        .ToList();
    
    foreach (var tribe in largeTribes) {
        Console.WriteLine($"{tribe.Name}: {tribe.MemberCount} members");
    }
}
```

## Notes

- Tribe IDs typically start with 98000xxx (e.g., 98000314)
- Member IDs are very large numbers (BigInteger type)
- Tax rates are represented as decimals (0.0 to 1.0)
- The member list is only available via `GetTribeById`
- Tribe URLs may be empty if not configured by the tribe

## Error Handling

Common errors:
- **Tribe not found**: Invalid tribe ID
- **Network errors**: API unavailable or timeout
- **Deserialization errors**: API response format changed

Example:
```csharp
var result = await client.GetTribeById(999999);
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```
