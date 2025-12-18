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
	.Configure<FrontierSharpHttpClientOptions>(options => {
		options.BaseUri = "https://blockchain-gateway-stillness.live.tech.evefrontier.com";
		options.HttpClientName = nameof(WorldApiClient);
	});
services.AddSingleton<IWorldApiClient, WorldApiClient>();

var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IWorldApiClient>();
var result = await client.GetSmartCharacterPage(limit: 25);

if (result.IsFailed) {
	Console.WriteLine("Failed with the following reasons:");
	foreach (var reason in result.Reasons) {
		Console.WriteLine($" - {reason.Message}");
	}
	return;
}

foreach (var character in result.Value.Data) {
	var detail = await client.GetSmartCharacterById(character.Address);	
	
	var onlineAssemblies = detail.Value.SmartAssemblies.Where(x => x.State == FrontierSharp.WorldApi.Models.SmartAssemblyState.Online);

	Console.WriteLine($"Character {detail.Value.Name} (ID: {detail.Value.Id}, Address: {detail.Value.Address}) has {onlineAssemblies.Count()} online assemblies:");
	
	foreach (var assembly in onlineAssemblies) {
		Console.WriteLine($"- Assembly \"{assembly.Name}\" (ID: {assembly.Id})");
	}
}
