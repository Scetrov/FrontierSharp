using System.Numerics;
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

public class SmartAssemblyCommandTests {
    private static SmartAssemblyCommand CreateCommand(IWorldApiClient? client = null, IAnsiConsole? console = null,
        ILogger<SmartAssemblyCommand>? logger = null) {
        return new SmartAssemblyCommand(
            logger ?? Substitute.For<ILogger<SmartAssemblyCommand>>(),
            client ?? Substitute.For<IWorldApiClient>(),
            console ?? Substitute.For<IAnsiConsole>(),
            Options.Create(new ConfigurationOptions())
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShowAll_WritesTable() {
        var client = Substitute.For<IWorldApiClient>();
        client.GetSmartAssemblyPage(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new WorldApiPayload<SmartAssemblyWithSolarSystem> {
                Data = [
                    new SmartAssemblyWithSolarSystem { Id = "1", Name = "Widget", SolarSystem = new SolarSystem { Id = 1, Name = "Sol" } }
                ],
                Metadata = new WorldApiMetadata { Total = 1, Limit = 100, Offset = 0 }
            }));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new SmartAssemblyCommand.Settings { ShowAll = true };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        rc.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_Id_ShowsDetails() {
        var detail = new SmartAssemblyDetail { Id = "2", Name = "Gadget", SolarSystem = new SolarSystem { Id = 2, Name = "Alpha" } };
        var client = Substitute.For<IWorldApiClient>();
        client.GetSmartAssemblyById(BigInteger.Parse("2"), Arg.Any<CancellationToken>()).Returns(Result.Ok(detail));

        var console = Substitute.For<IAnsiConsole>();
        var cmd = CreateCommand(client, console);
        var settings = new SmartAssemblyCommand.Settings { Id = BigInteger.Parse("2") };

        var rc = await cmd.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);
        rc.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
        await client.Received().GetSmartAssemblyById(BigInteger.Parse("2"), Arg.Any<CancellationToken>());
    }
}