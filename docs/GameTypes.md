# Game Types API

The Game Types API provides access to item type definitions, including ships, modules, resources, and other game entities in EVE Frontier.

## Overview

Game types define the characteristics of all items and entities in the game. Each type includes:
- Physical properties (mass, radius, volume)
- Classification (category, group)
- Metadata (name, description, icon)
- Inventory details (portion size)

## Available Methods

### GetAllGameTypes

Fetches all game type definitions.

**Signature:**
```csharp
Task<Result<IEnumerable<GameType>>> GetAllGameTypes(
    long limit = 100, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 100)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all game types

**Example:**
```csharp
var result = await client.GetAllGameTypes();
if (result.IsSuccess) {
    foreach (var type in result.Value) {
        Console.WriteLine($"{type.Name} ({type.CategoryName} > {type.GroupName})");
    }
}
```

**Response Model:**
```csharp
public class GameType {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Mass { get; set; }
    public double Radius { get; set; }
    public double Volume { get; set; }
    public int PortionSize { get; set; }
    public string GroupName { get; set; }
    public int GroupId { get; set; }
    public string CategoryName { get; set; }
    public int CategoryId { get; set; }
    public string IconUrl { get; set; }
}
```

---

### GetGameTypePage

Fetches a single page of game types for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<GameType>>> GetGameTypePage(
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
var result = await client.GetGameTypePage(limit: 50, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total types: {result.Value.Metadata.Total}");
    foreach (var type in result.Value.Data) {
        Console.WriteLine($"- {type.Name} (ID: {type.Id})");
    }
}
```

---

### GetGameTypeById

Fetches detailed information for a specific game type by its ID.

**Signature:**
```csharp
Task<Result<GameType>> GetGameTypeById(
    int id, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `id`: The game type ID to fetch
- `cancellationToken`: Optional cancellation token

**Returns:** Single game type details

**Example:**
```csharp
var result = await client.GetGameTypeById(12005);
if (result.IsSuccess) {
    var type = result.Value;
    Console.WriteLine($"Name: {type.Name}");
    Console.WriteLine($"Description: {type.Description}");
    Console.WriteLine($"Category: {type.CategoryName}");
    Console.WriteLine($"Group: {type.GroupName}");
    Console.WriteLine($"Mass: {type.Mass} kg");
    Console.WriteLine($"Volume: {type.Volume} m³");
}
```

## API Endpoints

- **List:** `/v2/game-types` (paginated)
- **Detail:** `/v2/game-types/{id}`

## Caching

Game type queries are cached:
- **List queries**: Cached by limit and offset parameters
- **Detail queries**: Cached by type ID

## Type Hierarchy

Game types are organized in a three-level hierarchy:

1. **Category**: Top-level classification (Ships, Modules, Resources, etc.)
2. **Group**: Mid-level classification (Frigates, Cruisers, Mining Modules, etc.)
3. **Type**: Individual items (Venture, Covetor, Mining Laser I, etc.)

## Physical Properties

### Mass
- Measured in kilograms (kg)
- Affects ship inertia and agility
- Important for cargo capacity calculations

### Volume
- Measured in cubic meters (m³)
- Determines cargo space requirements
- Critical for logistics and hauling

### Radius
- Measured in meters (m)
- Affects collision detection
- Relevant for signature radius calculations

## Common Use Cases

### Find Ships

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    var ships = types.Value
        .Where(t => t.CategoryName.Equals("Ship", StringComparison.OrdinalIgnoreCase))
        .OrderBy(t => t.Name)
        .ToList();
    
    foreach (var ship in ships) {
        Console.WriteLine($"{ship.Name}: {ship.Mass:N0} kg, {ship.Volume:N0} m³");
    }
}
```

