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
public class CalculateFuelRequiredCommandTests {
    private readonly IAnsiConsole _ansiConsole = Substitute.For<IAnsiConsole>();
    private readonly IFrontierDevToolsClient _devToolsClient = Substitute.For<IFrontierDevToolsClient>();

    private readonly ILogger<CalculateFuelRequiredCommand> _logger =
        Substitute.For<ILogger<CalculateFuelRequiredCommand>>();

    private readonly CalculateFuelRequiredCommand _sut;

    public CalculateFuelRequiredCommandTests() {
        _sut = new CalculateFuelRequiredCommand(_logger, _devToolsClient, _ansiConsole);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PrintFuelRequired_AndReturnZero_OnSuccess() {
        // Arrange
        var context = CommandContextHelper.Create();
        var settings = new CalculateFuelRequiredCommand.Settings {
            Mass = 500_000,
            Lightyears = 10,
            FuelEfficiency = 80
        };

        var result = Result.Ok(new FuelRequiredResponse {
            FuelRequired = 23.75m
        });

        _devToolsClient
            .CalculateFuelRequired(settings.Mass, settings.Lightyears, settings.FuelEfficiency,
                Arg.Any<CancellationToken>())
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
        var settings = new CalculateFuelRequiredCommand.Settings {
            Mass = 500_000,
            Lightyears = 10,
            FuelEfficiency = 80
        };

        var error = new Error("Something went wrong");
        var result = Result.Fail<FuelRequiredResponse>(error);

        _devToolsClient
            .CalculateFuelRequired(settings.Mass, settings.Lightyears, settings.FuelEfficiency,
                Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var exitCode = await _sut.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 10, 80, "The mass of your ship must be greater than 0.")]
    [InlineData(100, 0, 80, "Lightyears must be greater than 0.")]
    [InlineData(100, 10, 0, "There is no fuel efficiency of 0 or less in the game.")]
    [InlineData(100, 10, 100, "There is no fuel efficiency greater than 90 in the game.")]
    public void Validate_Should_ReturnError_WhenSettingsInvalid(decimal mass, decimal lightyears, decimal efficiency,
        string expectedMessage) {
        var settings = new CalculateFuelRequiredCommand.Settings {
            Mass = mass,
            Lightyears = lightyears,
            FuelEfficiency = efficiency
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Validate_Should_ReturnSuccess_WhenSettingsValid() {
        var settings = new CalculateFuelRequiredCommand.Settings {
            Mass = 100_000,
            Lightyears = 5,
            FuelEfficiency = 75
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }
}