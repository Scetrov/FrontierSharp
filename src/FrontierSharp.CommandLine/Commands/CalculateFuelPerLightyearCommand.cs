using System.ComponentModel;
using FluentResults;
using FrontierSharp.FrontierDevTools.Api;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class CalculateFuelPerLightyearCommand(ILogger<CalculateFuelPerLightyearCommand> logger, IFrontierDevToolsClient devToolsClient, IAnsiConsole ansiConsole) : AsyncCommand<CalculateFuelPerLightyearCommand.Settings> {
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.CalculateFuelPerLightyear(settings.Mass, settings.FuelEfficiency, CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors.OfType<IError>()) {
                logger.LogError(err.Message);
            }

            return 1;
        }

        ansiConsole.MarkupLine($"[b]Fuel required:[/] [cyan]{result.Value.FuelPerLightyear:N2}[/] ({settings.FuelEfficiency / 100:P0} Fuel)");
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--mass <mass>")]
        [Description("Mass, in kilograms, of your ship - you can get this from \"Show Info\" in-game.")]
        public required decimal Mass { get; set; }

        [CommandOption("--fuelEfficiency <fuelEfficiency>")]
        [Description("Fuel efficiency of your ship, as a whole number, i.e. --fuelEfficiency 80, for SOF-80")]
        public required decimal FuelEfficiency { get; set; }

        public override ValidationResult Validate() {
            switch (FuelEfficiency) {
                case <= 0:
                    return ValidationResult.Error("There is no fuel efficiency of 0 or less in the game.");
                case > 90:
                    return ValidationResult.Error("There is no fuel efficiency greater than 90 in the game.");
            }

            if (Mass <= 0) {
                return ValidationResult.Error("The mass of your ship must be greater than 0.");
            }

            return ValidationResult.Success();
        }
    }
}