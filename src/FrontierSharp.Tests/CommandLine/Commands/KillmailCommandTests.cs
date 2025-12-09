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

public class KillmailCommandTests {
    private static KillmailCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null,
        ILogger<KillmailCommand>? logger = null) {
        return new KillmailCommand(
            logger ?? Substitute.For<ILogger<KillmailCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var client = Substitute.For<IWorldApiClient>();
        var payload = new WorldApiPayload<Killmail> {
            Data = new[] { new Killmail { Victim = new SmartCharacter { Name = "Joe" }, Time = DateTimeOffset.UtcNow } },
            Metadata = new WorldApiMetadata { Total = 1, Limit = 100, Offset = 0 }
        };
        var pageResult = Task.FromResult(Result.Ok(payload));
        client.GetKillmailPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(pageResult);

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new KillmailCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_Id_ShowsDetails() {
        var km = new Killmail { Victim = new SmartCharacter { Name = "Bob" }, Time = DateTimeOffset.UtcNow };
        var client = Substitute.For<IWorldApiClient>();
        var killmailPayload = (IEnumerable<Killmail>)new[] { km };
        var killmailResult = Task.FromResult(Result.Ok(killmailPayload));
        client.GetAllKillmails(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(killmailResult);

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new KillmailCommand.Settings { VictimName = "Bob" };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }
}