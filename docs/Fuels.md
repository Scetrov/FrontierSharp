# Fuels API

The Fuels API provides access to fuel type definitions and their efficiency ratings in EVE Frontier.

## Overview

Fuels are special resources used for ship propulsion. Each fuel has:
- A game type definition (name, volume, mass, etc.)
- An efficiency rating that affects fuel consumption

## Available Methods

### GetAllFuels

Fetches all available fuel types.

**Signature:**
```csharp
Task<Result<IEnumerable<Fuel>>> GetAllFuels(
    long limit = 100, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `limit`: Number of items per page (default: 100)
- `cancellationToken`: Optional cancellation token

**Returns:** Collection of all fuel types

**Example:**
```csharp
var result = await client.GetAllFuels();
if (result.IsSuccess) {
    foreach (var fuel in result.Value) {
        Console.WriteLine($"{fuel.Type.Name}: Efficiency {fuel.Efficiency}");
    }
}
```

**Response Model:**
```csharp
public class Fuel {
    public GameType Type { get; set; }
    public byte Efficiency { get; set; }
}
```

---

### GetFuelPage

Fetches a single page of fuels for manual pagination.

**Signature:**
```csharp
Task<Result<WorldApiPayload<Fuel>>> GetFuelPage(
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
var result = await client.GetFuelPage(limit: 50, offset: 0);
if (result.IsSuccess) {
    Console.WriteLine($"Total fuels: {result.Value.Metadata.Total}");
    foreach (var fuel in result.Value.Data) {
        Console.WriteLine($"- {fuel.Type.Name}");
    }
}
```

## API Endpoints

- **List:** `/v2/fuels` (paginated)

Note: There is currently no detail endpoint for individual fuels.

## Caching

Fuel queries are cached:
- **List queries**: Cached by limit and offset parameters

## Efficiency Rating

The `Efficiency` property (0-255) affects fuel consumption:
- **Higher efficiency** = Less fuel consumed per lightyear
- **Lower efficiency** = More fuel consumed per lightyear

Formula:
```
Fuel consumed = Distance (LY) / Efficiency
```

## Common Use Cases

### Compare Fuel Efficiency

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    var sorted = fuels.Value
        .OrderByDescending(f => f.Efficiency)
        .ToList();
    
    Console.WriteLine("Fuel types by efficiency:");
    foreach (var fuel in sorted) {
        Console.WriteLine($"{fuel.Type.Name}: {fuel.Efficiency} efficiency");
    }
}
```

### Calculate Fuel Requirements

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    double distance = 150.0; // lightyears
    
    Console.WriteLine($"Fuel needed for {distance} LY trip:");
    foreach (var fuel in fuels.Value) {
        if (fuel.Efficiency > 0) {
            double units = distance / fuel.Efficiency;
            double volume = units * fuel.Type.Volume;
            double mass = units * fuel.Type.Mass;
            
            Console.WriteLine($"{fuel.Type.Name}:");
            Console.WriteLine($"  {units:F2} units");
            Console.WriteLine($"  {volume:F2} m³");
            Console.WriteLine($"  {mass:F2} kg");
        }
    }
}
```

### Best Fuel for Cargo Space

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    double cargoCapacity = 1000; // m³
    double tripDistance = 100; // LY
    
    var analysis = fuels.Value
        .Where(f => f.Efficiency > 0)
        .Select(f => {
            double unitsNeeded = tripDistance / f.Efficiency;
            double volumeNeeded = unitsNeeded * f.Type.Volume;
            bool fits = volumeNeeded <= cargoCapacity;
            
            return new {
                Fuel = f,
                UnitsNeeded = unitsNeeded,
                VolumeNeeded = volumeNeeded,
                Fits = fits,
                RemainingCargo = fits ? cargoCapacity - volumeNeeded : 0
            };
        })
        .Where(x => x.Fits)
        .OrderByDescending(x => x.RemainingCargo)
        .ToList();
    
    if (analysis.Any()) {
        var best = analysis.First();
        Console.WriteLine($"Best fuel for {tripDistance} LY with {cargoCapacity} m³ cargo:");
        Console.WriteLine($"  {best.Fuel.Type.Name}");
        Console.WriteLine($"  {best.UnitsNeeded:F2} units ({best.VolumeNeeded:F2} m³)");
        Console.WriteLine($"  {best.RemainingCargo:F2} m³ remaining for cargo");
    }
}
```

