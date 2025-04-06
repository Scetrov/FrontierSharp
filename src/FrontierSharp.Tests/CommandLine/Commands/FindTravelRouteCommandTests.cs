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
public class FindTravelRouteCommandTests {
    private readonly IFrontierDevToolsClient _client = Substitute.For<IFrontierDevToolsClient>();
    private readonly FindTravelRouteCommand _command;
    private readonly IAnsiConsole _console = Substitute.For<IAnsiConsole>();
    private readonly ILogger<FindTravelRouteCommand> _logger = Substitute.For<ILogger<FindTravelRouteCommand>>();

    public FindTravelRouteCommandTests() {
        _command = new FindTravelRouteCommand(_logger, _client, _console);
    }

    [Fact]
    public void Settings_ShouldValidateSuccessfully() {
        var settings = new FindTravelRouteCommand.Settings {
            Start = "A",
            End = "B",
            MaxDistance = 100
        };

        settings.Validate().Successful.Should().BeTrue();
    }

    [Fact]
    public void Settings_ShouldFail_WhenRequiredFieldsMissing() {
        var settings = new FindTravelRouteCommand.Settings {
            Start = "",
            End = ""
        };

        settings.Validate().Successful.Should().BeFalse();
    }

    [Fact]
    public void Settings_ShouldFail_WhenMaxDistanceTooHigh() {
        var settings = new FindTravelRouteCommand.Settings {
            Start = "A",
            End = "B",
            MaxDistance = 501
        };

        settings.Validate().Successful.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturn1_WhenClientFails() {
        var settings = new FindTravelRouteCommand.Settings {
            Start = "Alpha",
            End = "Beta"
        };

        var error = Substitute.For<IError>();
        error.Message.Returns("Route error");

        _client.FindTravelRoute(settings.Start, settings.End, settings.AvoidGates, settings.MaxDistance, Arg.Any<CancellationToken>())
            .Returns(Result.Fail<RouteResponse>(error));

        var result = await _command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(1);
        _logger.Received().LogError("Route error");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturn1_WhenRouteIsEmpty() {
        var settings = new FindTravelRouteCommand.Settings {
            Start = "From",
            End = "To"
        };

        var response = new RouteResponse {
            Route = Array.Empty<JumpResponse>()
        };

        _client.FindTravelRoute(settings.Start, settings.End, settings.AvoidGates, settings.MaxDistance, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(response));

        var result = await _command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(1);
        _logger.Received().LogError("No valid route found for the specified placement.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWriteRouteTable_AndReturn0_OnValidRoute() {
        var settings = new FindTravelRouteCommand.Settings {
            Start = "Foo",
            End = "Bar"
        };

        var response = new RouteResponse {
            Route = [
                new JumpResponse { From = "Foo", To = "Bar", DistanceInLightYears = 42.5m }
            ]
        };

        _client.FindTravelRoute(settings.Start, settings.End, settings.AvoidGates, settings.MaxDistance, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(response));

        var result = await _command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(0);
        _console.Received().Write(Arg.Any<Table>());
    }
}