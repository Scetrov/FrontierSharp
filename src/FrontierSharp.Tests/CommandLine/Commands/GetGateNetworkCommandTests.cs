using FluentAssertions;
using FluentResults;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

public class GetGateNetworkCommandTests {
    private static GetGateNetworkCommand CreateCommand(
        IFrontierDevToolsClient? client = null,
        IAnsiConsole? console = null,
        ILogger<GetCorporationCommand>? logger = null) =>
        new(logger ?? Substitute.For<ILogger<GetCorporationCommand>>(),
            client ?? Substitute.For<IFrontierDevToolsClient>(),
            console ?? Substitute.For<IAnsiConsole>());

    [Fact]
    public void Validate_Fails_WhenIdentifierIsEmpty() {
        var settings = new GetGateNetworkCommand.Settings {
            Identifier = "  "
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("identifier");
    }

    [Fact]
    public void Validate_Succeeds_WithValidIdentifier() {
        var settings = new GetGateNetworkCommand.Settings {
            Identifier = "Scetrov"
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenResultFails() {
        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetGateNetwork(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(Result.Fail<GateNetworkResponse>("Failed"));

        var command = CreateCommand(client);
        var settings = new GetGateNetworkCommand.Settings {
            Identifier = "98000001"
        };

        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenNoGatesReturned() {
        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetGateNetwork(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(Result.Ok(new GateNetworkResponse {
                  GateNetwork = new List<GateResponse>()
              }));

        var command = CreateCommand(client);
        var settings = new GetGateNetworkCommand.Settings {
            Identifier = "98000001"
        };

        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WithValidGates() {
        var gate = new GateResponse {
            FromSystem = "Alpha",
            ToSystem = "Beta",
            Owner = "Scetrov",
            FuelAmount = 250,
            FromIsOnline = true,
            ToIsOnline = false
        };

        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetGateNetwork(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(Result.Ok(new GateNetworkResponse {
                  GateNetwork = new List<GateResponse> { gate }
              }));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(client, console);

        var settings = new GetGateNetworkCommand.Settings {
            Identifier = "Scetrov"
        };

        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(0);
        console.Received().Write(Arg.Any<Table>());
    }
}
