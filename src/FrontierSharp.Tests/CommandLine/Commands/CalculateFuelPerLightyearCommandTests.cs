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
public class CalculateFuelPerLightyearCommandTests {
    private readonly IAnsiConsole _ansiConsole = Substitute.For<IAnsiConsole>();
    private readonly IFrontierDevToolsClient _devToolsClient = Substitute.For<IFrontierDevToolsClient>();

    private readonly ILogger<CalculateFuelPerLightyearCommand> _logger =
        Substitute.For<ILogger<CalculateFuelPerLightyearCommand>>();

    private readonly CalculateFuelPerLightyearCommand _sut;

    public CalculateFuelPerLightyearCommandTests() {
        _sut = new CalculateFuelPerLightyearCommand(_logger, _devToolsClient, _ansiConsole);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PrintFuelPerLightyear_AndReturnZero_OnSuccess() {
        // Arrange
        var context = CommandContextHelper.Create();
        var settings = new CalculateFuelPerLightyearCommand.Settings {
            Mass = 500_000,
            FuelEfficiency = 80
        };

        var result = Result.Ok(new FuelPerLightyearResponse {
            FuelPerLightyear = 5.5m
        });

        _devToolsClient
            .CalculateFuelPerLightyear(settings.Mass, settings.FuelEfficiency, Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var exitCode = await _sut.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_AndReturnOne_OnFailure() {
        // Arrange
        var context = CommandContextHelper.Create();
        var settings = new CalculateFuelPerLightyearCommand.Settings {
            Mass = 500_000,
            FuelEfficiency = 80
        };

        var error = new Error("API failure");
        var result = Result.Fail<FuelPerLightyearResponse>(error);

        _devToolsClient
            .CalculateFuelPerLightyear(settings.Mass, settings.FuelEfficiency, Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var exitCode = await _sut.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(1);
        _logger.Received().LogError("API failure");
    }

    [Theory]
    [InlineData(0, 80, "The mass of your ship must be greater than 0.")]
    [InlineData(100_000, 0, "There is no fuel efficiency of 0 or less in the game.")]
    [InlineData(100_000, 100, "There is no fuel efficiency greater than 90 in the game.")]
    public void Validate_Should_ReturnError_WhenSettingsInvalid(decimal mass, decimal efficiency,
        string expectedMessage) {
        var settings = new CalculateFuelPerLightyearCommand.Settings {
            Mass = mass,
            FuelEfficiency = efficiency
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Validate_Should_ReturnSuccess_WhenSettingsValid() {
        var settings = new CalculateFuelPerLightyearCommand.Settings {
            Mass = 200_000,
            FuelEfficiency = 75
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }
}