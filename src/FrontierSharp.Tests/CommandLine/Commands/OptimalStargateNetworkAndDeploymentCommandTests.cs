using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentResults;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands;

[SuppressMessage("Non-substitutable member", "NS1001:Non-virtual setup specification.")]
[SuppressMessage("Usage", "NS5000:Received check.")]
public class OptimalStargateNetworkAndDeploymentCommandTests {
    [Fact]
    public async Task ExecuteAsync_ShouldRenderTablesAndReturnSuccess_WhenRouteIsValid() {
        var logger = Substitute.For<ILogger<OptimalStargateNetworkAndDeploymentCommand>>();
        var devToolsClient = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();

        var response = new OptimalStargateNetworkAndDeploymentResponse {
            Results = new RouteResults {
                OptimalRoute = new OptimalRoute {
                    StartSystem = "ICT-SVL",
                    EndSystem = "UB3-3QJ",
                    TotalGateDistance = 123,
                    Ship = "Cruiser",
                    FuelType = "Hydrazine",
                    FuelVolume = 250,
                    FuelVolumeUsed = 120,
                    FuelCost = 56.78m,
                    SystemsReached = 10,
                    GatesDeployed = 5
                },
                FunctionalRoute = [
                    new FunctionalRouteSegment {
                        DeployFrom = "A",
                        DeployTo = "B",
                        FuelRequired = 10.5m,
                        Distance = 22,
                        Jumps = 3,
                        ShipPath = [
                            new ShipPathSegment {
                                From = "A",
                                To = "B",
                                Distance = 60
                            },
                            new ShipPathSegment {
                                From = "B",
                                To = "C",
                                Distance = 63
                            }
                        ]
                    }
                ],
                TravelLog = [
                    new TravelLogEntry {
                        From = "A",
                        To = "B",
                        Jumps = 2,
                        FuelUsed = 5,
                        Distance = 15
                    }
                ]
            }
        };

        devToolsClient
            .OptimalStargateNetworkAndDeployment("ICT-SVL", "UB3-3QJ")
            .Returns(Result.Ok(response));

        var command = new OptimalStargateNetworkAndDeploymentCommand(logger, devToolsClient, console);
        var settings = new OptimalStargateNetworkAndDeploymentCommand.Settings {
            Start = "ICT-SVL",
            End = "UB3-3QJ",
            MaxStargateDistance = 499m,
            NpcAvoidanceLevel = NpcAvoidanceLevel.High,
            IncludeShips = "Flegel",
            AvoidGates = false
        };

        var result = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(0);

        await devToolsClient.Received(1).OptimalStargateNetworkAndDeployment("ICT-SVL", "UB3-3QJ");
        console.Received().Write(Arg.Any<IRenderable>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogErrorAndReturnOne_WhenResultIsFailed() {
        var logger = Substitute.For<ILogger<OptimalStargateNetworkAndDeploymentCommand>>();
        var devToolsClient = Substitute.For<IFrontierDevToolsClient>();
        var console = Substitute.For<IAnsiConsole>();

        var resultWithError = Result.Fail<OptimalStargateNetworkAndDeploymentResponse>("Route calculation failed");

        devToolsClient
            .OptimalStargateNetworkAndDeployment("Start", "End", 499m, NpcAvoidanceLevel.High, false, "Ship")
            .Returns(resultWithError);

        var command = new OptimalStargateNetworkAndDeploymentCommand(logger, devToolsClient, console);
        var settings = new OptimalStargateNetworkAndDeploymentCommand.Settings {
            Start = "Start",
            End = "End",
            IncludeShips = "Ship",
            AvoidGates = false,
            NpcAvoidanceLevel = NpcAvoidanceLevel.High
        };

        var result = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        result.Should().Be(1);
        logger.Received().LogError("Route calculation failed");
    }
}