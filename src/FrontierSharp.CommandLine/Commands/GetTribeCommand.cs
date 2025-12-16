using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class GetTribeCommand(
    ILogger<GetTribeCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration)
    : BaseWorldApiCommand<GetTribeCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    private const int DefaultPageSize = 100;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (settings.Id.HasValue) return await ShowByIdAsync(settings, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name)) return await ShowByNameAsync(settings, cancellationToken);

        Logger.LogError("You must specify --id, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("All Tribes", "Id", "Name", "Members", "Tax", "Founded");
        var offset = 0L;

        while (true) {
            var page = await WorldApiClient.GetTribesPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                Logger.LogError("Unable to fetch tribe list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var tribe in page.Value.Data)
                table.AddRow(tribe.Id.ToString(), tribe.Name.EscapeMarkup(), tribe.MemberCount.ToString(),
                    tribe.TaxRate.ToString("0.00"), tribe.FoundedAt.ToAnsiString());

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByIdAsync(Settings settings, CancellationToken cancellationToken) {
        var tribeResult = await WorldApiClient.GetTribeById(settings.Id!.Value, cancellationToken);
        if (tribeResult.IsFailed) {
            Logger.LogError("Failed to load tribe {Id}: {Error}", settings.Id, tribeResult.ToErrorString());
            return 1;
        }

        var tribe = tribeResult.Value;
        if (tribe == null) {
            Logger.LogError("Tribe {Id} not found", settings.Id);
            return 1;
        }

        RenderTribeDetail(tribe, settings);
        return 0;
    }

    private async Task<int> ShowByNameAsync(Settings settings, CancellationToken cancellationToken) {
        var tribesResult = await LoadAllPagesAsync(WorldApiClient.GetTribesPage, settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (tribesResult.IsFailed) {
            Logger.LogError("Failed to load tribes: {Error}", tribesResult.ToErrorString());
            return 1;
        }

        var tribes = tribesResult.Value;
        var exact = tribes.FirstOrDefault(x => string.Equals(x.Name, settings.Name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var detailResult = await WorldApiClient.GetTribeById(exact.Id, cancellationToken);
            if (detailResult.IsFailed) {
                Logger.LogError("Unable to load tribe {Id}: {Error}", exact.Id, detailResult.ToErrorString());
                return 1;
            }

            RenderTribeDetail(detailResult.Value, settings);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(tribes, settings.Name!, t => t.Name);
        if (!candidates.Any()) {
            Logger.LogError("No tribes could be resolved for '{Name}'", settings.Name);
            return 1;
        }

        var bestDistance = candidates.First().Distance;
        RenderFuzzyWarning(settings.Name!, bestDistance);

        if (candidates.Count > 1) RenderMultipleMatches(candidates, t => t.Name, t => t.Id.ToString());

        var selected = candidates.First().Value;
        var detail = await WorldApiClient.GetTribeById(selected.Id, cancellationToken);
        if (detail.IsFailed) {
            Logger.LogError("Failed to load tribe {Id}: {Error}", selected.Id, detail.ToErrorString());
            return 1;
        }

        RenderTribeDetail(detail.Value, settings);
        return 0;
    }

    private void RenderTribeDetail(TribeDetail tribe, Settings settings) {
        var limit = settings.ShowAllMembers
            ? int.MaxValue
            : settings.MembersLimit switch { <= 0 => int.MaxValue, > 0 => settings.MembersLimit.Value, _ => Configuration.TribeMembersLimit };

        var table = SpectreUtils.CreateAnsiTable(tribe.Name.EscapeMarkup(), "Key", "Value");
        table.AddRow("Id", tribe.Id.ToString());
        table.AddRow("Short", tribe.NameShort.EscapeMarkup());
        table.AddRow("Description", tribe.Description.EscapeMarkup());
        table.AddRow("Tax", tribe.TaxRate.ToString("0.00"));
        table.AddRow("Founded", tribe.FoundedAt.ToAnsiString());

        if (!tribe.Members.Any()) {
            table.AddRow("Members", "[grey]None[/]");
        } else {
            var displayed = tribe.Members.Take(limit).Select(m => m.Name.EscapeMarkup()).ToList();
            table.AddRow("Members", string.Join(", ", displayed));

            if (limit != int.MaxValue && tribe.Members.Count() > limit)
                table.AddRow("Members", $"... showing {limit} of {tribe.Members.Count()} (use --show-all-members or higher --members-limit)");
        }

        AnsiConsole.Write(table);
    }

    public class Settings : BaseWorldApiSettings {
        [CommandOption("--id <id>")]
        [Description("Tribe identifier")]
        public long? Id { get; set; }

        [CommandOption("--name <name>")]
        [Description("Tribe name (fuzzy search)")]
        public string? Name { get; set; }

        [CommandOption("--show-all-members")]
        [Description("Ignore the member limit and show every member in detail view")]
        public bool ShowAllMembers { get; set; }

        [CommandOption("--members-limit <count>")]
        [Description("How many members to show; 0 or negative means unlimited")]
        public int? MembersLimit { get; set; }

        public override ValidationResult Validate() {
            // Validate the base mutually exclusive options: --id, --name, --show-all
            return ValidateExclusive(Id.HasValue, !string.IsNullOrWhiteSpace(Name), ShowAll);
        }
    }
}