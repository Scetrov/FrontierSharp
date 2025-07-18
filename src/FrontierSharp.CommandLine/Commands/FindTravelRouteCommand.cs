using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class FindTravelRouteCommand(
    ILogger<FindTravelRouteCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<FindTravelRouteCommand.Settings> {
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.FindTravelRoute(settings.Start, settings.End, settings.AvoidGates,
            settings.MaxDistance, CancellationToken.None);

        if (result.IsFailed) {
            logger.LogError("Failed to find travel route:\n{Error}", result.ToErrorString());

            return 1;
        }

        var route = result.Value.Route;
        var routeArray = route as JumpResponse[] ?? route.ToArray();

        if (routeArray.Length == 0) {
            logger.LogError("No valid route found for the specified placement.");
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable($"Route from {settings.Start} \u2192 {settings.End}", "From", "To",
            "Distance (LY)");

        foreach (var jump in routeArray)
            table.AddRow(jump.From, jump.To, ((int)Math.Floor(jump.DistanceInLightYears)).ToString());

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

        [CommandOption("--avoidGates <avoidGates>")]
        [Description("Avoid gates in the route")]
        public bool AvoidGates { get; set; }

        [CommandOption("--maxDistance <maxDistance>")]
        [Description("Maximum jump distance in lightyears between two systems in the route, i.e. 100")]
        public decimal MaxDistance { get; set; } = 499m;

        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(Start))
                return ValidationResult.Error("You must specify a start solar system.");

            if (string.IsNullOrWhiteSpace(End)) return ValidationResult.Error("You must specify an end solar system.");

            if (MaxDistance > 500)
                return ValidationResult.Error(
                    "Maximum distance must be less than 500 lightyears as this is the maximum distance for the longest jump in the game.");

            return ValidationResult.Success();
        }
    }
}