using AwesomeAssertions;
using FrontierSharp.SuiClient;
using Xunit;

namespace FrontierSharp.Tests.SuiClient;

public class SuiClientOptionsExtensionsTests {
    [Theory]
    [InlineData(SuiNetwork.Mainnet, "https://graphql.mainnet.sui.io/graphql")]
    [InlineData(SuiNetwork.Testnet, "https://graphql.testnet.sui.io/graphql")]
    [InlineData(SuiNetwork.Devnet, "https://graphql.devnet.sui.io/graphql")]
    public void WithNetwork_SetsExpectedEndpoint(SuiNetwork network, string expectedEndpoint) {
        var options = new SuiClientOptions();

        var result = options.WithNetwork(network);

        result.Should().BeSameAs(options);
        options.GraphQlEndpoint.Should().Be(expectedEndpoint);
    }

    [Fact]
    public void WithDefaultWorldPackageAddress_SetsPackageAddress() {
        var options = new SuiClientOptions();

        var result = options.WithDefaultWorldPackageAddress("0xoverride");

        result.Should().BeSameAs(options);
        options.DefaultWorldPackageAddress.Should().Be("0xoverride");
    }

    [Fact]
    public void WorldPackageAddress_AliasUpdatesDefaultWorldPackageAddress() {
        var options = new SuiClientOptions();

        options.WorldPackageAddress = "0xlegacy";

        options.DefaultWorldPackageAddress.Should().Be("0xlegacy");
        options.WorldPackageAddress.Should().Be("0xlegacy");
    }

    [Fact]
    public void WithNetwork_AndWithDefaultWorldPackageAddress_CanBeChained() {
        var options = new SuiClientOptions()
            .WithNetwork(SuiNetwork.Testnet)
            .WithDefaultWorldPackageAddress("0xoverride");

        options.GraphQlEndpoint.Should().Be("https://graphql.testnet.sui.io/graphql");
        options.DefaultWorldPackageAddress.Should().Be("0xoverride");
    }
}