using System.Numerics;
using System.Text;
using Humanizer;
using Spectre.Console;

namespace FrontierSharp.CommandLine.Utils;

public static class SpectreUtils {
    public static Table CreateAnsiTable(string title, params string[] columns) {
        var table = TableFactory(title);

        foreach (var column in columns) {
            table.AddColumn($"[bold]{column}[/]");
        }

        return table;
    }

    public static Table CreateAnsiListing(string title, Dictionary<string, string> data) {
        var table = TableFactory(title);

        table.AddColumns("[bold]Key[/]", "[bold]Value[/]");

        foreach (var entry in data) {
            table.AddRow($"[bold]{entry.Key}[/]", entry.Value);
        }

        return table;
    }

    private static Table TableFactory(string title) {
        var table = new Table {
            Border = TableBorder.Rounded,
            Title = new TableTitle(title),
            BorderStyle = new Style(Color.Orange1)
        };
        return table;
    }

    public static string AsWeiToEther(this BigInteger wei) {
        return ((decimal)wei / 1000000000000000000m).ToAnsiString();
    }

    public static string ToAnsiString(this bool value) {
        return value ? "[green]Yes[/]" : "[red]No[/]";
    }

    public static string ToAnsiString(this bool? value) {
        if (!value.HasValue) {
            return "[grey]N/A[/]";
        }

        return value.Value ? "[green]Yes[/]" : "[red]No[/]";
    }

    public static string FuelToAnsiString(this int value) {
        var builder = new StringBuilder();

        switch (value) {
            case 0:
                return "[red]Empty[/]";
            case > 0 and < 100:
                builder.AppendFormat("[orange1]{0}[/]", value);
                break;
            case >= 100 and < 240:
                builder.AppendFormat("[yellow]{0}[/]", value);
                break;
            case >= 240:
                builder.AppendFormat("[green]{0}[/]", value);
                break;
        }

        builder.Append($" ({TimeSpan.FromHours(value).Humanize()})");

        return builder.ToString();
    }

    public static string ToAnsiString(this DateTimeOffset value) {
        return $"{value:yyyy-MM-dd} [grey]({value.Humanize()})[/]";
    }

    public static string ToAnsiString(this decimal value) {
        return value.ToString("0.00[grey]000[/]");
    }

    public static string SliceMiddle(this string value, int chars = 18) {
        return value.Length < chars * 2 ? value : $"{value[..chars]}[grey]...[/]{value[^chars..]}";
    }
}