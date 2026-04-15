#:package FrontierSharp.SuiClient@0.15.20
#:package ZiggyCreatures.FusionCache@2.4.0
#:sdk Microsoft.NET.Sdk

using FrontierSharp.SuiClient;
using FrontierSharp.SuiClient.GraphQl;
using FrontierSharp.SuiClient.Models;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

var services = new ServiceCollection();

services.AddLogging();
services.AddHttpClient("SuiGraphQl", httpClient => {
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});
services.AddFusionCache().AsHybridCache();
services.Configure<SuiClientOptions>(options => {
    options.HttpClientName = "SuiGraphQl";

    // Optional overrides:
    // options.GraphQlEndpoint = "https://graphql.testnet.sui.io/graphql";
    // options.WorldPackageAddress = "0x...";
});
services.AddSingleton<ISuiGraphQlClient, SuiGraphQlClient>();
services.AddSingleton<IWorldClient, WorldClient>();

var serviceProvider = services.BuildServiceProvider();
var worldClient = serviceProvider.GetRequiredService<IWorldClient>();

var initialAssembliesResult = await worldClient.GetAllAssembliesAsync(first: 100);
if (initialAssembliesResult.IsFailed) {
    Console.WriteLine("Failed to load the initial assembly snapshot:");
    foreach (var error in initialAssembliesResult.Errors) {
        Console.WriteLine($" - {error.Message}");
    }
    return;
}

Console.WriteLine($"Loaded {initialAssembliesResult.Value.Count()} assemblies.");
Console.WriteLine("Watching for assembly changes. Press Ctrl+C to stop.");

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

var subscriptionResult = await worldClient.SubscribeToAssemblyUpdatesAsync(
    initialAssembliesResult.Value,
    async (batch, cancellationToken) => {
        foreach (var change in batch.Changes) {
            var assembly = change.Current ?? change.Previous;
            if (assembly == null)
                continue;

            var status = change.Current?.Status.ToString() ?? "Removed";
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] {change.ChangeType}: {assembly.Key.Tenant}/{assembly.Key.ItemId} ({status})");
        }

        await Task.CompletedTask;
    },
    new AssemblySubscriptionOptions {
        PollInterval = TimeSpan.FromSeconds(5),
        PageSize = 100
    },
    shutdown.Token);

if (subscriptionResult.IsFailed) {
    Console.WriteLine("Failed to start assembly watcher:");
    foreach (var error in subscriptionResult.Errors) {
        Console.WriteLine($" - {error.Message}");
    }
    return;
}

using var subscription = subscriptionResult.Value;

try {
    await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
} catch (OperationCanceledException) {
    // Expected when Ctrl+C is pressed.
}

subscription.Dispose();
await subscription.Completion;
Console.WriteLine("Assembly watcher stopped.");

