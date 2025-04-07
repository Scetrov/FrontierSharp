using System.ComponentModel;
using FluentResults;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class OptimizeStargateNetworkPlacementCommand(ILogger<OptimizeStargateNetworkPlacementCommand> logger, IFrontierDevToolsClient devToolsClient, IAnsiConsole ansiConsole) : AsyncCommand<OptimizeStargateNetworkPlacementCommand.Settings> {
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.OptimizeStargateAndNetworkPlacement(settings.Start, settings.End, settings.MaxDistance, settings.NpcAvoidanceLevel, CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors.OfType<IError>()) {
                logger.LogError(err.Message);
            }

            return 1;
        }

        var route = result.Value.Route;
        var routeArray = route as JumpResponse[] ?? route.ToArray();

        if (routeArray.Length == 0) {
            logger.LogError("No valid route found for the specified placement.");
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable($"Gate Placement {settings.Start} \u2192 {settings.End}", "From", "To", "Distance (LY)");

        foreach (var jump in routeArray) {
            table.AddRow(jump.From, jump.To, ((int)Math.Floor(jump.DistanceInLightYears)).ToString());
        }

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--start <start>")]
        [Description("Start Solarsystem, i.e. ICT-SVL")]
        public required string Start { get; set; }

        [CommandOption("--end <end>")]
        [Description("End Solarsystem, i.e. UB3-3QJ")]
        public required string End { get; set; }

        [CommandOption("--maxDistance <maxDistance>")]
        [Description("Maximum jump distance in lightyears between two systems in the route, i.e. 499")]
        public decimal MaxDistance { get; set; } = 499m;

        [CommandOption("--npcAvoidanceLevel <npcAvoidanceLevel>")]
        [Description("Level of NPC avoidance")]
        public NpcAvoidanceLevel NpcAvoidanceLevel { get; set; } = NpcAvoidanceLevel.High;


        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(Start)) {
                return ValidationResult.Error("You must specify a start solar system.");
            }

            if (string.IsNullOrWhiteSpace(End)) {
                return ValidationResult.Error("You must specify an end solar system.");
            }

            if (MaxDistance > 500) {
                return ValidationResult.Error("Maximum distance must be less than 500 lightyears as this is the maximum distance for the longest jump in the game.");
            }

            return ValidationResult.Success();
        }
    }
}