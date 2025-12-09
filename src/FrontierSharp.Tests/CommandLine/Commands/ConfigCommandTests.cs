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

public class ConfigCommandTests {
    private static ConfigCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null, ILogger<ConfigCommand>? logger = null) {
        return new ConfigCommand(
            logger ?? Substitute.For<ILogger<ConfigCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_Default_ShowsDetail() {
        var cfg = new WorldApiConfig { ChainId = 1, Name = "TestNet", BlockExplorerUrl = "http://explorer", IndexerUrl = "http://index" };
        var client = Substitute.For<IWorldApiClient>();
        client.GetConfig(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Ok<IEnumerable<WorldApiConfig>>(new[] { cfg })));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new ConfigCommand.Settings { ShowAll = false };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var cfg = new WorldApiConfig { ChainId = 2, Name = "MainNet", BlockExplorerUrl = "http://explore", IndexerUrl = "http://index" };
        var client = Substitute.For<IWorldApiClient>();
        client.GetConfig(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Ok<IEnumerable<WorldApiConfig>>(new[] { cfg })));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new ConfigCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }
}