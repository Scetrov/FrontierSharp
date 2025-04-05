using FluentAssertions;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

public class OptimizeStargateNetworkPlacementTests {
    private readonly IFrontierDevToolsClient _client = Substitute.For<IFrontierDevToolsClient>();

    private readonly OptimizeStargateNetworkPlacement _command;
    private readonly IAnsiConsole _console = Substitute.For<IAnsiConsole>();
    private readonly ILogger<GetCorporationCommand> _logger = Substitute.For<ILogger<GetCorporationCommand>>();

    public OptimizeStargateNetworkPlacementTests() {
        _command = new OptimizeStargateNetworkPlacement(_logger, _client, _console);
    }

    [Fact]
    public void Settings_ShouldValidateSuccessfully_WithValidInput() {
        var settings = new OptimizeStargateNetworkPlacement.Settings {
            Start = "A",
            End = "B",
            MaxDistance = 499m,
            NpcAvoidanceLevel = NPCAvodianceLevel.High
        };

        var result = settings.Validate();

        result.Successful.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "B")]
    [InlineData("A", "")]
    public void Settings_ShouldFailValidation_WhenStartOrEndMissing(string start, string end) {
        var settings = new OptimizeStargateNetworkPlacement.Settings {
            Start = start,
            End = end,
            MaxDistance = 499
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
    }

    [Fact]
    public void Settings_ShouldFailValidation_WhenMaxDistanceTooHigh() {
        var settings = new OptimizeStargateNetworkPlacement.Settings {
            Start = "X",
            End = "Y",
            MaxDistance = 600
        };

        var result = settings.Validate();

        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("Maximum distance must be less than 500");
    }
}