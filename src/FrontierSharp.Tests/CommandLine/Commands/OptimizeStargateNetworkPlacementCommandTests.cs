using System.Diagnostics.CodeAnalysis;
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

[SuppressMessage("Non-substitutable member", "NS1001:Non-virtual setup specification.")]
[SuppressMessage("Usage", "NS5000:Received check.")]
public class OptimizeStargateNetworkPlacementCommandTests {
    private readonly IFrontierDevToolsClient _client = Substitute.For<IFrontierDevToolsClient>();
    private readonly OptimizeStargateNetworkPlacementCommand _command;
    private readonly IAnsiConsole _console = Substitute.For<IAnsiConsole>();

    private readonly ILogger<OptimizeStargateNetworkPlacementCommand> _logger =
        Substitute.For<ILogger<OptimizeStargateNetworkPlacementCommand>>();

    public OptimizeStargateNetworkPlacementCommandTests() {
        _command = new OptimizeStargateNetworkPlacementCommand(_logger, _client, _console);
    }

    [Fact]
    public void Settings_ShouldValidateSuccessfully_WithValidInput() {
        var settings = new OptimizeStargateNetworkPlacementCommand.Settings {
            Start = "A",
            End = "B",
            MaxDistance = 499m
        };

        settings.Validate().Successful.Should().BeTrue();
    }

    [Fact]
    public void Settings_ShouldFailValidation_WhenMissingRequiredFields() {
        var settings = new OptimizeStargateNetworkPlacementCommand.Settings {
            Start = "",
            End = ""
        };

        settings.Validate().Successful.Should().BeFalse();
    }

    [Fact]
    public void Settings_ShouldFailValidation_WhenMaxDistanceTooHigh() {
        var settings = new OptimizeStargateNetworkPlacementCommand.Settings {
            Start = "A",
            End = "B",
            MaxDistance = 501
        };

        settings.Validate().Successful.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturn1_WhenApiFails() {
        var settings = new OptimizeStargateNetworkPlacementCommand.Settings {
            Start = "Alpha",
            End = "Beta"
        };

        var error = Substitute.For<IError>();
        error.Message.Returns("Something failed");

        _client.OptimizeStargateAndNetworkPlacement(settings.Start, settings.End, settings.MaxDistance,
                settings.NpcAvoidanceLevel, Arg.Any<CancellationToken>())
            .Returns(Result.Fail<RouteResponse>(error));

        var result = await _command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(1);
        _logger.Received().LogError("Something failed");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturn1_WhenRouteIsEmpty() {
        var settings = new OptimizeStargateNetworkPlacementCommand.Settings {
            Start = "X",
            End = "Y"
        };

        var response = new RouteResponse {
            Route = Array.Empty<JumpResponse>()
        };

        _client.OptimizeStargateAndNetworkPlacement(settings.Start, settings.End, settings.MaxDistance,
                settings.NpcAvoidanceLevel, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(response));

        var result = await _command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(1);
        _logger.Received().LogError("No valid route found for the specified placement.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWriteTable_AndReturn0_WhenRouteIsValid() {
        var settings = new OptimizeStargateNetworkPlacementCommand.Settings {
            Start = "A",
            End = "B"
        };

        var response = new RouteResponse {
            Route = [
                new JumpResponse {
                    From = "A",
                    To = "B",
                    DistanceInLightYears = 300.3m
                }
            ]
        };

        _client.OptimizeStargateAndNetworkPlacement(settings.Start, settings.End, settings.MaxDistance,
                settings.NpcAvoidanceLevel, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(response));

        var result = await _command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(0);
        _console.Received().Write(Arg.Any<Table>());
    }
}