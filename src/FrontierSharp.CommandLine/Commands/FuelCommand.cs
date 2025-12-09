using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class FuelCommand(
    ILogger<FuelCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration) : BaseWorldApiCommand<FuelCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 100;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (settings.Id.HasValue) return await ShowByTypeIdAsync(settings.Id.Value, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name))
            return await ShowByTypeNameAsync(settings.Name!, settings.PageSize ?? DefaultPageSize, cancellationToken);

        Logger.LogError("You must specify --id, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("Fuels", "TypeId", "TypeName", "Efficiency");
        var offset = 0L;

        while (true) {
            var page = await WorldApiClient.GetFuelsPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch fuels list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var f in page.Value.Data)
                table.AddRow(f.Type.Id.ToString(), f.Type.Name, ((int)f.Efficiency).FuelToAnsiString());

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByTypeIdAsync(long typeId, CancellationToken cancellationToken) {
        var all = await WorldApiClient.GetAllFuels(100, cancellationToken);
        if (all.IsFailed) {
            Logger.LogError("Failed to fetch fuels: {Error}", all.ToErrorString());
            return 1;
        }

        var f = all.Value.FirstOrDefault(x => x.Type.Id == typeId);
        if (f == null) {
            Logger.LogError("Fuel for type {Id} not found", typeId);
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable(f.Type.Name, "Key", "Value");
        table.AddRow("TypeId", f.Type.Id.ToString());
        table.AddRow("TypeName", f.Type.Name);
        table.AddRow("Efficiency", ((int)f.Efficiency).FuelToAnsiString());
        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByTypeNameAsync(string name, int pageSize, CancellationToken cancellationToken) {
        var pages = await LoadAllPagesAsync(WorldApiClient.GetFuelsPage, pageSize, cancellationToken);
        if (pages.IsFailed) {
            Logger.LogError("Failed to load fuels: {Error}", pages.ToErrorString());
            return 1;
        }

        var list = pages.Value;
        var exact = list.FirstOrDefault(x => string.Equals(x.Type.Name, name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var tbl = SpectreUtils.CreateAnsiTable(exact.Type.Name, "Key", "Value");
            tbl.AddRow("TypeId", exact.Type.Id.ToString());
            tbl.AddRow("TypeName", exact.Type.Name);
            tbl.AddRow("Efficiency", ((int)exact.Efficiency).FuelToAnsiString());
            AnsiConsole.Write(tbl);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(list, name, f => f.Type.Name);
        if (!candidates.Any()) {
            Logger.LogError("No fuels could be resolved for '{Name}'", name);
            return 1;
        }

        var best = candidates.First().Distance;
        RenderFuzzyWarning(name, best);
        if (candidates.Count > 1) RenderMultipleMatches(candidates, f => f.Type.Name, f => f.Type.Id.ToString());

        var selected = candidates.First().Value;
        var finalTable = SpectreUtils.CreateAnsiTable(selected.Type.Name, "Key", "Value");
        finalTable.AddRow("TypeId", selected.Type.Id.ToString());
        finalTable.AddRow("TypeName", selected.Type.Name);
        finalTable.AddRow("Efficiency", ((int)selected.Efficiency).FuelToAnsiString());
        AnsiConsole.Write(finalTable);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--id <id>")]
        [Description("Type identifier for fuel (exact)")]
        public long? Id { get; set; }

        [CommandOption("--name <name>")]
        [Description("Type name for fuel (fuzzy)")]
        public string? Name { get; set; }
    }
}