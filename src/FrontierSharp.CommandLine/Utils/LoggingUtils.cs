using Serilog.Events;

namespace FrontierSharp.CommandLine.Utils;

public static class LoggingUtils {
    public static LogEventLevel GetLogLevel(this string[] args, LogEventLevel defaultLevel = LogEventLevel.Warning) {
        const string flag = "--loglevel";
        if (!args.Contains(flag)) return defaultLevel;
        var logLevel = args[Array.IndexOf(args, flag) + 1];
        if (Enum.TryParse(typeof(LogEventLevel), logLevel, true, out var derivedLogLevel)) {
            return (LogEventLevel)derivedLogLevel;
        }

        return defaultLevel;
    }
}