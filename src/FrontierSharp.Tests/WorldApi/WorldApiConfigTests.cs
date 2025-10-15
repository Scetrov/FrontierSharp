using AwesomeAssertions;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.WorldApi;

public class WorldApiConfigTests {
    private readonly HybridCache _cache = new FakeHybridCache();
    private readonly MockLogger<FrontierSharpHttpClient> _logger = Substitute.For<MockLogger<FrontierSharpHttpClient>>();
    private readonly IOptions<FrontierSharpHttpClientOptions> _options = Substitute.For<IOptions<FrontierSharpHttpClientOptions>>();

    public WorldApiConfigTests() {
        _options.Value.Returns(new FrontierSharpHttpClientOptions { BaseUri = "https://test.local" });
    }

    [Fact]
    public async Task GetConfig_ShouldParse_WithRealData() {
        // Arrange
        var payload = ResourceHelper.GetEmbeddedResource("FrontierSharp.Tests.WorldApi.payloads.v2.config.json");
        using var reader = new StreamReader(payload);
        var json = await reader.ReadToEndAsync();
        var factory = SubstitutableHttpClientFactory.CreateWithPayload(json);
        var httpClient = new FrontierSharpHttpClient(_logger, factory, _cache, _options);
        var client = new WorldApiClient(httpClient);

        // Act
        var result = await client.GetConfig();

        // Assert
        result.Errors.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        var config = result.Value.ToArray();
        config.Should().HaveCount(1);
        var c = config[0];
        c.ChainId.Should().Be(695569);
        c.Name.Should().Be("EVE pyrope Game");
        c.NativeCurrency.Decimals.Should().Be(18);
        c.NativeCurrency.Name.Should().Be("Gas");
        c.NativeCurrency.Symbol.Should().Be("GAS");
        c.Contracts.World.Address.Should().Be("0x7b71fe480ac4e7e96d150a1454411c5cbfb2b1f1");
        c.Contracts.EveToken.Address.Should().Be("0x14822e59e6498a067B274F813f94098c23C59940");
        c.RpcUrls.Default.Http.Should().Be("https://pyrope-external-sync-node-rpc.live.tech.evefrontier.com");
        c.RpcUrls.Public.Http.Should().Be("https://rpc.pyropechain.com");
        c.BlockExplorerUrl.Should().Be("https://explorer.pyropechain.com");
        c.ItemTypeIDs.Fuel.Should().Be(83839);
        c.ExchangeWalletAddress.Should().Be("0xE294Ee4F54235Fd80e6DD0779ebc0f3A4339D267");
        c.EVEToLuxExchangeRate.Should().Be(10000);
        c.PodPublicSigningKey.Should().Be("3qZtgFE30kxJToevQi5CzKrjtvdyzRViPM+W8+DqhSM");
        c.CycleStartDate.Should().Be(new DateTimeOffset(2025, 08, 20, 14, 00, 00, TimeSpan.Zero));
        c.Systems.Should().ContainKey("createCharacter");
        c.Systems["createCharacter"].Should().StartWith("0x");
    }
}

