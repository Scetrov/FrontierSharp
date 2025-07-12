using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FluentResults;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class FindCommonSystemsWithinDistanceRequestCommand(
    ILogger<FindCommonSystemsWithinDistanceRequestCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<FindCommonSystemsWithinDistanceRequestCommand.Settings> {
    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.FindCommonSystemsWithinDistance(settings.SystemA, settings.SystemB,
            settings.MaxDistance, CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors) {
                if (err is not null) {
                    logger.LogError(err.Message);                    
                }
            }

            return 1;
        }

        if (!result.Value.CommonSystems.Any()) {
            logger.LogError("No common systems found within {maxDistance} of {systemA} and {systemB}",
                settings.MaxDistance, settings.SystemA, settings.SystemB);
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable(
            $"Common systems between {result.Value.ReferenceSystems.First()} and {result.Value.ReferenceSystems.Last()}",
            "System Name", "Distance from A (LY)", "Distance from B (LY)", "NPC Gates");

        foreach (var system in result.Value.CommonSystems)
            table.AddRow(system.SystemName, ((int)Math.Floor(system.DistanceFromAInLy)).ToString(),
                ((int)Math.Floor(system.DistanceFromBInLy)).ToString(), system.NpcGates.ToString());

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandArgument(0, "<a>")]
        [Description("Start Solarsystem, i.e. ICT-SVL")]
        public required string SystemA { get; set; }

        [CommandArgument(1, "<b>")]
        [Description("End Solarsystem, i.e. UB3-3QJ")]
        public required string SystemB { get; set; }

        [CommandOption("--maxDistance <maxDistance>")]
        [Description("Maximum jump distance in lightyears between two systems in the route, i.e. 400")]
        public decimal MaxDistance { get; set; } = 400m;

        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(SystemA))
                return ValidationResult.Error("You must specify a start solar system.");

            if (string.IsNullOrWhiteSpace(SystemB))
                return ValidationResult.Error("You must specify an end solar system.");

            return ValidationResult.Success();
        }
    }
}