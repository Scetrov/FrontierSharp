using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class SolarSystemCommand(
    ILogger<SolarSystemCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration)
    : BaseWorldApiCommand<SolarSystemCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 1000;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (settings.Id.HasValue) return await ShowByIdAsync(settings, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name)) return await ShowByNameAsync(settings, cancellationToken);

        Logger.LogError("You must specify --id, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("Solar Systems", "Id", "Name");
        var offset = 0L;

        while (true) {
            var page = await WorldApiClient.GetSolarSystemPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch solar system list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var sys in page.Value.Data)
                table.AddRow(sys.Id.ToString(), sys.Name);

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByIdAsync(Settings settings, CancellationToken cancellationToken) {
        var res = await WorldApiClient.GetSolarSystemById(settings.Id!.Value, cancellationToken);
        if (res.IsFailed) {
            Logger.LogError("Failed to load solar system {Id}: {Error}", settings.Id, res.ToErrorString());
            return 1;
        }

        if (res.Value == null) {
            Logger.LogError("Solar system {Id} not found", settings.Id);
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable(res.Value.Name, "Key", "Value");
        table.AddRow("Id", res.Value.Id.ToString());
        table.AddRow("Name", res.Value.Name);
        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByNameAsync(Settings settings, CancellationToken cancellationToken) {
        var pages = await LoadAllPagesAsync(WorldApiClient.GetSolarSystemPage, settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (pages.IsFailed) {
            Logger.LogError("Failed to load solar systems: {Error}", pages.ToErrorString());
            return 1;
        }

        var list = pages.Value;
        var exact = list.FirstOrDefault(x => string.Equals(x.Name, settings.Name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var detail = await WorldApiClient.GetSolarSystemById(exact.Id, cancellationToken);
            if (detail.IsFailed) {
                Logger.LogError("Unable to load solar system {Id}: {Error}", exact.Id, detail.ToErrorString());
                return 1;
            }

            var table = SpectreUtils.CreateAnsiTable(detail.Value.Name, "Key", "Value");
            table.AddRow("Id", detail.Value.Id.ToString());
            table.AddRow("Name", detail.Value.Name);
            AnsiConsole.Write(table);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(list, settings.Name!, s => s.Name);
        if (!candidates.Any()) {
            Logger.LogError("No solar systems could be resolved for '{Name}'", settings.Name);
            return 1;
        }

        var best = candidates.First().Distance;
        RenderFuzzyWarning(settings.Name!, best);
        if (candidates.Count > 1) RenderMultipleMatches(candidates, s => s.Name, s => s.Id.ToString());

        var selected = candidates.First().Value;
        var final = await WorldApiClient.GetSolarSystemById(selected.Id, cancellationToken);
        if (final.IsFailed) {
            Logger.LogError("Failed to load solar system {Id}: {Error}", selected.Id, final.ToErrorString());
            return 1;
        }

        var tbl = SpectreUtils.CreateAnsiTable(final.Value.Name, "Key", "Value");
        tbl.AddRow("Id", final.Value.Id.ToString());
        tbl.AddRow("Name", final.Value.Name);
        AnsiConsole.Write(tbl);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--id <id>")]
        [Description("Solar system identifier")]
        public long? Id { get; set; }

        [CommandOption("--name <name>")]
        [Description("Solar system name (fuzzy search)")]
        public string? Name { get; set; }
    }
}