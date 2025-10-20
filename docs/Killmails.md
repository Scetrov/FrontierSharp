# Killmails API

The Killmails API provides access to combat events and player kill information in EVE Frontier.

## Overview

Killmails record PvP combat events where a player character or ship is destroyed. Each killmail contains:
- Victim information
- Killer information
- Location (solar system)
- Loss type (ship or pod)
- Timestamp

## Available Methods

### GetAllKillmails

Fetches all recorded killmails.

**Signature:**
```csharp
Task<Result<IEnumerable<Killmail>>> GetAllKillmails(
    long limit = 100, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 100)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all killmails

**Example:**
```csharp
var result = await client.GetAllKillmails();
if (result.IsSuccess) {
    foreach (var killmail in result.Value) {
        Console.WriteLine($"{killmail.Killer.Name} killed {killmail.Victim.Name} " +
            $"in system {killmail.SolarSystemId} ({killmail.LossType})");
    }
}
```

**Response Model:**
```csharp
public class Killmail {
    public SmartCharacter Victim { get; set; }
    public SmartCharacter Killer { get; set; }
    public long SolarSystemId { get; set; }
    public LossType LossType { get; set; }
    public DateTimeOffset Time { get; set; }
}

public enum LossType {
    Ship = 0,
    Pod = 1
}
```

---

### GetKillmailPage

Fetches a single page of killmails for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<Killmail>>> GetKillmailPage(
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
var result = await client.GetKillmailPage(limit: 50, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total killmails: {result.Value.Metadata.Total}");
    foreach (var killmail in result.Value.Data) {
        Console.WriteLine($"- {killmail.Time:yyyy-MM-dd HH:mm}: {killmail.Victim.Name}");
    }
}
```

## API Endpoints

- **List:** `/v2/killmails` (paginated)

Note: There is currently no detail endpoint for individual killmails.

## Caching

Killmail queries are cached:
- **List queries**: Cached by limit and offset parameters

## Loss Types

- **Ship**: The victim lost a ship
- **Pod**: The victim lost their pod (escape capsule)

Losing a pod is more significant than losing a ship, as it may result in clone activation.

## Common Use Cases

### Recent Kills

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var recent = killmails.Value
        .OrderByDescending(k => k.Time)
        .Take(10)
        .ToList();
    
    foreach (var kill in recent) {
        Console.WriteLine($"{kill.Time:yyyy-MM-dd HH:mm}: {kill.Killer.Name} > {kill.Victim.Name}");
    }
}
```

### Player Kill Statistics

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var playerAddress = "0xb40b47e10a771cb0d997d866440459baab32df9c";
    
    var kills = killmails.Value.Count(k => 
        k.Killer.Address.Equals(playerAddress, StringComparison.OrdinalIgnoreCase));
    
    var losses = killmails.Value.Count(k => 
        k.Victim.Address.Equals(playerAddress, StringComparison.OrdinalIgnoreCase));
    
    Console.WriteLine($"Kills: {kills}");
    Console.WriteLine($"Losses: {losses}");
    Console.WriteLine($"K/D Ratio: {(double)kills / Math.Max(losses, 1):F2}");
}
```

### Most Dangerous Systems

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var systemKills = killmails.Value
        .GroupBy(k => k.SolarSystemId)
        .Select(g => new {
            SystemId = g.Key,
            Kills = g.Count()
        })
        .OrderByDescending(x => x.Kills)
        .Take(10)
        .ToList();
    
    // Get system names
    var systems = await client.GetAllSolarSystems();
    if (systems.IsSuccess) {
        var systemMap = systems.Value.ToDictionary(s => s.Id, s => s.Name);
        
        foreach (var item in systemKills) {
            var systemName = systemMap.TryGetValue(item.SystemId, out var name) 
                ? name : "Unknown";
            Console.WriteLine($"{systemName}: {item.Kills} kills");
        }
    }
}
```

### Top Killers

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var topKillers = killmails.Value
        .GroupBy(k => k.Killer.Address)
        .Select(g => new {
            Killer = g.First().Killer,
            Kills = g.Count()
        })
        .OrderByDescending(x => x.Kills)
        .Take(10)
        .ToList();
    
    Console.WriteLine("Top Killers:");
    foreach (var item in topKillers) {
        Console.WriteLine($"{item.Killer.Name}: {item.Kills} kills");
    }
}
```

### Ship vs Pod Losses

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var shipLosses = killmails.Value.Count(k => k.LossType == LossType.Ship);
    var podLosses = killmails.Value.Count(k => k.LossType == LossType.Pod);
    
    Console.WriteLine($"Ship losses: {shipLosses}");
    Console.WriteLine($"Pod losses: {podLosses}");
    Console.WriteLine($"Pod loss rate: {(double)podLosses / shipLosses:P}");
}
```

### Kills by Time Period

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var startDate = DateTimeOffset.UtcNow.AddDays(-7);
    
    var recentKills = killmails.Value
        .Where(k => k.Time >= startDate)
        .GroupBy(k => k.Time.Date)
        .Select(g => new {
            Date = g.Key,
            Kills = g.Count()
        })
        .OrderBy(x => x.Date)
        .ToList();
    
    Console.WriteLine("Kills per day (last 7 days):");
    foreach (var item in recentKills) {
        Console.WriteLine($"{item.Date:yyyy-MM-dd}: {item.Kills} kills");
    }
}
```

### Revenge Tracker

```csharp
var killmails = await client.GetAllKillmails();
if (killmails.IsSuccess) {
    var playerAddress = "0xb40b47e10a771cb0d997d866440459baab32df9c";
    
    // Find who killed you
    var killers = killmails.Value
        .Where(k => k.Victim.Address.Equals(playerAddress, StringComparison.OrdinalIgnoreCase))
        .Select(k => k.Killer.Address)
        .Distinct()
        .ToList();
    
    // Check if you got revenge
    foreach (var killerAddress in killers) {
        var revenge = killmails.Value.Any(k => 
            k.Killer.Address.Equals(playerAddress, StringComparison.OrdinalIgnoreCase) &&
            k.Victim.Address.Equals(killerAddress, StringComparison.OrdinalIgnoreCase));
        
        var killer = killmails.Value.First(k => k.Killer.Address.Equals(killerAddress)).Killer;
        Console.WriteLine($"{killer.Name}: {(revenge ? "Revenge taken ✓" : "Still hunting")}");
    }
}
```

## Time Information

- All timestamps are in UTC
- Format: ISO 8601 DateTimeOffset
- Useful for time-based analysis and trends

## Notes

- Killmails are permanent records
- Both victim and killer information includes address, name, and ID
- Solar system ID references the location of the kill
- No detail endpoint exists (all information is in the list)
- Killmails are ordered by time (most recent first in the API)

## Performance Considerations

- Killmail data grows over time
- Consider filtering by date for recent analysis
- Use pagination for large result sets
- Cache results for statistical analysis

## Error Handling

Common errors:
- **Network errors**: API unavailable or timeout
- **Deserialization errors**: Unexpected response format

Example:
```csharp
var result = await client.GetAllKillmails();
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

## Related APIs

- Use [Smart Characters API](./SmartCharacters.md) to get detailed victim/killer information
- Use [Solar Systems API](./SolarSystems.md) to get location details
- Use [Smart Assemblies API](./SmartAssemblies.md) to understand what ships were lost
- Use [Tribes API](./Tribes.md) to analyze corporation-level combat statistics
