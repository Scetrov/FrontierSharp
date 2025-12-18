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
var result = await client.GetTribesPage();

if (result.IsFailed) {
	Console.WriteLine("Failed with the following reasons:");
	foreach (var reason in result.Reasons) {
		Console.WriteLine($" - {reason.Message}");
	}
	return;
}

foreach (var tribe in result.Value.Data) {
    Console.WriteLine($"{tribe.Id}: {tribe.Name} [{tribe.NameShort}] - {tribe.MemberCount} members");
}
