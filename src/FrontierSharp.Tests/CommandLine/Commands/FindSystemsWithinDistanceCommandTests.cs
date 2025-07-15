using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using FluentResults;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

[SuppressMessage("Non-substitutable member", "NS1001:Non-virtual setup specification.")]
[SuppressMessage("Usage", "NS5000:Received check.")]
public class FindSystemsWithinDistanceCommandTests {
    [Fact]
    public async Task ExecuteAsync_ShouldReturnZeroAndWriteTable_WhenSystemsAreFound() {
        var logger = Substitute.For<ILogger<FindSystemsWithinDistanceCommand>>();
        var client = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();
        var command = new FindSystemsWithinDistanceCommand(logger, client, console);

        var systems = new List<SystemDistanceResponse> {
            new() {
                SystemName = "SYS-01",
                DistanceInLightYears = 12.5m
            },
            new() {
                SystemName = "SYS-02",
                DistanceInLightYears = 8.3m
            }
        };

        client.FindSystemsWithinDistance("ROOT", 50, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SystemsWithinDistanceResponse {
                NearbySystems = systems,
                ReferenceSystem = "ROOT"
            }));

        var context = CommandContextHelper.Create();
        var settings = new FindSystemsWithinDistanceCommand.Settings {
            SystemName = "ROOT",
            MaxDistance = 50
        };

        var result = await command.ExecuteAsync(context, settings);

        result.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOneAndLogError_WhenResultIsFailed() {
        var logger = Substitute.For<ILogger<FindSystemsWithinDistanceCommand>>();
        var client = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();
        var command = new FindSystemsWithinDistanceCommand(logger, client, console);

        var error = new Error("API failure");
        client.FindSystemsWithinDistance("ROOT", 50, Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SystemsWithinDistanceResponse>(error));

        var context = CommandContextHelper.Create();
        var settings = new FindSystemsWithinDistanceCommand.Settings {
            SystemName = "ROOT",
            MaxDistance = 50
        };

        var result = await command.ExecuteAsync(context, settings);

        result.Should().Be(1);
        console.DidNotReceive().Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOneAndLogMessage_WhenNoSystemsFound() {
        var logger = Substitute.For<ILogger<FindSystemsWithinDistanceCommand>>();
        var client = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();
        var command = new FindSystemsWithinDistanceCommand(logger, client, console);

        client.FindSystemsWithinDistance("EMPTY", 100, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new SystemsWithinDistanceResponse {
                NearbySystems = [],
                ReferenceSystem = "EMPTY"
            }));

        var context = CommandContextHelper.Create();
        var settings = new FindSystemsWithinDistanceCommand.Settings {
            SystemName = "EMPTY",
            MaxDistance = 100
        };

        var result = await command.ExecuteAsync(context, settings);

        result.Should().Be(1);
        console.DidNotReceive().Write(Arg.Any<Table>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Settings_ShouldReturnError_WhenSystemNameIsInvalid(string? systemName) {
        var settings = new FindSystemsWithinDistanceCommand.Settings {
            SystemName = systemName!,
            MaxDistance = 20
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Be("You must specify a start solar system.");
    }

    [Fact]
    public void Settings_ShouldReturnSuccess_WhenSystemNameIsValid() {
        var settings = new FindSystemsWithinDistanceCommand.Settings {
            SystemName = "EFN-12M",
            MaxDistance = 80
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }
}