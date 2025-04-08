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
public class CalculateTravelDistanceCommandTests {
    private readonly ILogger<CalculateTravelDistanceCommand> _logger = Substitute.For<ILogger<CalculateTravelDistanceCommand>>();
    private readonly IFrontierDevToolsClient _devToolsClient = Substitute.For<IFrontierDevToolsClient>();
    private readonly IAnsiConsole _ansiConsole = Substitute.For<IAnsiConsole>();
    private readonly CalculateTravelDistanceCommand _sut;

    public CalculateTravelDistanceCommandTests() {
        _sut = new CalculateTravelDistanceCommand(_logger, _devToolsClient, _ansiConsole);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PrintDistance_AndReturnZero_OnSuccess() {
        // Arrange
        var context = CommandContextHelper.Create();
        var settings = new CalculateTravelDistanceCommand.Settings {
            CurrentFuel = 100,
            Mass = 500_000,
            FuelEfficiency = 80
        };

        var result = Result.Ok(new TravelDistanceResponse() {
            MaxTravelDistanceInLightYears = 42.42m
        });

        _devToolsClient
            .CalculateTravelDistance(settings.CurrentFuel, settings.Mass, settings.FuelEfficiency, Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var exitCode = await _sut.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogErrors_AndReturnOne_OnFailure() {
        // Arrange
        var context = CommandContextHelper.Create();
        var settings = new CalculateTravelDistanceCommand.Settings {
            CurrentFuel = 100,
            Mass = 500_000,
            FuelEfficiency = 80
        };

        var error = new FluentResults.Error("Failed to calculate travel distance");
        var result = Result.Fail<TravelDistanceResponse>(error);

        _devToolsClient
            .CalculateTravelDistance(settings.CurrentFuel, settings.Mass, settings.FuelEfficiency, Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var exitCode = await _sut.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(1);

        _logger.Received().LogError("Failed to calculate travel distance");
    }

    [Theory]
    [InlineData(0, 50_000, 80, "The amount of fuel in your ship must be greater than 0.")]
    [InlineData(100, 50_000, 0, "There is no fuel efficiency of 0 or less in the game.")]
    [InlineData(100, 50_000, 95, "There is no fuel efficiency greater than 90 in the game.")]
    [InlineData(100, 0, 80, "The mass of your ship must be greater than 0.")]
    public void Validate_Should_ReturnError_WhenSettingsInvalid(decimal fuel, decimal mass, decimal efficiency, string expectedMessage) {
        var settings = new CalculateTravelDistanceCommand.Settings {
            CurrentFuel = fuel,
            Mass = mass,
            FuelEfficiency = efficiency
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Validate_Should_ReturnSuccess_WhenSettingsValid() {
        var settings = new CalculateTravelDistanceCommand.Settings {
            CurrentFuel = 100,
            Mass = 500_000,
            FuelEfficiency = 80
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }
}
