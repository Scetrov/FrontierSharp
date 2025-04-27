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

public class GetCharacterCommand(
    ILogger<GetCharacterCommand> logger,
    IFrontierDevToolsClient devToolsClient,
    IAnsiConsole ansiConsole) : AsyncCommand<GetCharacterCommand.Settings> {
    // ReSharper disable once ClassNeverInstantiated.Global
    public enum CharacterSearchType {
        Name,
        Address
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var result = settings.SearchType switch {
            CharacterSearchType.Name => await devToolsClient.GetCharactersByName(settings.Name, CancellationToken.None),
            CharacterSearchType.Address => await devToolsClient.GetCharactersByAddress(settings.Address,
                CancellationToken.None),
            _ => throw new InvalidOperationException("Invalid search type.")
        };

        if (result.IsFailed) {
            foreach (var err in result.Errors.OfType<IError>()) logger.LogError(err.Message);

            return 1;
        }

        var characters = result.Value;

        if (characters == null || !characters.Characters.Any()) {
            logger.LogError("No characters found.");
            return 1;
        }

        Table table;
        if (characters.Characters.Count() > 1) {
            table = SpectreUtils.CreateAnsiTable("Characters", "Name", "Address", "Tribe Id");

            foreach (var character in characters.Characters)
                table.AddRow(character.Name, character.Address, character.CorpId.ToString());
        }
        else {
            var character = characters.Characters.Single();
            table = SpectreUtils.CreateAnsiListing("Character", new Dictionary<string, string> {
                {
                    "Name", character.Name
                }, {
                    "Is Smart Character", character.IsSmartCharacter.ToAnsiString()
                }, {
                    "Id", character.Id.SliceMiddle()
                }, {
                    "Address", character.Address
                }, {
                    "Tribe Id", character.CorpId.ToString()
                }, {
                    "Created At", character.CreatedAt.ToAnsiString()
                }, {
                    "EVE Balance", character.EveBalanceWei.AsWeiToEther()
                }, {
                    "Gas Balance", character.GasBalanceWei.AsWeiToEther()
                }
            });
        }

        ansiConsole.Write(table);
        return 0;
    }

    public class Settings : CommandSettings {
        [CommandOption("--name <name>")]
        [Description("The name of the character")]
        public string Name { get; set; } = string.Empty;

        [CommandOption("--address <address>")]
        [Description("The address of the character")]
        public string Address { get; set; } = string.Empty;

        public CharacterSearchType SearchType {
            get {
                if (!string.IsNullOrWhiteSpace(Name)) return CharacterSearchType.Name;

                if (!string.IsNullOrWhiteSpace(Address)) return CharacterSearchType.Address;

                throw new NotImplementedException("Search Type not Implemented.");
            }
        }

        public override ValidationResult Validate() {
            var optionCount = new[] {
                Name, Address
            }.Count(x => !string.IsNullOrWhiteSpace(x));

            if (optionCount != 1)
                return ValidationResult.Error("You must specify exactly one of --name, --address, or --corpid");

            // ReSharper disable once InvertIf
            if (!string.IsNullOrWhiteSpace(Address)) {
                if (!Address.StartsWith("0x"))
                    return ValidationResult.Error("You must prefix blockchain addresses with 0x");

                if (Address.Length != 42)
                    return ValidationResult.Error("Blockchain addresses must be 42 characters long");
            }

            return ValidationResult.Success();
        }
    }
}