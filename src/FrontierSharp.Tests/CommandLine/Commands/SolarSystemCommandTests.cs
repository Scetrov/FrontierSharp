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

public class SolarSystemCommandTests {
    private static SolarSystemCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null,
        ILogger<SolarSystemCommand>? logger = null) {
        return new SolarSystemCommand(
            logger ?? Substitute.For<ILogger<SolarSystemCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var client = Substitute.For<IWorldApiClient>();
        client.GetSolarSystemPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new WorldApiPayload<SolarSystem> {
                Data = new[] { new SolarSystem { Id = 1, Name = "Sol" } },
                Metadata = new WorldApiMetadata { Total = 1, Limit = 1000, Offset = 0 }
            }));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new SolarSystemCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_Id_ShowsDetails() {
        var detail = new SolarSystemDetail { Id = 2, Name = "Alpha" };
        var client = Substitute.For<IWorldApiClient>();
        client.GetSolarSystemById(2, Arg.Any<CancellationToken>()).Returns(Result.Ok(detail));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new SolarSystemCommand.Settings { Id = 2 };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        Assert.Equal(0, rc);
        console.Received(1).Write(Arg.Any<Table>());
        await client.Received().GetSolarSystemById(2, Arg.Any<CancellationToken>());
    }
}