using System.ComponentModel;
using FluentResults;
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
    IOptions<ConfigurationOptions> configuration) : AsyncCommand<GetTribeCommand.Settings> {
    private const int DefaultPageSize = 100;
    private readonly IAnsiConsole _ansiConsole = ansiConsole;
    private readonly ConfigurationOptions _configuration = configuration.Value;
    private readonly ILogger<GetTribeCommand> _logger = logger;
    private readonly IWorldApiClient _worldApiClient = worldApiClient;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        if (settings.ShowAll) return await ShowAllAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (settings.Id.HasValue) return await ShowByIdAsync(settings, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.Name)) return await ShowByNameAsync(settings, cancellationToken);

        _logger.LogError("You must specify --id, --name, or --show-all");
        return 1;
    }

    private async Task<int> ShowAllAsync(int pageSize, CancellationToken cancellationToken) {
        var table = SpectreUtils.CreateAnsiTable("All Tribes", "Id", "Name", "Members", "Tax", "Founded");
        var offset = 0L;

        while (true) {
            var page = await _worldApiClient.GetTribesPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) {
                _logger.LogError("Unable to fetch tribe list: {Error}", page.ToErrorString());
                return 1;
            }

            foreach (var tribe in page.Value.Data)
                table.AddRow(tribe.Id.ToString(), tribe.Name, tribe.MemberCount.ToString(),
                    tribe.TaxRate.ToString("0.00"), tribe.FoundedAt.ToAnsiString());

            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        _ansiConsole.Write(table);
        return 0;
    }

    private async Task<int> ShowByIdAsync(Settings settings, CancellationToken cancellationToken) {
        var tribeResult = await _worldApiClient.GetTribeById(settings.Id!.Value, cancellationToken);
        if (tribeResult.IsFailed) {
            _logger.LogError("Failed to load tribe {Id}: {Error}", settings.Id, tribeResult.ToErrorString());
            return 1;
        }

        var tribe = tribeResult.Value;
        if (tribe == null) {
            _logger.LogError("Tribe {Id} not found", settings.Id);
            return 1;
        }

        RenderTribeDetail(tribe, settings);
        return 0;
    }

    private async Task<int> ShowByNameAsync(Settings settings, CancellationToken cancellationToken) {
        var tribesResult = await LoadAllTribesAsync(settings.PageSize ?? DefaultPageSize, cancellationToken);
        if (tribesResult.IsFailed) {
            _logger.LogError("Failed to load tribes: {Error}", tribesResult.ToErrorString());
            return 1;
        }

        var tribes = tribesResult.Value;
        var exact = tribes.FirstOrDefault(x => string.Equals(x.Name, settings.Name, StringComparison.OrdinalIgnoreCase));
        if (exact != null) {
            var detailResult = await _worldApiClient.GetTribeById(exact.Id, cancellationToken);
            if (detailResult.IsFailed) {
                _logger.LogError("Unable to load tribe {Id}: {Error}", exact.Id, detailResult.ToErrorString());
                return 1;
            }

            RenderTribeDetail(detailResult.Value, settings);
            return 0;
        }

        var candidates = BuildFuzzyCandidates(tribes, settings.Name!);
        if (!candidates.Any()) {
            _logger.LogError("No tribes could be resolved for '{Name}'", settings.Name);
            return 1;
        }

        var bestDistance = candidates.First().Distance;
        if (bestDistance > _configuration.TribeFuzzyWarningThreshold)
            _logger.LogWarning("Closest match for '{Name}' has distance {Distance}; rerun with --id for certainty.",
                settings.Name, bestDistance);

        if (candidates.Count > 1) {
            _ansiConsole.MarkupLine("[yellow]Multiple close matches found; rerun with --id to be precise:[/]");
            foreach (var candidate in candidates)
                _ansiConsole.MarkupLine($"  • {candidate.Tribe.Name} (id {candidate.Tribe.Id}, dist {candidate.Distance})");
        }

        var selected = candidates.First().Tribe;
        var detail = await _worldApiClient.GetTribeById(selected.Id, cancellationToken);
        if (detail.IsFailed) {
            _logger.LogError("Failed to load tribe {Id}: {Error}", selected.Id, detail.ToErrorString());
            return 1;
        }

        RenderTribeDetail(detail.Value, settings);
        return 0;
    }

    private async Task<Result<List<Tribe>>> LoadAllTribesAsync(int pageSize, CancellationToken cancellationToken) {
        var tribes = new List<Tribe>();
        var offset = 0L;

        while (true) {
            var page = await _worldApiClient.GetTribesPage(pageSize, offset, cancellationToken);
            if (page.IsFailed) return Result.Fail<List<Tribe>>(page.Errors);

            tribes.AddRange(page.Value.Data);
            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        return Result.Ok(tribes);
    }

    private IReadOnlyList<TribeCandidate> BuildFuzzyCandidates(IEnumerable<Tribe> tribes, string inputName) {
        var grouped = tribes.Select(tribe => new TribeCandidate {
                Tribe = tribe,
                Distance = Levenshtein.Distance(inputName, tribe.Name)
            })
            .GroupBy(candidate => candidate.Distance)
            .OrderBy(group => group.Key)
            .FirstOrDefault();

        return grouped?.OrderBy(candidate => candidate.Tribe.Name, StringComparer.OrdinalIgnoreCase).ToList()
               ?? [];
    }

    private void RenderTribeDetail(TribeDetail tribe, Settings settings) {
        var limit = settings.ShowAllMembers
            ? int.MaxValue
            : settings.MembersLimit switch { <= 0 => int.MaxValue, > 0 => settings.MembersLimit.Value, _ => _configuration.TribeMembersLimit };

        var table = SpectreUtils.CreateAnsiTable(tribe.Name, "Key", "Value");
        table.AddRow("Id", tribe.Id.ToString());
        table.AddRow("Short", tribe.NameShort);
        table.AddRow("Description", tribe.Description);
        table.AddRow("Tax", tribe.TaxRate.ToString("0.00"));
        table.AddRow("Founded", tribe.FoundedAt.ToAnsiString());

        if (!tribe.Members.Any()) {
            table.AddRow("Members", "[grey]None[/]");
        }
        else {
            var displayed = tribe.Members.Take(limit).Select(m => m.Name).ToList();
            table.AddRow("Members", string.Join(", ", displayed));

            if (limit != int.MaxValue && tribe.Members.Count() > limit)
                table.AddRow("Members", $"... showing {limit} of {tribe.Members.Count()} (use --show-all-members or higher --members-limit)");
        }

        _ansiConsole.Write(table);
    }

    public class Settings : CommandSettings {
        [CommandOption("--id <id>")]
        [Description("Tribe identifier")]
        public long? Id { get; set; }

        [CommandOption("--name <name>")]
        [Description("Tribe name (fuzzy search)")]
        public string? Name { get; set; }

        [CommandOption("--show-all")]
        [Description("Stream the list of every tribe")]
        public bool ShowAll { get; set; }

        [CommandOption("--show-all-members")]
        [Description("Ignore the member limit and show every member in detail view")]
        public bool ShowAllMembers { get; set; }

        [CommandOption("--members-limit <count>")]
        [Description("How many members to show; 0 or negative means unlimited")]
        public int? MembersLimit { get; set; }

        [CommandOption("--page-size <size>")]
        [Description("How many tribes to fetch per page when streaming or searching")]
        public int? PageSize { get; set; }

        public override ValidationResult Validate() {
            var opts = new[] { Id.HasValue, !string.IsNullOrWhiteSpace(Name), ShowAll };
            if (opts.Count(flag => flag) != 1)
                return ValidationResult.Error("Supply exactly one of --id, --name, or --show-all");

            return ValidationResult.Success();
        }
    }

    private record TribeCandidate {
        public Tribe Tribe { get; init; } = new();
        public int Distance { get; init; }
    }
}