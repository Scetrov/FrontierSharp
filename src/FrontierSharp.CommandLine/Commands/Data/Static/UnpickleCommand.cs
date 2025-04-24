using System.Collections;
using System.ComponentModel;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Logging;
using Razorvine.Pickle;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace FrontierSharp.CommandLine.Commands.Data.Static;

public class ResourceUnpickleCommand(
    ILogger<ResourceListCommand> logger,
    IFrontierResourceHiveFactory frontierResourcesHiveFactory,
    IAnsiConsole ansiConsole) : AsyncCommand<ResourceUnpickleCommand.Settings> {
    private readonly static string[] Colors = {
        "lime",
        "yellow",
        "fuchsia",
        "aqua"
    };

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
        var content = Unpickle(path);

        var tree = new Tree($"Unpickled content from [green]{results[0].Filename}[/]");

        foreach (var key in content.Keys) {
            var node = tree.AddNode($"[aqua]{key.ToString().EscapeMarkup()}[/]");
            switch (content[key]) {
                case Hashtable hashtable:
                    ConstructNode(hashtable, node, settings.MaxItems, 0);
                    continue;
                case ArrayList arrayList:
                    AddArrayList(node, arrayList, "white", key, settings.MaxItems, 0);
                    continue;
                default:
                    AddLeaf(node, key, content[key], "white");
                    break;
            }
        }

        ansiConsole.Write(tree);

        return Task.FromResult(0);

        bool Predicate(ResFile x) {
            var filenamePredicate = settings.Filename == null || x.Filename.Equals(settings.Filename.Trim(), StringComparison.OrdinalIgnoreCase);
            var pathPredicate = settings.RelativePath == null || x.RelativePath.Equals(settings.RelativePath.Trim(), StringComparison.OrdinalIgnoreCase);
            return filenamePredicate && pathPredicate;
        }
    }

    private static void ConstructNode(Hashtable content, TreeNode node, int maxItems, int depth) {
        var indexer = 0;
        var color = Colors[depth % Colors.Length];
        foreach (var key in content.Keys) {
            if (indexer++ > maxItems) {
                node.AddNode($"... {content.Count - maxItems:N0} more items".EscapeMarkup());
                break;
            }

            var valueObject = content[key];

            switch (valueObject) {
                case Hashtable hashtable: {
                    var childNode = node.AddNode($"[{color}]{key.ToString().EscapeMarkup()}[/]");
                    ConstructNode(hashtable, childNode, maxItems, depth + 1);
                    continue;
                }
                case ArrayList arrayList:
                    AddArrayList(node, arrayList, color, key, maxItems, depth + 1);
                    continue;
                default:
                    AddLeaf(node, key, valueObject, color);
                    break;
            }
        }
    }

    private static void AddArrayList(TreeNode node, ArrayList arrayList, string color, object key, int maxItems, int depth) {
        var array = arrayList.Count > maxItems ? arrayList.GetRange(0, maxItems).ToArray() : arrayList.ToArray();
        if (array.Length == 0) {
            node.AddNode($"[{color}]{key.ToString().EscapeMarkup()}:[/] [[ ]]");
            return;
        }

        if (array.Any(x => x?.GetType() == typeof(Hashtable))) {
            var indexer = 0;
            foreach (var item in array) {
                var childNode = node.AddNode($"[{color}]{(indexer++).ToString().EscapeMarkup()}:[/]");

                if (item is Hashtable childHashtable) {
                    ConstructNode(childHashtable, childNode, maxItems, depth + 1);
                    continue;
                }

                AddLeaf(node, key, item, color);
            }

            if (arrayList.Count > array.Length) node.AddNode($"... {arrayList.Count - maxItems:N0} more items".EscapeMarkup());

            return;
        }

        var arrayContinuance = arrayList.Count > maxItems ? ", ..." : string.Empty;
        var arrayString = $"[ {string.Join(", ", array)} ]".EscapeMarkup();
        var typeString = array.Any(x => x?.GetType() == array[0]?.GetType()) ? array[0]?.GetType().Name : "Mixed";
        node.AddNode($"[{color}]{key.ToString().EscapeMarkup()}:[/] {arrayString}{arrayContinuance} ({typeString}[[]])");
    }

    private static void AddLeaf(TreeNode node, object key, object? valueObject, string color) {
        var keyString = key.ToString().EscapeMarkup();
        var value = valueObject?.ToString().EscapeMarkup();
        var valueString = valueObject == null ? "null" : $"{value} ({valueObject?.GetType().Name})";
        node.AddNode($"[{color}]{keyString}[/]: {valueString}");
    }

    private static Hashtable Unpickle(string path) {
        using var unpiclker = new Unpickler();
        using var fileStream = File.OpenRead(path);
        return (Hashtable)unpiclker.load(fileStream);
    }

    public class Settings : BaseStaticDataCommandSettings {
        [CommandOption("--filename <filename>")]
        [Description("Case-insensitive filename to unpickle")]
        public required string? Filename { get; set; }

        [CommandOption("--relativePath <relativePath>")]
        [Description("Case-insensitive relative path to unpickle")]
        public required string? RelativePath { get; set; }

        [CommandOption("--maxItems <maxItems>")]
        public int MaxItems { get; set; } = 5;
    }
}