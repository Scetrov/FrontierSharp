using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class CalculateDistanceCommand(
    ILogger<CalculateDistanceCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<CalculateDistanceCommand.Settings> {
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.CalculateDistance(settings.SystemA, settings.SystemB, CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors) logger.LogError(err.Message);

            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable($"Distance {settings.SystemA} \u2192 {settings.SystemB}", "SystemA",
            "SystemB", "Distance (LY)");

        table.AddRow(result.Value.SystemA, result.Value.SystemB,
            ((int)Math.Floor(result.Value.DistanceInLightYears)).ToString());

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandArgument(0, "<a>")]
        [Description("Start Solarsystem, i.e. ICT-SVL")]
        public required string SystemA { get; init; }

        [CommandArgument(1, "<b>")]
        [Description("End Solarsystem, i.e. UB3-3QJ")]
        public required string SystemB { get; init; }

        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(SystemA))
                return ValidationResult.Error("You must specify a start solar system.");

            if (string.IsNullOrWhiteSpace(SystemB))
                return ValidationResult.Error("You must specify an end solar system.");

            return ValidationResult.Success();
        }
    }
}