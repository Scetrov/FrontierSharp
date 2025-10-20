# Solar Systems API

The Solar Systems API provides access to star system information and locations in the EVE Frontier universe.

## Overview

Solar Systems are the fundamental spatial units in EVE Frontier. Each system has:
- Unique ID
- System name
- 3D coordinates (X, Y, Z)
- Associated celestial objects (in detail view)

## Available Methods

### GetAllSolarSystems

Fetches all solar systems in the universe.

**Signature:**
```csharp
Task<Result<IEnumerable<SolarSystem>>> GetAllSolarSystems(
    long limit = 1000, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 1000)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all solar systems with basic information

**Example:**
```csharp
var result = await client.GetAllSolarSystems();
if (result.IsSuccess) {
    foreach (var system in result.Value) {
        Console.WriteLine($"{system.Name} at ({system.Location.X}, {system.Location.Y}, {system.Location.Z})");
    }
}
```

**Response Model:**
```csharp
public class SolarSystem {
    public long Id { get; set; }
    public string Name { get; set; }
    public Location Location { get; set; }
}

public class Location {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}
```

---

### GetSolarSystemPage

Fetches a single page of solar systems for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<SolarSystem>>> GetSolarSystemPage(
    long limit = 1000, 
    long offset = 0, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items to fetch (default: 1000, max: 1000)
- `offset`: Number of items to skip (default: 0)
- `cancellationToken`: Optional cancellation token

**Returns:** Paginated response with metadata

**Example:**
```csharp
var result = await client.GetSolarSystemPage(limit: 100, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total systems: {result.Value.Metadata.Total}");
    foreach (var system in result.Value.Data) {
        Console.WriteLine($"- {system.Name} (ID: {system.Id})");
    }
}
```

---

### GetSolarSystemById

Fetches detailed information about a specific solar system.

**Signature:**
```csharp
Task<Result<SolarSystemDetail>> GetSolarSystemById(
    long id, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `id`: Solar system ID
- `cancellationToken`: Optional cancellation token

**Returns:** Detailed system information including celestial objects

**Example:**
```csharp
var result = await client.GetSolarSystemById(30012580);
if (result.IsSuccess) {
    var system = result.Value;
    Console.WriteLine($"System: {system.Name}");
    Console.WriteLine($"ID: {system.Id}");
    Console.WriteLine($"Location: ({system.Location.X:N2}, {system.Location.Y:N2}, {system.Location.Z:N2})");
    
    // Note: SolarSystemDetail may include additional properties
    // such as celestial objects, gates, etc. depending on the API response
}
```

**Response Model:**
```csharp
public class SolarSystemDetail : SolarSystem {
    // Additional properties may be available depending on API
    // Check the actual API response for available fields
}
```

## API Endpoints

- **List:** `/v2/solarsystems` (paginated)
- **Detail:** `/v2/solarsystems/{id}`

## Caching

All solar system queries are cached:
- **List queries**: Cached by limit and offset parameters
- **Detail queries**: Cached by system ID

Cache key format: `WorldApi_SolarSystem_{SolarSystemId}`

## Common Use Cases

### Finding a System by Name

```csharp
var systems = await client.GetAllSolarSystems();
if (systems.IsSuccess) {
    var system = systems.Value
        .FirstOrDefault(s => s.Name.Equals("Kinakka", StringComparison.OrdinalIgnoreCase));
    
    if (system != null) {
        Console.WriteLine($"Found: {system.Name} (ID: {system.Id})");
        
        // Get detailed information
        var details = await client.GetSolarSystemById(system.Id);
    }
}
```

### Calculating Distance Between Systems

```csharp
var systems = await client.GetAllSolarSystems();
if (systems.IsSuccess) {
    var systemA = systems.Value.First(s => s.Name == "SystemA");
    var systemB = systems.Value.First(s => s.Name == "SystemB");
    
    var distance = Math.Sqrt(
        Math.Pow(systemB.Location.X - systemA.Location.X, 2) +
        Math.Pow(systemB.Location.Y - systemA.Location.Y, 2) +
        Math.Pow(systemB.Location.Z - systemA.Location.Z, 2)
    );
    
    Console.WriteLine($"Distance: {distance:N2} light-years");
}
```

### Finding Nearby Systems

```csharp
var systems = await client.GetAllSolarSystems();
if (systems.IsSuccess) {
    var origin = systems.Value.First(s => s.Name == "Origin");
    var maxDistance = 100.0;
    
    var nearby = systems.Value
        .Select(s => new {
            System = s,
            Distance = Math.Sqrt(
                Math.Pow(s.Location.X - origin.Location.X, 2) +
                Math.Pow(s.Location.Y - origin.Location.Y, 2) +
                Math.Pow(s.Location.Z - origin.Location.Z, 2)
            )
        })
        .Where(x => x.Distance <= maxDistance && x.System.Id != origin.Id)
        .OrderBy(x => x.Distance)
        .ToList();
    
    foreach (var item in nearby) {
        Console.WriteLine($"{item.System.Name}: {item.Distance:N2} LY");
    }
}
```

### Creating a System Map

```csharp
var systems = await client.GetAllSolarSystems();
if (systems.IsSuccess) {
    var map = systems.Value.ToDictionary(s => s.Id, s => s);
    
    // Use the map for fast lookups
    if (map.TryGetValue(30012580, out var system)) {
        Console.WriteLine($"System {system.Name} found at coordinates " +
            $"({system.Location.X}, {system.Location.Y}, {system.Location.Z})");
    }
}
```

### Finding Systems in a Region

```csharp
var systems = await client.GetAllSolarSystems();
if (systems.IsSuccess) {
    // Define a cubic region
    var minX = -1000.0; var maxX = 1000.0;
    var minY = -1000.0; var maxY = 1000.0;
    var minZ = -1000.0; var maxZ = 1000.0;
    
    var systemsInRegion = systems.Value
        .Where(s => s.Location.X >= minX && s.Location.X <= maxX)
        .Where(s => s.Location.Y >= minY && s.Location.Y <= maxY)
        .Where(s => s.Location.Z >= minZ && s.Location.Z <= maxZ)
        .ToList();
    
    Console.WriteLine($"Found {systemsInRegion.Count} systems in region");
}
```

## Coordinate System

Solar systems use a 3D Cartesian coordinate system:
- **X, Y, Z**: Floating-point coordinates in light-years
- **Origin**: (0, 0, 0) represents the center of the universe
- **Scale**: Typical system spacing is 10-500 light-years

Distance formula:
```csharp
var distance = Math.Sqrt(
    Math.Pow(x2 - x1, 2) + 
    Math.Pow(y2 - y1, 2) + 
    Math.Pow(z2 - z1, 2)
);
```

## System IDs

Solar system IDs are numeric:
- Format: Long integer (e.g., 30012580)
- Unique and permanent
- Used to reference systems in other APIs

## Performance Considerations

- Solar systems data is relatively static
- Consider caching the full list locally for distance calculations
- The default page size is 1000 (higher than other endpoints)
- Total number of systems is typically in the tens of thousands

## Notes

- System coordinates are in 3D space
- Location data is relatively static but may update
- System names are unique
- Use system IDs for references, not names
- Distance calculations should account for jump gate networks

## Error Handling

Common errors:
- **System not found**: Invalid system ID
- **Network errors**: API unavailable or timeout
- **Deserialization errors**: Unexpected response format

Example:
```csharp
var result = await client.GetSolarSystemById(999999999);
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

## Related APIs

- Use [Smart Assemblies API](./SmartAssemblies.md) to find ships/structures in a system
- Use [Killmails API](./Killmails.md) to find combat events in a system
- Consider using FrontierDevTools API for route planning between systems
