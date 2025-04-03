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

public class GetCharacterCommandTests {
    private static GetCharacterCommand CreateCommand(
        IFrontierDevToolsClient? client = null,
        IAnsiConsole? console = null,
        ILogger<GetCharacterCommand>? logger = null) =>
        new(logger ?? Substitute.For<ILogger<GetCharacterCommand>>(),
            client ?? Substitute.For<IFrontierDevToolsClient>(),
            console ?? Substitute.For<IAnsiConsole>());

    [Fact]
    public void Settings_Validate_Fails_WhenNoOptionProvided() {
        var settings = new GetCharacterCommand.Settings();
        var result = settings.Validate();
        result.Successful.Should().BeFalse();
    }

    [Fact]
    public void Settings_Validate_Fails_WhenBothNameAndAddressProvided() {
        var settings = new GetCharacterCommand.Settings {
            Name = "Alice",
            Address = "0x1234567890123456789012345678901234567890"
        };
        var result = settings.Validate();
        result.Successful.Should().BeFalse();
    }

    [Fact]
    public void Settings_Validate_Fails_WhenAddressInvalidFormat() {
        var settings = new GetCharacterCommand.Settings {
            Address = "123456"
        };
        var result = settings.Validate();
        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("prefix");
    }

    [Fact]
    public void Settings_Validate_Fails_WhenAddressWrongLength() {
        var settings = new GetCharacterCommand.Settings {
            Address = "0x1234"
        };
        var result = settings.Validate();
        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("42 characters");
    }

    [Fact]
    public void Settings_SearchType_Throws_WhenNoValidOption() {
        var settings = new GetCharacterCommand.Settings();
        Action act = () => _ = settings.SearchType;
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public async Task ExecuteAsync_UsesAddressSearch_WhenSearchTypeIsAddress() {
        var loggerMock = Substitute.For<ILogger<GetCharacterCommand>>();
        var devToolsClientMock = Substitute.For<IFrontierDevToolsClient>();
        var ansiConsoleMock = Substitute.For<IAnsiConsole>();

        var response = new CharactersResponse {
            Characters = new List<CharacterResponse> {
                new() { Name = "AddrGuy", Address = "0x1234567890123456789012345678901234567890", CorpId = 5 }
            }
        };
        devToolsClientMock.GetCharactersByAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(response));

        var settings = new GetCharacterCommand.Settings {
            Address = "0x1234567890123456789012345678901234567890"
        };

        var command = CreateCommand(devToolsClientMock, ansiConsoleMock, loggerMock);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(0);
        await devToolsClientMock.Received().GetCharactersByAddress(settings.Address, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AddressSearch_ReturnsOne_OnFailure() {
        var loggerMock = Substitute.For<ILogger<GetCharacterCommand>>();
        var devToolsClientMock = Substitute.For<IFrontierDevToolsClient>();
        var ansiConsoleMock = Substitute.For<IAnsiConsole>();

        devToolsClientMock.GetCharactersByAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<CharactersResponse>("address error"));

        var settings = new GetCharacterCommand.Settings {
            Address = "0x1234567890123456789012345678901234567890"
        };

        var command = CreateCommand(devToolsClientMock, ansiConsoleMock, loggerMock);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_AddressSearch_ReturnsOne_OnEmptyCharacters() {
        var devToolsClientMock = Substitute.For<IFrontierDevToolsClient>();
        devToolsClientMock.GetCharactersByAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new CharactersResponse {
                Characters = new List<CharacterResponse>()
            }));

        var settings = new GetCharacterCommand.Settings {
            Address = "0x1234567890123456789012345678901234567890"
        };

        var command = CreateCommand(devToolsClientMock);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_AddressSearch_ReturnsZero_OnMultipleCharacters() {
        var devToolsClientMock = Substitute.For<IFrontierDevToolsClient>();
        devToolsClientMock.GetCharactersByAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new CharactersResponse {
                Characters = new List<CharacterResponse> {
                    new() { Name = "A", Address = "0x1", CorpId = 1 },
                    new() { Name = "B", Address = "0x2", CorpId = 2 }
                }
            }));

        var settings = new GetCharacterCommand.Settings {
            Address = "0x1234567890123456789012345678901234567890"
        };

        var consoleMock = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(devToolsClientMock, consoleMock);
        var exitCode = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        exitCode.Should().Be(0);
        consoleMock.Received().Write(Arg.Any<Table>());
    }
}
