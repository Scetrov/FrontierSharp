using System.ComponentModel;
using FluentResults;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands;

public class GetCorporationCommand(ILogger<GetCorporationCommand> logger, IFrontierDevToolsClient devToolsClient, IAnsiConsole ansiConsole) : AsyncCommand<GetCorporationCommand.Settings> {
    // ReSharper disable once ClassNeverInstantiated.Global
    public enum CorporationSearchType {
        Id,
        Player
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GetCorporationCommand.Settings settings) {
        logger.LogInformation("Getting corporations for player {searchType} - [{playerName}|{id}]", settings.SearchType, settings.PlayerName, settings.Id);
        var result = settings.SearchType switch {
            CorporationSearchType.Id => await devToolsClient.GetCharactersByCorpId(settings.Id!.Value, CancellationToken.None),
            CorporationSearchType.Player => await devToolsClient.GetCharactersByPlayer(settings.PlayerName, CancellationToken.None),
            _ => throw new InvalidOperationException("Invalid search type.")
        };

        if (result.IsFailed) {
            foreach (var err in result.Errors.OfType<IError>()) {
                logger.LogError(err.Message);
            }

            return 1;
        }

        var corporation = result.Value;
        
        if (corporation == null || !corporation.CorpCharacters.Any()) {
            logger.LogError("No characters found for that corporation.");
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable((settings.Id.HasValue ? settings.Id.ToString() : settings.PlayerName)!, "Player");

        foreach (var character in corporation.CorpCharacters) {
            table.AddRow(character);
        }

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--id <id>")]
        [Description("The ID of the corporation")]
        public int? Id { get; set; } = null;

        [CommandOption("--player <player>")]
        [Description("The name of a player in the corporation")]
        public string PlayerName { get; set; } = string.Empty;

        public CorporationSearchType SearchType {
            get {
                if (Id.HasValue) {
                    return CorporationSearchType.Id;
                }

                if (!string.IsNullOrWhiteSpace(PlayerName)) {
                    return CorporationSearchType.Player;
                }

                throw new NotImplementedException("Search Type not Implemented.");
            }
        }

        public override ValidationResult Validate() {
            var optionCount = 0;
            
            if (Id.HasValue) {
                optionCount++;
            }
            
            if (!string.IsNullOrWhiteSpace(PlayerName)) {
                optionCount++;
            }

            if (optionCount != 1) {
                return ValidationResult.Error("You must specify exactly one of --id, --address, or --player");
            }

            return ValidationResult.Success();
        }
    }
}