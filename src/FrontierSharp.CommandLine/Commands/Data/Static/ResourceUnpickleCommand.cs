using System.ComponentModel;
using System.IO.Abstractions;
using System.Text.Json;
using Dumpify;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Logging;
using Razorvine.Pickle;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands.Data.Static;

public class ResourceUnpickleCommand(
    ILogger<ResourceListCommand> logger,
    IFrontierResourceHiveFactory frontierResourcesHiveFactory,
    IFileSystem fileSystem,
    IAnsiConsole ansiConsole) : AsyncCommand<ResourceUnpickleCommand.Settings> {
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var frontierResourcesHive = frontierResourcesHiveFactory.Create(settings.Root);
        var index = frontierResourcesHive.GetIndex().Files;
        var resIndex = frontierResourcesHive.GetResIndex().Files;
        var results = index.Concat(resIndex).Where(Predicate).ToArray();

        switch (results.Length) {
            case 0:
                logger.LogWarning("No files named '{Filename}' found", settings.Filename);
                ansiConsole.MarkupLine($"[red]No files found named '{settings.Filename}'[/]");
                return Task.FromResult(1);
            case > 2: {
                logger.LogError("More than 2 files matched the specified filters");
                ansiConsole.MarkupLine("[red]More than 2 files matched the specified filters[/]");
                var table = SpectreUtils.CreateAnsiTable("Resource Files", "Filename", "Relative Path");
                foreach (var file in results) table.AddRow(file.Filename.EscapeMarkup(), file.RelativePath.EscapeMarkup());

                ansiConsole.Write(table);
                return Task.FromResult(1);
            }
        }

        var path = frontierResourcesHive.ResolvePath(results[0].RelativePath);
        var content = Unpickle(path, fileSystem);

        var tableConfig = new TableConfig {
            MaxCollectionCount = settings.MaxItems
        };

        if (settings.Output == null) {
            content.Dump(tableConfig: tableConfig);
            return Task.FromResult(0);
        }

        switch (settings.OutputFormat) {
            case Settings.OutputFormatOption.Json:
                var json = JsonSerializer.Serialize(content, new JsonSerializerOptions {
                    WriteIndented = true
                });
                fileSystem.File.WriteAllText(settings.Output, json);
                break;
            case Settings.OutputFormatOption.Yaml:
                var yaml = new Serializer().Serialize(content);
                fileSystem.File.WriteAllText(settings.Output, yaml);
                break;
            default:
                throw new NotImplementedException(settings.OutputFormat.ToString());
        }

        return Task.FromResult(0);

        bool Predicate(ResFile x) {
            var filenamePredicate = settings.Filename == null || x.Filename.Equals(settings.Filename.Trim(), StringComparison.OrdinalIgnoreCase);
            var pathPredicate = settings.RelativePath == null || x.RelativePath.Equals(settings.RelativePath.Trim(), StringComparison.OrdinalIgnoreCase);
            return filenamePredicate && pathPredicate;
        }
    }

    private static object Unpickle(string path, IFileSystem fileSystem) {
        using var unpiclker = new Unpickler();
        using var fileStream = fileSystem.File.OpenRead(path);
        return unpiclker.load(fileStream);
    }

    public class Settings : BaseStaticDataCommandSettings {
        public enum OutputFormatOption {
            Json,
            Yaml
        }

        [CommandOption("--filename <filename>")]
        [Description("Case-insensitive filename to unpickle")]
        public required string? Filename { get; set; }

        [CommandOption("--relativePath <relativePath>")]
        [Description("Case-insensitive relative path to unpickle")]
        public required string? RelativePath { get; set; }

        [CommandOption("--maxItems <maxItems>")]
        public int MaxItems { get; set; } = 5;

        [CommandOption("--output <output>")]
        public string? Output { get; set; } = null;

        [CommandOption("--outputFormat <outputFormat>")]
        public OutputFormatOption OutputFormat { get; set; } = OutputFormatOption.Json;
    }
}