### Browse by Category

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    var categories = types.Value
        .GroupBy(t => t.CategoryName)
        .Select(g => new {
            Category = g.Key,
            Count = g.Count()
        })
        .OrderByDescending(x => x.Count)
        .ToList();
    
    foreach (var cat in categories) {
        Console.WriteLine($"{cat.Category}: {cat.Count} types");
    }
}
```

### Find Types in a Group

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    var frigates = types.Value
        .Where(t => t.GroupName.Equals("Frigate", StringComparison.OrdinalIgnoreCase))
        .OrderBy(t => t.Name)
        .ToList();
    
    Console.WriteLine($"Frigates ({frigates.Count}):");
    foreach (var frigate in frigates) {
        Console.WriteLine($"- {frigate.Name}");
    }
}
```

### Cargo Optimization

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    double cargoCapacity = 5000; // m³
    
    var cargo = types.Value
        .Where(t => t.CategoryName == "Resource")
        .Select(t => new {
            Type = t,
            Units = (int)(cargoCapacity / t.Volume),
            TotalMass = (cargoCapacity / t.Volume) * t.Mass
        })
        .OrderBy(x => x.TotalMass)
        .ToList();
    
    foreach (var item in cargo.Take(10)) {
        Console.WriteLine($"{item.Type.Name}: {item.Units} units, {item.TotalMass:N0} kg");
    }
}
```

### Type Lookup by Name

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    var searchName = "Ore";
    
    var matches = types.Value
        .Where(t => t.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase))
        .OrderBy(t => t.Name)
        .ToList();
    
    Console.WriteLine($"Types containing '{searchName}':");
    foreach (var match in matches) {
        Console.WriteLine($"- {match.Name} ({match.GroupName})");
    }
}
```

### Compare Ship Classes

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    var shipGroups = new[] { "Frigate", "Destroyer", "Cruiser", "Battleship" };
    
    foreach (var group in shipGroups) {
        var ships = types.Value
            .Where(t => t.GroupName.Equals(group, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (ships.Any()) {
            var avgMass = ships.Average(s => s.Mass);
            var avgVolume = ships.Average(s => s.Volume);
            Console.WriteLine($"{group}: {ships.Count} ships, avg mass {avgMass:N0} kg, avg volume {avgVolume:N0} m³");
        }
    }
}
```

### Mining Resource Reference

```csharp
var types = await client.GetAllGameTypes();
if (types.IsSuccess) {
    var resources = types.Value
        .Where(t => t.GroupName.Contains("Ore", StringComparison.OrdinalIgnoreCase))
        .OrderBy(t => t.Volume)
        .ToList();
    
    Console.WriteLine("Mining Resources (by volume):");
    foreach (var resource in resources) {
        Console.WriteLine($"{resource.Name}: {resource.Volume:F2} m³ per unit");
    }
}
```

## Portion Size

The `PortionSize` property indicates the default stack size or unit quantity for the type. This is relevant for:
- Resource stacking in inventory
- Manufacturing requirements
- Market orders

## Icon URLs

Each type includes an `IconUrl` pointing to a visual representation. Useful for:
- UI displays
- Type identification
- Visual catalogs

## Notes

- Type IDs are stable and can be used as references
- Not all types are player-accessible (some are internal game objects)
- Physical properties are essential for game mechanics
- Type hierarchy (Category > Group > Type) follows EVE conventions

## Performance Considerations

- Game types are relatively static (changes are infrequent)
- Full type list caching is recommended for applications
- Use category/group filtering to reduce memory footprint
- Type lookups by ID are fast (single API call)

## Error Handling

Common errors:
- **Network errors**: API unavailable or timeout
- **Not found**: Invalid type ID (404)
- **Deserialization errors**: Unexpected response format

Example:
```csharp
var result = await client.GetGameTypeById(99999);
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

## Related APIs

- Use [Smart Assemblies API](./SmartAssemblies.md) to see types in player inventories
- Use [Fuels API](./Fuels.md) for fuel-specific type information
- Type information is referenced throughout other APIs for item identification
