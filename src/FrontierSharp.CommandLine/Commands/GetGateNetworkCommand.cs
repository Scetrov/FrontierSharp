using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FluentResults;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class GetGateNetworkCommand(
    ILogger<GetGateNetworkCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<GetGateNetworkCommand.Settings> {
    // ReSharper disable once ClassNeverInstantiated.Global
    public enum CorporationSearchType {
        Id,
        Player
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = await devToolsClient.GetGateNetwork(settings.Identifier, CancellationToken.None);

        if (result.IsFailed) {
            foreach (var err in result.Errors) {
                if (err is not null) {
                    logger.LogError(err.Message);                    
                }
            }

            return 1;
        }

        var gateNetwork = result.Value;

        if (gateNetwork == null || !gateNetwork.GateNetwork.Any()) {
            logger.LogError("No gates found for that corporation.");
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable($"Gate Network for {settings.Identifier}", "From", "To", "Owner",
            "Fuel", "From Online", "To Online");

        foreach (var gate in gateNetwork.GateNetwork)
            table.AddRow(gate.FromSystem, gate.ToSystem ?? "[gray]Not Connected[/]", gate.Owner,
                gate.FuelAmount.FuelToAnsiString(), gate.FromIsOnline.ToAnsiString(), gate.ToIsOnline.ToAnsiString());

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--identifier <identifier>")]
        [Description("Identifier of the corp, i.e. 98000001, or player name, i.e. Scetrov")]
        public required string Identifier { get; set; }

        public override ValidationResult Validate() {
            if (string.IsNullOrWhiteSpace(Identifier)) return ValidationResult.Error("You must specify an identifier.");

            return ValidationResult.Success();
        }
    }
}