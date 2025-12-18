#:package FrontierSharp.WorldApi@0.15.20
#:package ZiggyCreatures.FusionCache@2.4.0
#:sdk Microsoft.NET.Sdk

using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

var services = new ServiceCollection();

services.AddHttpClient();
services.AddFusionCache().AsHybridCache();
services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(WorldApiClient))
	.Configure<FrontierSharpHttpClientOptions>(static options => {
		options.BaseUri = "https://blockchain-gateway-stillness.live.tech.evefrontier.com";
		options.HttpClientName = nameof(WorldApiClient);
	});
services.AddSingleton<IWorldApiClient, WorldApiClient>();

var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IWorldApiClient>();
var result = await client.GetAllTypes();

if (result.IsFailed) {
    Console.WriteLine("Failed to get types:");
    foreach (var error in result.Errors) {
        Console.WriteLine($" - {error.Message}");
    }
	return;
}

foreach (var category in result.Value.GroupBy(x => x.CategoryName)) {
    Console.WriteLine($"Category: {category.Key}");
    foreach (var type in category) {
        Console.WriteLine($" - {type.Name} (ID: {type.Id})");
    }
}