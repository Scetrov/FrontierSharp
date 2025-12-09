using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class KillmailCommand(
    ILogger<KillmailCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration)
    : BaseWorldApiCommand<KillmailCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 100;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.VictimName)) return await ShowByVictimNameAsync(settings.VictimName, settings, cancellationToken);

        Logger.LogError("You must specify --victim-name or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("Killmails", "Id", "Victim", "Time");
        var offset = 0L;

        while (true) {
            var page = await WorldApiClient.GetKillmailPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch killmail list: {Error}", page.ToErrorString());
                return 1;
            }

            // Display a sequential index as the identifier since Killmail has no numeric Id
            var idx = 0L;
            foreach (var k in page.Value.Data) {
                var seq = (offset + idx + 1).ToString();
                table.AddRow(seq, k.Victim.Name.EscapeMarkup(), k.Time.ToAnsiString());
                idx++;
            }

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async Task<int> ShowByVictimNameAsync(string name, Settings settings, CancellationToken cancellationToken) {
        var all = await WorldApiClient.GetAllKillmails(100, cancellationToken);
        if (all.IsFailed) {
            Logger.LogError("Failed to fetch killmails: {Error}", all.ToErrorString());
            return 1;
        }

        var list = all.Value;
        var enumerable = list as Killmail[] ?? list.ToArray();
        var exact = enumerable.FirstOrDefault(k => string.Equals(k.Victim.Name, name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var tbl = SpectreUtils.CreateAnsiTable($"Killmail: {exact.Victim.Name}", "Key", "Value");
            tbl.AddRow("Victim", exact.Victim.Name);
            tbl.AddRow("Time", exact.Time.ToAnsiString());
            AnsiConsole.Write(tbl);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(enumerable, name, k => k.Victim.Name);
        if (!candidates.Any()) {
            Logger.LogError("No killmails could be resolved for '{Name}'", name);
            return 1;
        }

        var best = candidates.First().Distance;
        RenderFuzzyWarning(name, best);
        if (candidates.Count > 1) RenderMultipleMatches(candidates, k => k.Victim.Name, k => k.Time.ToString());

        var selected = candidates.First().Value;
        var tbl2 = SpectreUtils.CreateAnsiTable($"Killmail: {selected.Victim.Name}", "Key", "Value");
        tbl2.AddRow("Victim", selected.Victim.Name);
        tbl2.AddRow("Time", selected.Time.ToAnsiString());
        AnsiConsole.Write(tbl2);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--victim-name <name>")]
        [Description("Victim name to search for (fuzzy)")]
        public string? VictimName { get; set; }
    }
}