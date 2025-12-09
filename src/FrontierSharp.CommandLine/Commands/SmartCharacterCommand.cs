using System.ComponentModel;
using System.Text;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class SmartCharacterCommand(
    ILogger<SmartCharacterCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration)
    : BaseWorldApiCommand<SmartCharacterCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 100;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (!string.IsNullOrWhiteSpace(settings.Address)) return await ShowByAddressAsync(settings, cancellationToken);
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, settings, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name)) return await ShowByNameAsync(settings, cancellationToken);

        Logger.LogError("You must specify --address, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowByAddressAsync(Settings settings, CancellationToken cancellationToken) {
        var res = await WorldApiClient.GetSmartCharacterById(settings.Address!, cancellationToken);
        if (res.IsFailed) {
            Logger.LogError("Failed to load character {Address}: {Error}", settings.Address, res.ToErrorString());
            return 1;
        }

        if (res.Value == null) {
            Logger.LogError("Character {Address} not found", settings.Address);
            return 1;
        }

        var detail = res.Value;

        var table = SpectreUtils.CreateAnsiTable(detail.Name.EscapeMarkup(), "Key", "Value");
        table.AddRow("Address", detail.Address.EscapeMarkup());
        table.AddRow("Name", detail.Name.EscapeMarkup());
        table.AddRow("Portrait", detail.PortraitUrl.EscapeMarkup());
        table.AddRow("TribeId", detail.TribeId.ToString());
        table.AddRow("EVE Balance", detail.EveBalanceInWei.AsWeiToEther());
        table.AddRow("Gas Balance", detail.GasBalanceInWei.AsWeiToEther());
        var assemblies = detail.SmartAssemblies.ToList();
        table.AddRow("Assemblies", assemblies.Count.ToString());
        if (assemblies.Count != 0) {
            var assemblyDisplay = assemblies
                .Where(x => x.Type != SmartAssemblyType.Unknown)
                .Take(25)
                .Select(FormatAssembly);
            table.AddRow("Assembly Types", string.Join(Environment.NewLine, assemblyDisplay));
        }
        AnsiConsole.Write(table);
        return 0;
    }

    private static string FormatAssembly(SmartAssembly a) {
        var builder = new StringBuilder();
        builder.Append(string.IsNullOrWhiteSpace(a.Name) ? a.Type.ToString() : a.Name);
        builder.Append(' ');
        builder.Append($"({a.State})");
        return builder.ToString();
    }

    private async Task<int> ShowAllAsync(int pageSize, Settings settings, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("Characters", "Address", "Name");

        // If a specific page is requested, fetch just that page (1-based page number)
        if (settings.PageNumber.HasValue) {
            var pageIndex = Math.Max(1, settings.PageNumber.Value) - 1;
            var pageOffset = pageIndex * (long)pageSize;
            var page = await WorldApiClient.GetSmartCharacterPage(pageSize, pageOffset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch character list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var ch in page.Value.Data)
                table.AddRow(ch.Address.EscapeMarkup(), ch.Name.EscapeMarkup());

            AnsiConsole.Write(table);
            return 0;
        }

        // Otherwise stream all pages
        var offset = 0L;
        while (true) {
            var page = await WorldApiClient.GetSmartCharacterPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch character list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var ch in page.Value.Data)
                table.AddRow(ch.Address.EscapeMarkup(), ch.Name.EscapeMarkup());

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByNameAsync(Settings settings, CancellationToken cancellationToken) {
        var pages = await LoadAllPagesAsync(WorldApiClient.GetSmartCharacterPage, settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (pages.IsFailed) {
            Logger.LogError("Failed to load characters: {Error}", pages.ToErrorString());
            return 1;
        }

        var list = pages.Value;
        var exact = list.FirstOrDefault(x => string.Equals(x.Name, settings.Name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var detailRes = await WorldApiClient.GetSmartCharacterById(exact.Address, cancellationToken);
            if (detailRes.IsFailed) {
                Logger.LogError("Unable to load character {Address}: {Error}", exact.Address, detailRes.ToErrorString());
                return 1;
            }

            var detail = detailRes.Value;
            var table = SpectreUtils.CreateAnsiTable(detail.Name.EscapeMarkup(), "Key", "Value");
            table.AddRow("Address", detail.Address.EscapeMarkup());
            table.AddRow("Name", detail.Name.EscapeMarkup());
            table.AddRow("Portrait", detail.PortraitUrl.EscapeMarkup());
            table.AddRow("TribeId", detail.TribeId.ToString());
            table.AddRow("EVE Balance", detail.EveBalanceInWei.AsWeiToEther());
            table.AddRow("Gas Balance", detail.GasBalanceInWei.AsWeiToEther());
            var assemblies = detail.SmartAssemblies.ToList();
            table.AddRow("Assemblies", assemblies.Count.ToString());
            if (assemblies.Count != 0) {
                var assemblyDisplay = assemblies
                    .Where(x => x.Type != SmartAssemblyType.Unknown)
                    .Take(25)
                    .Select(FormatAssembly);
                table.AddRow("Assembly Types", string.Join(Environment.NewLine, assemblyDisplay));
            }
            AnsiConsole.Write(table);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(list, settings.Name!, c => c.Name);
        if (!candidates.Any()) {
            Logger.LogError("No characters could be resolved for '{Name}'", settings.Name);
            return 1;
        }

        var best = candidates.First().Distance;
        RenderFuzzyWarning(settings.Name!, best);
        if (candidates.Count > 1) RenderMultipleMatches(candidates, sc => sc.Name, sc => sc.Address);

        var selected = candidates.First().Value;
        var finalRes = await WorldApiClient.GetSmartCharacterById(selected.Address, cancellationToken);
        if (finalRes.IsFailed) {
            Logger.LogError("Failed to load character {Address}: {Error}", selected.Address, finalRes.ToErrorString());
            return 1;
        }

        var final = finalRes.Value;
        var tbl = SpectreUtils.CreateAnsiTable(final.Name, "Key", "Value");
        tbl.AddRow("Address", final.Address);
        tbl.AddRow("Name", final.Name);
        tbl.AddRow("Portrait", final.PortraitUrl);
        tbl.AddRow("TribeId", final.TribeId.ToString());
        tbl.AddRow("EVE Balance", final.EveBalanceInWei.AsWeiToEther());
        tbl.AddRow("Gas Balance", final.GasBalanceInWei.AsWeiToEther());
        var finalAssemblies = final.SmartAssemblies.ToList();
        tbl.AddRow("Assemblies", finalAssemblies.Count.ToString());
        if (finalAssemblies.Any()) tbl.AddRow("Assembly Names", string.Join(", ", finalAssemblies.Take(8).Select(a => a.Name)));
        AnsiConsole.Write(tbl);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--address <address>")]
        [Description("Character address (exact)")]
        public string? Address { get; set; }

        [CommandOption("--name <name>")]
        [Description("Character name (fuzzy search)")]
        public string? Name { get; set; }

        [CommandOption("--page <n>")]
        [Description("When used with --show-all, fetch this 1-based page number instead of streaming all pages")]
        public int? PageNumber { get; set; }
    }
}