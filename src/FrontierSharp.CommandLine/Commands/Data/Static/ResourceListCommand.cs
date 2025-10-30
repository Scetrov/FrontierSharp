using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands.Data.Static;

public class ResourceListCommand(
    ILogger<ResourceListCommand> logger,
    IFrontierResourceHiveFactory frontierResourcesHiveFactory,
    IAnsiConsole ansiConsole) : AsyncCommand<ResourceListCommand.Settings> {
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        var frontierResourcesHive = frontierResourcesHiveFactory.Create(settings.Root);
        var index = frontierResourcesHive.GetIndex().Files;
        var resIndex = frontierResourcesHive.GetResIndex().Files;
        var results = index.Concat(resIndex).Where(Predicate).ToArray();

        if (results.Length == 0) {
            logger.LogWarning("No files found for filter '{Filter}' and type '{Type}'", settings.Filter, settings.Type);
            ansiConsole.MarkupLine($"[red]No files found for filter '{settings.Filter}' and type '{settings.Type}'[/]");
            return Task.FromResult(1);
        }

        var table = SpectreUtils.CreateAnsiTable("Resource Files", "Filename", "Relative Path");

        foreach (var file in results) table.AddRow(file.Filename.EscapeMarkup(), file.RelativePath.EscapeMarkup());

        ansiConsole.Write(table);

        var filterText = string.IsNullOrWhiteSpace(settings.Filter)
            ? "[cyan]any[/]"
            : $"[magenta]'{settings.Filter}'[/]";
        var typeText = string.IsNullOrWhiteSpace(settings.Type) ? "[cyan]any[/]" : $"[magenta]'{settings.Type}'[/]";
        ansiConsole.MarkupLine(
            $"[green]Found [white]{results.Length:N0}[/] files for filter {filterText} and type {typeText}[/]");

        return Task.FromResult(0);

        bool Predicate(ResFile x) {
            var filterPredicate = string.IsNullOrWhiteSpace(settings.Filter) ||
                                  x.Filename.Contains(settings.Filter, StringComparison.OrdinalIgnoreCase);
            var typePredicate = string.IsNullOrWhiteSpace(settings.Type) ||
                                x.Filename.EndsWith(settings.Type, StringComparison.OrdinalIgnoreCase);
            return filterPredicate && typePredicate;
        }
    }

    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class Settings : BaseStaticDataCommandSettings {
        [CommandOption("--filter <filter>")]
        [Description("Case-insensitive substring matching")]
        public required string Filter { get; set; }

        [CommandOption("--type <type>")]
        [Description("Case insensitive file extension type")]
        public required string Type { get; set; }
    }
}