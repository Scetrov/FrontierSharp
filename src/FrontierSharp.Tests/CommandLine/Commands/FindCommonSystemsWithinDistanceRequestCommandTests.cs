using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using FluentResults;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

[SuppressMessage("Usage", "NS5000:Received check.")]
[SuppressMessage("Non-substitutable member", "NS1004:Argument matcher used with a non-virtual member of a class.")]
[SuppressMessage("Non-substitutable member", "NS1001:Non-virtual setup specification.")]
public class FindCommonSystemsWithinDistanceRequestCommandTests {
    private readonly IFrontierDevToolsClient _client = Substitute.For<IFrontierDevToolsClient>();
    private readonly IAnsiConsole _console = Substitute.For<IAnsiConsole>();

    private readonly ILogger<FindCommonSystemsWithinDistanceRequestCommand> _logger =
        Substitute.For<ILogger<FindCommonSystemsWithinDistanceRequestCommand>>();

    private FindCommonSystemsWithinDistanceRequestCommand CreateCommand() {
        return new FindCommonSystemsWithinDistanceRequestCommand(_logger, _client, _console);
    }

    private static FindCommonSystemsWithinDistanceRequestCommand.Settings CreateSettings(
        string a = "ABC-1", string b = "XYZ-9", decimal distance = 400m) {
        return new FindCommonSystemsWithinDistanceRequestCommand.Settings {
            SystemA = a,
            SystemB = b,
            MaxDistance = distance
        };
    }

    [Fact]
    public async Task ExecuteAsync_Returns1_WhenApiFails() {
        var command = CreateCommand();
        var settings = CreateSettings();

        var error = Result.Fail<CommonSystemsWithinDistanceResponse>("Something went wrong");
        _client.FindCommonSystemsWithinDistance(settings.SystemA, settings.SystemB, settings.MaxDistance,
                Arg.Any<CancellationToken>())
            .Returns(error);

        var result = await command.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Returns1_WhenNoCommonSystemsFound() {
        var command = CreateCommand();
        var settings = CreateSettings();

        var success = Result.Ok(new CommonSystemsWithinDistanceResponse {
            ReferenceSystems = [settings.SystemA, settings.SystemB],
            CommonSystems = []
        });

        _client.FindCommonSystemsWithinDistance(settings.SystemA, settings.SystemB, settings.MaxDistance,
                Arg.Any<CancellationToken>())
            .Returns(success);

        var result = await command.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Returns0_WhenCommonSystemsFound() {
        var command = CreateCommand();
        var settings = CreateSettings();

        var systems = new List<CommonSystemsResponse> {
            new() {
                SystemName = "SYS-1",
                DistanceFromAInLy = 12.3m,
                DistanceFromBInLy = 14.6m,
                NpcGates = 2
            }
        };

        var success = Result.Ok(new CommonSystemsWithinDistanceResponse {
            ReferenceSystems = [settings.SystemA, settings.SystemB],
            CommonSystems = systems
        });

        _client.FindCommonSystemsWithinDistance(settings.SystemA, settings.SystemB, settings.MaxDistance,
                Arg.Any<CancellationToken>())
            .Returns(success);

        var result = await command.ExecuteAsync(CommandContextHelper.Create(), settings, CancellationToken.None);

        result.Should().Be(0);
        _console.Received().Write(Arg.Any<IRenderable>());
    }

    [Fact]
    public void Validate_ReturnsError_IfSystemAIsMissing() {
        var settings = CreateSettings("");
        var validation = settings.Validate();

        validation.Successful.Should().BeFalse();
        validation.Message.Should().Be("You must specify a start solar system.");
    }

    [Fact]
    public void Validate_ReturnsError_IfSystemBIsMissing() {
        var settings = CreateSettings(b: "");
        var validation = settings.Validate();

        validation.Successful.Should().BeFalse();
        validation.Message.Should().Be("You must specify an end solar system.");
    }

    [Fact]
    public void Validate_ReturnsSuccess_IfInputsAreValid() {
        var settings = CreateSettings();
        var validation = settings.Validate();

        validation.Successful.Should().BeTrue();
    }
}