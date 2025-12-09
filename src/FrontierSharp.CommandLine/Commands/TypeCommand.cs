using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class TypeCommand(
    ILogger<TypeCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration) : BaseWorldApiCommand<TypeCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 100;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (settings.Id.HasValue) return await ShowByIdAsync(settings.Id.Value, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name)) return await ShowByNameAsync(settings, cancellationToken);

        Logger.LogError("You must specify --id, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("Types", "Id", "Name", "Group");
        var offset = 0L;

        while (true) {
            var page = await WorldApiClient.GetTypesPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch types list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var t in page.Value.Data)
                table.AddRow(t.Id.ToString(), t.Name, t.GroupName);

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByIdAsync(long id, CancellationToken cancellationToken) {
        var res = await WorldApiClient.GetTypeById(id, cancellationToken);
        if (res.IsFailed) {
            Logger.LogError("Failed to load type {Id}: {Error}", id, res.ToErrorString());
            return 1;
        }

        var t = res.Value;
        if (t == null) {
            Logger.LogError("Type {Id} not found", id);
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable(t.Name, "Key", "Value");
        table.AddRow("Id", t.Id.ToString());
        table.AddRow("Name", t.Name);
        table.AddRow("Group", t.GroupName);
        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByNameAsync(Settings settings, CancellationToken cancellationToken) {
        var pages = await LoadAllPagesAsync(WorldApiClient.GetTypesPage, settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (pages.IsFailed) {
            Logger.LogError("Failed to load types: {Error}", pages.ToErrorString());
            return 1;
        }

        var list = pages.Value;
        var exact = list.FirstOrDefault(x => string.Equals(x.Name, settings.Name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var detail = await WorldApiClient.GetTypeById(exact.Id, cancellationToken);
            if (detail.IsFailed) {
                Logger.LogError("Unable to load type {Id}: {Error}", exact.Id, detail.ToErrorString());
                return 1;
            }

            var table = SpectreUtils.CreateAnsiTable(detail.Value.Name, "Key", "Value");
            table.AddRow("Id", detail.Value.Id.ToString());
            table.AddRow("Name", detail.Value.Name);
            table.AddRow("Group", detail.Value.GroupName);
            AnsiConsole.Write(table);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(list, settings.Name!, s => s.Name);
        if (!candidates.Any()) {
            Logger.LogError("No types could be resolved for '{Name}'", settings.Name);
            return 1;
        }

        var best = candidates.First().Distance;
        RenderFuzzyWarning(settings.Name!, best);
        if (candidates.Count > 1) RenderMultipleMatches(candidates, s => s.Name, s => s.Id.ToString());

        var selected = candidates.First().Value;
        var final = await WorldApiClient.GetTypeById(selected.Id, cancellationToken);
        if (final.IsFailed) {
            Logger.LogError("Failed to load type {Id}: {Error}", selected.Id, final.ToErrorString());
            return 1;
        }

        var tbl = SpectreUtils.CreateAnsiTable(final.Value.Name, "Key", "Value");
        tbl.AddRow("Id", final.Value.Id.ToString());
        tbl.AddRow("Name", final.Value.Name);
        tbl.AddRow("Group", final.Value.GroupName);
        AnsiConsole.Write(tbl);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--id <id>")]
        [Description("Type identifier")]
        public long? Id { get; set; }

        [CommandOption("--name <name>")]
        [Description("Type name (fuzzy search)")]
        public string? Name { get; set; }
    }
}