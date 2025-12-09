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

public class TypeCommandTests {
    private static TypeCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null, ILogger<TypeCommand>? logger = null) {
        return new TypeCommand(
            logger ?? Substitute.For<ILogger<TypeCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var client = Substitute.For<IWorldApiClient>();
        var payload = new WorldApiPayload<GameType> {
            Data = new[] { new GameType { Id = 1, Name = "Widget", GroupName = "Gadgets" } },
            Metadata = new WorldApiMetadata { Total = 1, Limit = 100, Offset = 0 }
        };
        client.GetTypesPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Ok(payload)));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new TypeCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_Id_ShowsDetails() {
        var detail = new GameType { Id = 2, Name = "Gadget", GroupName = "Devices" };
        var client = Substitute.For<IWorldApiClient>();
        client.GetTypeById(2, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Ok(detail)));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new TypeCommand.Settings { Id = 2 };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }
}