using FluentResults;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public abstract class BaseWorldApiCommand<TSettings> : AsyncCommand<TSettings> where TSettings : BaseWorldApiSettings {
    protected readonly IAnsiConsole AnsiConsole;
    protected readonly ConfigurationOptions Configuration;
    protected readonly ILogger Logger;
    protected readonly IWorldApiClient WorldApiClient;

    protected BaseWorldApiCommand(ILogger logger, IWorldApiClient worldApiClient, IAnsiConsole ansiConsole,
        IOptions<ConfigurationOptions> configuration) {
        Logger = logger;
        WorldApiClient = worldApiClient;
        AnsiConsole = ansiConsole;
        Configuration = configuration.Value;
    }

    // Generic pagination helper that aggregates pages returned by a pageFunction(limit, offset, ct)
    protected async Task<Result<List<T>>> LoadAllPagesAsync<T>(Func<long, long, CancellationToken, Task<Result<WorldApiPayload<T>>>> pageFunc,
        long pageSize, CancellationToken cancellationToken) {
        var items = new List<T>();
        var offset = 0L;

        while (true) {
            var page = await pageFunc(pageSize, offset, cancellationToken);
            if (page.IsFailed) return Result.Fail<List<T>>(page.Errors);

            items.AddRange(page.Value.Data);
            offset += page.Value.Data.LongCount();
            if (offset >= page.Value.Metadata.Total) break;
        }

        return Result.Ok(items);
    }

    // Build fuzzy candidates based on Levenshtein distance; returns candidates ordered by distance then name
    protected IReadOnlyList<Candidate<T>> BuildFuzzyCandidates<T>(IEnumerable<T> items, string inputName, Func<T, string> nameSelector) {
        var candidates = items.Select(item => new Candidate<T>(item, Levenshtein.Distance(inputName, nameSelector(item))))
            .GroupBy(c => c.Distance)
            .OrderBy(g => g.Key)
            .FirstOrDefault()
            ?.OrderBy(c => nameSelector(c.Value), StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<Candidate<T>>();

        return candidates;
    }

    protected void RenderFuzzyWarning(string name, int distance) {
        if (distance > Configuration.FuzzyWarningThreshold)
            Logger.LogWarning("Closest match for '{Name}' has distance {Distance}; rerun with --id for certainty.", name, distance);
    }

    protected void RenderMultipleMatches<T>(IEnumerable<Candidate<T>> candidates, Func<T, string> nameSelector, Func<T, string> idSelector) {
        AnsiConsole.MarkupLine("[yellow]Multiple close matches found; rerun with --id to be precise:[/]");
        foreach (var candidate in candidates)
            AnsiConsole.MarkupLine($"  • {nameSelector(candidate.Value)} (id {idSelector(candidate.Value)}, dist {candidate.Distance})");
    }

    protected record Candidate<T>(T Value, int Distance);
}