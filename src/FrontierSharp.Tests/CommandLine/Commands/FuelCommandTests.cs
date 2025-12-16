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

public class FuelCommandTests {
    private static FuelCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null, ILogger<FuelCommand>? logger = null) {
        return new FuelCommand(
            logger ?? Substitute.For<ILogger<FuelCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var client = Substitute.For<IWorldApiClient>();
        var payload = new WorldApiPayload<Fuel> {
            Data = [new Fuel { Type = new GameType { Id = 1, Name = "Hydrogen" }, Efficiency = 120 }],
            Metadata = new WorldApiMetadata { Total = 1, Limit = 100, Offset = 0 }
        };
        client.GetFuelsPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Ok(payload)));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new FuelCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        rc.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_TypeId_ShowsDetails() {
        var fuel = new Fuel { Type = new GameType { Id = 2, Name = "Helium" }, Efficiency = 80 };
        var client = Substitute.For<IWorldApiClient>();
        client.GetAllFuels(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Ok<IEnumerable<Fuel>>([fuel])));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new FuelCommand.Settings { Id = 2 };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        rc.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }
}