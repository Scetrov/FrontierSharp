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

public class GetCorporationCommandTests {
    private static GetCorporationCommand CreateCommand(
        IFrontierDevToolsClient? client = null,
        IAnsiConsole? console = null,
        ILogger<GetCorporationCommand>? logger = null) {
        return new GetCorporationCommand(logger ?? Substitute.For<ILogger<GetCorporationCommand>>(),
            client ?? Substitute.For<IFrontierDevToolsClient>(),
            console ?? Substitute.For<IAnsiConsole>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenSearchTypeIsId_AndApiFails() {
        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetCharactersByCorpId(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<CorporationResponse>("API failure"));

        var settings = new GetCorporationCommand.Settings {
            Id = 123
        };

        var command = CreateCommand(client);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
        await client.Received(1).GetCharactersByCorpId(123, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenSearchTypeIsPlayer_AndApiFails() {
        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetCharactersByPlayer(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<CorporationResponse>("API error"));

        var settings = new GetCorporationCommand.Settings {
            PlayerName = "Alice"
        };

        var command = CreateCommand(client);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
        await client.Received(1).GetCharactersByPlayer("Alice", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenApiReturnsNullOrEmptyCharacters() {
        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetCharactersByCorpId(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new CorporationResponse {
                CorpCharacters = new List<string>() // empty
            }));

        var settings = new GetCorporationCommand.Settings {
            Id = 98000001
        };

        var command = CreateCommand(client);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WhenUsingIdAndCharactersReturned() {
        var characters = new List<string> {
            "Alpha",
            "Beta"
        };

        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetCharactersByCorpId(98000001, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new CorporationResponse {
                CorpCharacters = characters
            }));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(client, console);

        var settings = new GetCorporationCommand.Settings {
            Id = 98000001
        };

        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(0);
        console.Received().Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WhenUsingPlayerNameAndCharactersReturned() {
        var characters = new List<string> {
            "Gamma"
        };

        var client = Substitute.For<IFrontierDevToolsClient>();
        client.GetCharactersByPlayer("Scetrov", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new CorporationResponse {
                CorpCharacters = characters
            }));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(client, console);

        var settings = new GetCorporationCommand.Settings {
            PlayerName = "Scetrov"
        };

        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(0);
        console.Received().Write(Arg.Any<Table>());
    }
}