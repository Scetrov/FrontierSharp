# FrontierSharp WorldApi Documentation

This directory contains comprehensive documentation for the FrontierSharp WorldApi client endpoints.

## Overview

The WorldApi client provides access to EVE Frontier's World API, allowing you to query information about:

- **Tribes** - Player corporations/guilds and their members
- **Smart Characters** - Player characters on the blockchain
- **Smart Assemblies** - Ships and structures in the game world
- **Solar Systems** - Star systems and their locations
- **Killmails** - Combat events and player kills
- **Game Types** - Item and entity type definitions
- **Fuels** - Fuel types and specifications
- **Configuration** - Game configuration and contract addresses

## Quick Start

```csharp
using FrontierSharp.WorldApi;
using Microsoft.Extensions.DependencyInjection;

// Setup DI
services.AddHttpClient("WorldApi");
services.AddFusionCache().AsHybridCache();
services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(WorldApiClient))
    .Configure<FrontierSharpHttpClientOptions>(options => {
        options.BaseUri = "https://blockchain-gateway-stillness.live.tech.evefrontier.com";
        options.HttpClientName = "WorldApi";
    });
services.AddSingleton<IWorldApiClient, WorldApiClient>();

// Use the client
var client = serviceProvider.GetRequiredService<IWorldApiClient>();
var tribes = await client.GetAllTribes();
```

## Common Patterns

All paginated endpoints follow a consistent pattern:

### GetAll Methods
Automatically fetches all pages and returns a complete collection:
```csharp
var allTribes = await client.GetAllTribes();
```

### GetPage Methods
Fetches a single page for manual pagination control:
```csharp
var page = await client.GetTribesPage(limit: 100, offset: 0);
```

### GetById Methods
Fetches detailed information for a specific entity:
```csharp
var tribeDetail = await client.GetTribeById(98000314);
```

## Response Types

All methods return `Result<T>` from FluentResults:
```csharp
var result = await client.GetTribeById(98000314);
if (result.IsSuccess) {
    var tribe = result.Value;
    Console.WriteLine($"Tribe: {tribe.Name}");
} else {
    Console.WriteLine($"Error: {result.Errors.First().Message}");
}
```

## Documentation Index

- [Tribes API](./Tribes.md) - Corporation/guild management and member listings
- [Smart Characters API](./SmartCharacters.md) - Player character information
- [Smart Assemblies API](./SmartAssemblies.md) - Ships and structures
- [Solar Systems API](./SolarSystems.md) - Star system information and locations
- [Killmails API](./Killmails.md) - Combat events and player deaths
- [Game Types API](./GameTypes.md) - Item and entity type definitions
- [Fuels API](./Fuels.md) - Fuel types and specifications
- [Configuration API](./Configuration.md) - Game configuration and contracts

## Caching

All GET requests are automatically cached using HybridCache (FusionCache). Cache keys are generated based on:
- Endpoint name
- Request parameters
- Entity IDs

Cache behavior can be configured through `FrontierSharpHttpClientOptions`.

## Error Handling

The client uses FluentResults for error handling. Common error scenarios:

- **Network errors**: Connection timeouts, DNS failures
- **API errors**: Invalid parameters, entity not found
- **Deserialization errors**: Unexpected API response format

Example error handling:
```csharp
var result = await client.GetTribeById(invalid_id);
if (result.IsFailed) {
    foreach (var error in result.Errors) {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

## Rate Limiting

The World API may implement rate limiting. Consider:
- Using the page-based methods for large datasets
- Implementing exponential backoff for retries
- Caching results where appropriate

## Contributing

When adding new endpoints, please:
1. Add the interface method to `IWorldApiClient`
2. Implement in `WorldApiClient`
3. Create request models in `RequestModel/`
4. Create response models in `Models/`
5. Add comprehensive tests
6. Document in this directory