### Fuel Cost Analysis

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    // Assume you have price data
    var fuelPrices = new Dictionary<string, decimal> {
        { "Hydrogen", 100m },
        { "Deuterium", 500m },
        { "Tritium", 2000m }
    };
    
    double distance = 200; // LY
    
    Console.WriteLine($"Fuel cost for {distance} LY:");
    foreach (var fuel in fuels.Value) {
        if (fuel.Efficiency > 0 && fuelPrices.TryGetValue(fuel.Type.Name, out var price)) {
            double units = distance / fuel.Efficiency;
            decimal cost = (decimal)units * price;
            
            Console.WriteLine($"{fuel.Type.Name}: {units:F2} units = {cost:C}");
        }
    }
}
```

### Range Calculator

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    double availableVolume = 500; // m³
    
    Console.WriteLine($"Maximum range with {availableVolume} m³ of fuel:");
    foreach (var fuel in fuels.Value) {
        if (fuel.Efficiency > 0) {
            double units = availableVolume / fuel.Type.Volume;
            double range = units * fuel.Efficiency;
            
            Console.WriteLine($"{fuel.Type.Name}: {range:F2} LY");
        }
    }
}
```

### Fuel Efficiency Rating

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    var bestEfficiency = fuels.Value.Max(f => f.Efficiency);
    
    Console.WriteLine("Fuel efficiency ratings:");
    foreach (var fuel in fuels.Value.OrderByDescending(f => f.Efficiency)) {
        double rating = (double)fuel.Efficiency / bestEfficiency * 100;
        Console.WriteLine($"{fuel.Type.Name}: {rating:F1}% efficiency");
    }
}
```

### Lightest Fuel Option

```csharp
var fuels = await client.GetAllFuels();
if (fuels.IsSuccess) {
    double distance = 75; // LY
    
    var lightest = fuels.Value
        .Where(f => f.Efficiency > 0)
        .Select(f => new {
            Fuel = f,
            Units = distance / f.Efficiency,
            Mass = (distance / f.Efficiency) * f.Type.Mass
        })
        .OrderBy(x => x.Mass)
        .First();
    
    Console.WriteLine($"Lightest fuel for {distance} LY:");
    Console.WriteLine($"  {lightest.Fuel.Type.Name}");
    Console.WriteLine($"  {lightest.Units:F2} units");
    Console.WriteLine($"  {lightest.Mass:F2} kg");
}
```

## Fuel Properties

Each fuel inherits properties from its GameType:
- **Volume**: Cargo space per unit (m³)
- **Mass**: Weight per unit (kg)
- **Name**: Display name
- **Description**: Fuel information
- **PortionSize**: Stack size

## Notes

- Fuel list is relatively small and stable
- All fuels share the same consumption formula (distance / efficiency)
- Higher efficiency fuels are usually more expensive
- Volume and mass vary between fuel types
- Some ships may have fuel type restrictions (check ship specs)

## Performance Considerations

- Fuel data is lightweight and changes infrequently
- Cache fuel list for repeated calculations
- Pre-calculate fuel requirements for common routes
- Consider both volume and efficiency for cargo optimization

## Error Handling

Common errors:
- **Network errors**: API unavailable or timeout
- **Deserialization errors**: Unexpected response format

Example:
```csharp
var result = await client.GetAllFuels();
if (result.IsFailed) {
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

## Related APIs

- Use [Game Types API](./GameTypes.md) for detailed fuel type information
- Use [FrontierDevTools Distance API](./README.md#frontier-devtools-api) to calculate distances
- Use [Smart Assemblies API](./SmartAssemblies.md) to check ship fuel capacity
- Fuel calculations often work with [Solar Systems API](./SolarSystems.md) for route planning

## FrontierDevTools Integration

The FrontierDevTools API provides a `CalculateFuelPerLightyear` method that uses fuel efficiency data:

```csharp
var fuelClient = services.GetRequiredService<IFrontierDevToolsClient>();
var result = await fuelClient.CalculateFuelPerLightyear(fuelTypeId: 12345);

if (result.IsSuccess) {
    Console.WriteLine($"Fuel per LY: {result.Value.FuelPerLightyear}");
}
```

This method provides an alternative to manual efficiency calculations and may include additional game-specific logic.
