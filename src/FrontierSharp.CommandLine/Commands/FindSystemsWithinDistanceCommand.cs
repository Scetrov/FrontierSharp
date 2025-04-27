using System.ComponentModel;
using FluentResults;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.ResponseModels;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class FindSystemsWithinDistanceCommand(
    ILogger<FindSystemsWithinDistanceCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<FindSystemsWithinDistanceCommand.Settings> {
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result =
            await devToolsClient.FindSystemsWithinDistance(settings.SystemName, settings.MaxDistance,
                CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors.OfType<IError>()) logger.LogError(err.Message);

            return 1;
        }

        var systems = result.Value.NearbySystems;
        var systemsArray = systems as SystemDistanceResponse[] ?? systems.ToArray();

        if (systemsArray.Length == 0) {
            logger.LogError("No systems found within {maxDistance} of {systemName}", settings.MaxDistance,
                settings.SystemName);
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable($"Systems within {settings.MaxDistance} of {settings.SystemName}",
            "System", "Distance (LY)");

        foreach (var system in systemsArray)
            table.AddRow(system.SystemName, ((int)Math.Floor(system.DistanceInLightYears)).ToString());

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandArgument(0, "<systemName>")]
        [Description("Start Solarsystem, i.e. ICT-SVL")]
        public required string SystemName { get; set; }

        [CommandOption("--maxDistance <maxDistance>")]
        [Description("Maximum jump distance in lightyears between two systems in the route, i.e. 100")]
        public decimal MaxDistance { get; set; } = 60m;

        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(SystemName))
                return ValidationResult.Error("You must specify a start solar system.");

            return ValidationResult.Success();
        }
    }
}