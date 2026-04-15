# FrontierSharp.SuiClient

## Description

`FrontierSharp.SuiClient` provides access to EVE Frontier data exposed through Sui GraphQL, including:

- killmails
- characters
- assemblies
- polling-based assembly update subscriptions

The assembly watcher API is designed for the common flow of loading a snapshot with `GetAllAssembliesAsync()` and then
subscribing to incremental changes.

## Installation

```sh
dotnet add package FrontierSharp.SuiClient
dotnet add package ZiggyCreatures.FusionCache
```

## Dependency Injection Setup

```csharp
var services = new ServiceCollection();

const string HTTP_CLIENT_NAME = "SuiGraphQl";

services.AddLogging();
services.AddHttpClient(HTTP_CLIENT_NAME);
services.AddFusionCache().AsHybridCache();
services.Configure<SuiClientOptions>(options => {
    options.HttpClientName = HTTP_CLIENT_NAME;
    options.WithNetwork(SuiNetwork.Testnet);
    options.WithDefaultWorldPackageAddress("0x28b497559d65ab320d9da4613bf2498d5946b2c0ae3597ccfda3072ce127448c");
});
services.AddSingleton<ISuiGraphQlClient, SuiGraphQlClient>();
services.AddSingleton<IWorldClient, WorldClient>();

var serviceProvider = services.BuildServiceProvider();

var client = serviceProvider.GetRequiredService<IWorldClient>();

var initialAssembliesResult = await client.GetAllAssembliesAsync(first: 50);
if (initialAssembliesResult.IsFailed) {
    foreach (var error in initialAssembliesResult.Errors) {
        Console.WriteLine($"Failed to load assemblies: {error.Message}");
    }
    return;
}

Console.WriteLine($"Loaded {initialAssembliesResult.Value.Count()} Assemblies from initial batch");

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
        PageSize = 50
    });

if (subscriptionResult.IsFailed) {
    foreach (var error in subscriptionResult.Errors) {
        Console.WriteLine($"Failed to start subscription: {error.Message}");
    }
    return;
}

using var subscription = subscriptionResult.Value;

await Task.Delay(-1);

subscription.Dispose();
await subscription.Completion;
```

## Querying Different World Packages

To query a different world package without creating another client, pass `worldPackageAddress` on the call:

```csharp
var result = await client.GetAllAssembliesAsync(
    first: 100,
    worldPackageAddress: "0xanotherworldpackage");
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

- Compiled example
  project: [examples/AssemblyWatcherExample/Program.cs](https://github.com/Scetrov/FrontierSharp/blob/main/examples/AssemblyWatcherExample/Program.cs)
- Script-style
  sample: [examples/assembly-watcher.cs](https://github.com/Scetrov/FrontierSharp/blob/main/examples/assembly-watcher.cs)

