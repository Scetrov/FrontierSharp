using Spectre.Console.Cli;

namespace FrontierSharp.Tests.CommandLine.Commands;

public static class CommandExecutionHelper {
    public static Task<int> ExecuteAsync<TSettings>(ICommand<TSettings> command, TSettings settings,
        CancellationToken cancellationToken = default) where TSettings : CommandSettings {
        return command.ExecuteAsync(CommandContextHelper.Create(), settings, cancellationToken);
    }
}