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
public class CalculateDistanceCommandTests {
    [Fact]
    public async Task ExecuteAsync_ShouldWriteTableAndReturnZero_OnSuccess() {
        // Arrange
        var logger = Substitute.For<ILogger<CalculateDistanceCommand>>();
        var client = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();
        var command = new CalculateDistanceCommand(logger, client, console);

        var settings = new CalculateDistanceCommand.Settings {
            SystemA = "SOL-A",
            SystemB = "SOL-B"
        };

        var response = new DistanceResponse {
            SystemA = "SOL-A",
            SystemB = "SOL-B",
            DistanceInLightYears = 27
        };

        client.CalculateDistance("SOL-A", "SOL-B", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(response));

        var context = CommandContextHelper.Create();

        // Act
        var exitCode = await command.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(0);
        console.Received(1).Write(Arg.Any<Table>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogErrorAndReturnOne_OnFailure() {
        // Arrange
        var logger = Substitute.For<ILogger<CalculateDistanceCommand>>();
        var client = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();
        var command = new CalculateDistanceCommand(logger, client, console);

        var settings = new CalculateDistanceCommand.Settings {
            SystemA = "SYS-X",
            SystemB = "SYS-Y"
        };

        var error = new Error("Could not calculate distance");
        client.CalculateDistance("SYS-X", "SYS-Y", Arg.Any<CancellationToken>())
            .Returns(Result.Fail<DistanceResponse>(error));

        var context = CommandContextHelper.Create();

        // Act
        var exitCode = await command.ExecuteAsync(context, settings);

        // Assert
        exitCode.Should().Be(1);
        logger.Received(1).LogError("Could not calculate distance");
        console.DidNotReceive().Write(Arg.Any<Table>());
    }

    [Theory]
    [InlineData("", "SYS-B", "You must specify a start solar system.")]
    [InlineData("   ", "SYS-B", "You must specify a start solar system.")]
    [InlineData("SYS-A", "", "You must specify an end solar system.")]
    [InlineData("SYS-A", " ", "You must specify an end solar system.")]
    public void Settings_Validate_ShouldReturnError_WhenSystemIsInvalid(string systemA, string systemB, string expectedError) {
        var settings = new CalculateDistanceCommand.Settings {
            SystemA = systemA!,
            SystemB = systemB!
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Be(expectedError);
    }

    [Fact]
    public void Settings_Validate_ShouldReturnSuccess_WhenSystemsAreValid() {
        var settings = new CalculateDistanceCommand.Settings {
            SystemA = "SYS-1",
            SystemB = "SYS-2"
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }
}