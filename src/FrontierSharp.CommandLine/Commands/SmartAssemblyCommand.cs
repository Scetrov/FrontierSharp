using System.ComponentModel;
using System.Numerics;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class SmartAssemblyCommand(
    ILogger<SmartAssemblyCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration)
    : BaseWorldApiCommand<SmartAssemblyCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 100;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.Id.HasValue) return await ShowByIdAsync(settings, cancellationToken);
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name)) return await ShowByNameAsync(settings, cancellationToken);

        Logger.LogError("You must specify --id, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("Assemblies", "Id", "Type", "SolarSystem");
        var offset = 0L;

        while (true) {
            var page = await WorldApiClient.GetSmartAssemblyPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch assembly list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var asm in page.Value.Data)
                table.AddRow(asm.Id.EscapeMarkup(), asm.Name.EscapeMarkup(), asm.SolarSystem.Name.EscapeMarkup());

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByIdAsync(Settings settings, CancellationToken cancellationToken) {
        var res = await WorldApiClient.GetSmartAssemblyById(settings.Id!.Value, cancellationToken);
        if (res.IsFailed) {
            Logger.LogError("Failed to load assembly {Id}: {Error}", settings.Id, res.ToErrorString());
            return 1;
        }

        if (res.Value == null) {
            Logger.LogError("Assembly {Id} not found", settings.Id);
            return 1;
        }

        var table = SpectreUtils.CreateAnsiTable(res.Value.Id.EscapeMarkup(), "Key", "Value");
        table.AddRow("Id", res.Value.Id.EscapeMarkup());
        table.AddRow("Type", res.Value.Name.EscapeMarkup());
        table.AddRow("SolarSystem", res.Value.SolarSystem.Name.EscapeMarkup());
        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByNameAsync(Settings settings, CancellationToken cancellationToken) {
        var pages = await LoadAllPagesAsync(WorldApiClient.GetSmartAssemblyPage, settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (pages.IsFailed) {
            Logger.LogError("Failed to load assemblies: {Error}", pages.ToErrorString());
            return 1;
        }

        var list = pages.Value;
        var exact = list.FirstOrDefault(x => string.Equals(x.Name, settings.Name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var detail = await WorldApiClient.GetSmartAssemblyById(BigInteger.Parse(exact.Id), cancellationToken);
            if (detail.IsFailed) {
                Logger.LogError("Unable to load assembly {Id}: {Error}", exact.Id, detail.ToErrorString());
                return 1;
            }

            var table = SpectreUtils.CreateAnsiTable(detail.Value.Id.EscapeMarkup(), "Key", "Value");
            table.AddRow("Id", detail.Value.Id.EscapeMarkup());
            table.AddRow("Type", detail.Value.Name.EscapeMarkup());
            table.AddRow("SolarSystem", detail.Value.SolarSystem.Name.EscapeMarkup());
            AnsiConsole.Write(table);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(list, settings.Name!, a => a.Name);
        if (!candidates.Any()) {
            Logger.LogError("No assemblies could be resolved for '{Name}'", settings.Name);
            return 1;
        }

        var best = candidates.First().Distance;
        RenderFuzzyWarning(settings.Name!, best);
        if (candidates.Count > 1) RenderMultipleMatches(candidates, a => a.Name, a => a.Id);

        var selected = candidates.First().Value;
        var final = await WorldApiClient.GetSmartAssemblyById(BigInteger.Parse(selected.Id), cancellationToken);
        if (final.IsFailed) {
            Logger.LogError("Failed to load assembly {Id}: {Error}", selected.Id, final.ToErrorString());
            return 1;
        }

        var tbl = SpectreUtils.CreateAnsiTable(final.Value.Id.EscapeMarkup(), "Key", "Value");
        tbl.AddRow("Id", final.Value.Id.EscapeMarkup());
        tbl.AddRow("Type", final.Value.Name.EscapeMarkup());
        tbl.AddRow("SolarSystem", final.Value.SolarSystem.Name.EscapeMarkup());
        AnsiConsole.Write(tbl);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--id <id>")]
        [Description("Assembly identifier")]
        public BigInteger? Id { get; set; }

        [CommandOption("--name <name>")]
        [Description("Assembly type name (fuzzy search)")]
        public string? Name { get; set; }
    }
}