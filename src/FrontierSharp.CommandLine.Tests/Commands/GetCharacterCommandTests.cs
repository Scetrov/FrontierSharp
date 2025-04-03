using System.Numerics;
using FluentAssertions;
using FluentResults;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using Moq;
using Spectre.Console;
using Spectre.Console.Cli;
using Xunit;

namespace FrontierSharp.CommandLine.Tests.Commands;

// Helper to create a CommandContext using a mock IAnsiConsole.
public static class CommandContextHelper {
    public static CommandContext Create() {
        var args = new[] { "dummy" };
        var remaining = new Mock<IRemainingArguments>().Object;
        return new CommandContext(args, remaining, "dummy", null);
    }
}

public class GetCharacterCommandTests {
    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenResultFails() {
        // Arrange
        var loggerMock = new Mock<ILogger<GetCharacterCommand>>();
        var devToolsClientMock = new Mock<IFrontierDevToolsClient>();
        var ansiConsoleMock = new Mock<IAnsiConsole>();

        const string errorMessage = "API error occurred";
        var failureResult = Result.Fail<CharactersResponse>(errorMessage);
        devToolsClientMock
            .Setup(x => x.GetCharactersByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => failureResult);

        var settings = new GetCharacterCommand.Settings { Name = "TestName" };
        var command = new GetCharacterCommand(
            loggerMock.Object,
            devToolsClientMock.Object,
            ansiConsoleMock.Object);

        // Act
        var exitCode = await command.ExecuteAsync(
            CommandContextHelper.Create(), settings);

        // Assert
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenNoCharactersFound() {
        // Arrange
        var loggerMock = new Mock<ILogger<GetCharacterCommand>>();
        var devToolsClientMock = new Mock<IFrontierDevToolsClient>();
        var ansiConsoleMock = new Mock<IAnsiConsole>();

        var response = new CharactersResponse { Characters = new List<CharacterResponse>() };
        var successResult = Result.Ok(response);
        devToolsClientMock
            .Setup(x => x.GetCharactersByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => successResult);

        var settings = new GetCharacterCommand.Settings { Name = "TestName" };
        var command = new GetCharacterCommand(
            loggerMock.Object,
            devToolsClientMock.Object,
            ansiConsoleMock.Object);

        // Act
        var exitCode = await command.ExecuteAsync(
            CommandContextHelper.Create(), settings);

        // Assert
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WithMultipleCharacters() {
        // Arrange
        var loggerMock = new Mock<ILogger<GetCharacterCommand>>();
        var devToolsClientMock = new Mock<IFrontierDevToolsClient>();
        var ansiConsoleMock = new Mock<IAnsiConsole>();

        var character1 = new CharacterResponse { Name = "Alice", Address = "0xAlice", CorpId = 1 };
        var character2 = new CharacterResponse { Name = "Bob", Address = "0xBob", CorpId = 2 };
        var response = new CharactersResponse { Characters = new List<CharacterResponse> { character1, character2 } };
        var successResult = Result.Ok(response);
        devToolsClientMock
            .Setup(x => x.GetCharactersByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => successResult);

        var settings = new GetCharacterCommand.Settings { Name = "TestName" };
        var command = new GetCharacterCommand(
            loggerMock.Object,
            devToolsClientMock.Object,
            ansiConsoleMock.Object);

        ansiConsoleMock
            .Setup(x => x.Write(It.IsAny<Table>()));

        // Act
        var exitCode = await command.ExecuteAsync(
            CommandContextHelper.Create(), settings);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WithSingleCharacter() {
        // Arrange
        var loggerMock = new Mock<ILogger<GetCharacterCommand>>();
        var devToolsClientMock = new Mock<IFrontierDevToolsClient>();
        var ansiConsoleMock = new Mock<IAnsiConsole>();

        // Create one character with full details.
        var character = new CharacterResponse {
            Name = "Charlie",
            Address = "0xCharlieAddress12345678901234567890",
            CorpId = 3,
            Id = "UniqueCharacterIdThatIsLong",
            IsSmartCharacter = true,
            CreatedAt = new DateTimeOffset(2021, 6, 15, 12, 0, 0, TimeSpan.Zero),
            EveBalanceWei = BigInteger.Parse("1000000000000000000"), // 1 Ether
            GasBalanceWei = BigInteger.Parse("500000000000000000") // 0.5 Ether
        };
        var response = new CharactersResponse { Characters = new List<CharacterResponse> { character } };
        var successResult = Result.Ok(response);
        devToolsClientMock
            .Setup(x => x.GetCharactersByName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => successResult);

        var settings = new GetCharacterCommand.Settings { Name = "TestName" };
        var command = new GetCharacterCommand(
            loggerMock.Object,
            devToolsClientMock.Object,
            ansiConsoleMock.Object);

        ansiConsoleMock
            .Setup(x => x.Write(It.IsAny<Table>()));

        // Act
        var exitCode = await command.ExecuteAsync(
            CommandContextHelper.Create(), settings);

        // Assert
        exitCode.Should().Be(0);
    }
}