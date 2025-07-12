using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class OptimalStargateNetworkAndDeploymentCommand(
    ILogger<OptimalStargateNetworkAndDeploymentCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<OptimalStargateNetworkAndDeploymentCommand.Settings> {
    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.OptimalStargateNetworkAndDeployment(settings.Start, settings.End,
            settings.MaxStargateDistance, settings.NpcAvoidanceLevel, settings.AvoidGates, settings.IncludeShips,
            CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors) {
                if (err is not null) {
                    logger.LogError(err.Message);                    
                }
            }

            return 1;
        }

        var summary = result.Value.Results.OptimalRoute;
        var functionalRoute = result.Value.Results.FunctionalRoute.ToArray();
        var travelLog = result.Value.Results.TravelLog.ToArray();

        var summaryDictionary = new Dictionary<string, string> {
            {
                "Start", summary.StartSystem
            }, {
                "End", summary.EndSystem
            }, {
                "Max Distance (LY)", summary.TotalGateDistance.ToString()
            }, {
                "Ship", summary.Ship
            }, {
                "Fuel Type", summary.FuelType
            }, {
                "Fuel Volume", summary.FuelVolume.ToString(CultureInfo.InvariantCulture)
            }, {
                "Fuel Used", summary.FuelVolumeUsed.ToString(CultureInfo.InvariantCulture)
            }, {
                "FuelCost", summary.FuelCost.ToString(CultureInfo.InvariantCulture)
            }, {
                "Systems Reached", summary.SystemsReached.ToString()
            }, {
                "Gates Deployed", summary.GatesDeployed.ToString()
            }
        };

        var summaryTable = SpectreUtils.CreateAnsiListing("Summary", summaryDictionary);

        ansiConsole.Write(summaryTable);

        if (functionalRoute.Length == 0) {
            logger.LogError("No valid route found for the specified placement.");
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable($"Gate Placement {settings.Start} \u2192 {settings.End}", "From", "To",
            "Fuel Required", "Distance (LY)", "Jumps", "Ship Path");

        foreach (var waypoint in functionalRoute) {
            var fuelRequired = waypoint.FuelRequired.ToString(CultureInfo.InvariantCulture);
            var distance = waypoint.Distance.ToString(CultureInfo.InvariantCulture);
            var jumps = waypoint.Jumps.ToString(CultureInfo.InvariantCulture);
            var shipPath = waypoint.ShipPath.FormatShipPath();
            table.AddRow(waypoint.DeployFrom, waypoint.DeployTo, fuelRequired, distance, jumps, shipPath);
        }

        ansiConsole.Write(table);

        var logTable = SpectreUtils.CreateAnsiTable("Travel Log", "From", "To", "Jumps", "Fuel Used", "Distance (LY)");

        foreach (var travelLogEntry in travelLog)
            logTable.AddRow(travelLogEntry.From, travelLogEntry.To, travelLogEntry.Jumps.ToString(),
                travelLogEntry.FuelUsed.ToString(CultureInfo.InvariantCulture), travelLogEntry.Distance.ToString());

        ansiConsole.Write(logTable);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--start <start>")]
        [Description("Start Solarsystem, i.e. ICT-SVL")]
        public required string Start { get; set; }

        [CommandOption("--end <end>")]
        [Description("End Solarsystem, i.e. UB3-3QJ")]
        public required string End { get; set; }

        [CommandOption("--maxStargateDistance <maxStargateDistance>")]
        [Description("Maximum jump distance in lightyears between two smart gates in the route, i.e. 499")]
        public decimal MaxStargateDistance { get; set; } = 499m;

        [CommandOption("--npcAvoidanceLevel <npcAvoidanceLevel>")]
        [Description("Level of NPC avoidance")]
        public NpcAvoidanceLevel NpcAvoidanceLevel { get; set; } = NpcAvoidanceLevel.High;

        [CommandOption("--ship <ship>")]
        [Description("Ship to use to deploy the route")]
        public string IncludeShips { get; set; } = string.Empty;

        [CommandOption("--avoidGates")]
        [Description("Whether to avoid (True) NPC Gates or not (False")]
        public bool AvoidGates { get; set; } = false;

        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(Start))
                return ValidationResult.Error("You must specify a start solar system.");

            if (string.IsNullOrWhiteSpace(End)) return ValidationResult.Error("You must specify an end solar system.");

            if (MaxStargateDistance < 1)
                return ValidationResult.Error("Maximum distance must be greater than 1 lightyears.");

            if (MaxStargateDistance > 500)
                return ValidationResult.Error(
                    "Maximum distance must be less than 500 lightyears as this is the maximum distance for the longest jump in the game.");

            return ValidationResult.Success();
        }
    }
}