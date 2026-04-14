# FrontierSharp.SuiClient

## Description

`FrontierSharp.SuiClient` provides access to EVE Frontier data exposed through Sui GraphQL, including:

- killmails
- characters
- assemblies
- polling-based assembly update subscriptions

The assembly watcher API is designed for the common flow of loading a snapshot with `GetAllAssembliesAsync()` and then subscribing to incremental changes.

## Installation

```sh
dotnet add package FrontierSharp.SuiClient
dotnet add package ZiggyCreatures.FusionCache
```

## Dependency Injection Setup

```csharp
using FrontierSharp.SuiClient;
using FrontierSharp.SuiClient.GraphQl;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

var services = new ServiceCollection();

services.AddLogging();
services.AddHttpClient("SuiGraphQl");
services.AddFusionCache().AsHybridCache();
services.Configure<SuiClientOptions>(options => {
    options.HttpClientName = "SuiGraphQl";

    // Optional overrides:
    // options.GraphQlEndpoint = "https://graphql.testnet.sui.io/graphql";
    // options.WorldPackageAddress = "0x...";
    // options.GraphQlCacheDuration = TimeSpan.FromSeconds(30);
});
services.AddSingleton<ISuiGraphQlClient, SuiGraphQlClient>();
services.AddSingleton<IWorldClient, WorldClient>();
```

## Watching for Assembly Updates

If you already have a snapshot, pass it into `SubscribeToAssemblyUpdatesAsync(...)` and react only to future changes:

```csharp
var client = serviceProvider.GetRequiredService<IWorldClient>();

var initialAssembliesResult = await client.GetAllAssembliesAsync(first: 100);
if (initialAssembliesResult.IsFailed) {
    foreach (var error in initialAssembliesResult.Errors) {
        Console.WriteLine($"Failed to load assemblies: {error.Message}");
    }
    return;
}

var subscriptionResult = await client.SubscribeToAssemblyUpdatesAsync(
    initialAssembliesResult.Value,
    async (batch, cancellationToken) => {
        foreach (var change in batch.Changes) {
            var assembly = change.Current ?? change.Previous;
            Console.WriteLine($"{change.ChangeType}: {assembly!.Key.Tenant}/{assembly.Key.ItemId}");
        }

        await Task.CompletedTask;
    },
    new AssemblySubscriptionOptions {
        PollInterval = TimeSpan.FromSeconds(5),
        PageSize = 100
    });

if (subscriptionResult.IsFailed) {
    foreach (var error in subscriptionResult.Errors) {
        Console.WriteLine($"Failed to start subscription: {error.Message}");
    }
    return;
}

using var subscription = subscriptionResult.Value;

// ... keep the process alive here ...

subscription.Dispose();
await subscription.Completion;
```

If you want the watcher to load its own baseline and emit it to the callback, use the overload without `currentAssemblies`:

```csharp
var subscriptionResult = await client.SubscribeToAssemblyUpdatesAsync(
    async (batch, cancellationToken) => {
        if (batch.IsInitialSnapshot) {
            Console.WriteLine($"Initial snapshot contains {batch.CurrentAssemblies.Count} assemblies.");
            return;
        }

        Console.WriteLine($"Received {batch.Changes.Count} changes.");
        await Task.CompletedTask;
    },
    new AssemblySubscriptionOptions {
        EmitInitialSnapshot = true,
        PollInterval = TimeSpan.FromSeconds(5)
    });
```

## Update Semantics

Each callback receives an `AssemblyUpdateBatch`:

- `IsInitialSnapshot` is `true` only when `EmitInitialSnapshot` is enabled and the watcher is sending the baseline load.
- `CurrentAssemblies` contains the latest full snapshot known to the watcher.
- `Changes` contains only the delta for that poll cycle.

Each `AssemblyChange` contains:

- `ChangeType` — `Added`, `Updated`, or `Removed`
- `Previous` — the prior assembly state for updates and removals
- `Current` — the new assembly state for additions and updates
- `Key` — a shortcut to the assembly's `TenantItemId`

## Notes

- The watcher polls on a timer; it is not a websocket subscription.
- Polling queries bypass the GraphQL cache so updates are not delayed by `GraphQlCacheDuration`.
- Dispose the returned `IAssemblyUpdateSubscription` to stop polling, then await `Completion` for shutdown.

## Example

- Compiled example project: [`../../examples/AssemblyWatcherExample/Program.cs`](../../examples/AssemblyWatcherExample/Program.cs)
- Script-style sample: [`../../examples/assembly-watcher.cs`](../../examples/assembly-watcher.cs)

