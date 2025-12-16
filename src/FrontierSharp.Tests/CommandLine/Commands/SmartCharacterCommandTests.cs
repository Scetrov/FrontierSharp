using AwesomeAssertions;
using FluentResults;
using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

public class SmartCharacterCommandTests {
    private static SmartCharacterCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null,
        ILogger<SmartCharacterCommand>? logger = null) {
        return new SmartCharacterCommand(
            logger ?? Substitute.For<ILogger<SmartCharacterCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var client = Substitute.For<IWorldApiClient>();
        client.GetSmartCharacterPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new WorldApiPayload<SmartCharacter> {
                Data = [new SmartCharacter { Address = "0x1", Name = "Alice" }],
                Metadata = new WorldApiMetadata { Total = 1, Limit = 100, Offset = 0 }
            }));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new SmartCharacterCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        rc.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_Address_ShowsDetails() {
        var detail = new SmartCharacterDetail { Address = "0x2", Name = "Bob" };
        var client = Substitute.For<IWorldApiClient>();
        client.GetSmartCharacterById("0x2", Arg.Any<CancellationToken>()).Returns(Result.Ok(detail));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new SmartCharacterCommand.Settings { Address = "0x2" };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        rc.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
        await client.Received().GetSmartCharacterById("0x2", Arg.Any<CancellationToken>());
    }
}