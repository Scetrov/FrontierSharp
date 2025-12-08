using Serilog.Events;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Utils;

public static class DependencyInjectionUtils {
    public static LogEventLevel GetLogLevel(this string[] args, LogEventLevel? defaultLevel = null) {
        if (defaultLevel == null) {
#if DEBUG
            defaultLevel = LogEventLevel.Debug;
#else
            defaultLevel = LogEventLevel.Warning;
#endif
        }

        const string flag = "--loglevel";
        if (!args.Contains(flag)) return defaultLevel.Value;
        var logLevel = args[Array.IndexOf(args, flag) + 1];
        if (Enum.TryParse(typeof(LogEventLevel), logLevel, true, out var derivedLogLevel))
            return (LogEventLevel)derivedLogLevel;

        return defaultLevel.Value;
    }

    public static IConfigurator ConfigureExceptions(this IConfigurator config) {
        config.SetExceptionHandler((ex, _) => {
            if (ex is CommandRuntimeException commandEx) {
                AnsiConsole.MarkupLine($"[red]Error:[/] {commandEx.Message.EscapeMarkup()}");
                return -1;
            }

            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        });

#if DEBUG
        return config.PropagateExceptions();
#else
        return config;
#endif
    }
}