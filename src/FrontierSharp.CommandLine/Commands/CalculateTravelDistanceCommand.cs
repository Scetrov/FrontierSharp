using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FrontierSharp.FrontierDevTools.Api;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class CalculateTravelDistanceCommand(
    ILogger<CalculateTravelDistanceCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<CalculateTravelDistanceCommand.Settings> {
    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.CalculateTravelDistance(settings.CurrentFuel, settings.Mass,
            settings.FuelEfficiency, CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors) {
                if (err is not null) {
                    logger.LogError(err.Message);                    
                }
            }

            return 1;
        }

        ansiConsole.MarkupLine(
            $"[b]Travel distance:[/] [cyan]{result.Value.MaxTravelDistanceInLightYears:N2}[/] lightyears");
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--currentFuel <currentFuel>")]
        [Description("Amount of fuel in your ship")]
        public required decimal CurrentFuel { get; set; }

        [CommandOption("--fuelEfficiency <fuelEfficiency>")]
        [Description("Fuel efficiency of your ship, as a whole number, i.e. --fuelEfficiency 80, for SOF-80")]
        public required decimal FuelEfficiency { get; set; }

        [CommandOption("--mass <mass>")]
        [Description("Mass, in kilograms, of your ship - you can get this from \"Show Info\" in-game.")]
        public required decimal Mass { get; set; }

        public override ValidationResult Validate() {
            if (CurrentFuel <= 0)
                return ValidationResult.Error("The amount of fuel in your ship must be greater than 0.");

            switch (FuelEfficiency) {
                case <= 0:
                    return ValidationResult.Error("There is no fuel efficiency of 0 or less in the game.");
                case > 90:
                    return ValidationResult.Error("There is no fuel efficiency greater than 90 in the game.");
            }

            if (Mass <= 0) return ValidationResult.Error("The mass of your ship must be greater than 0.");

            return ValidationResult.Success();
        }
    }
}