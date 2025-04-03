using NSubstitute;
using Spectre.Console.Cli;

namespace FrontierSharp.Tests.CommandLine.Commands;

public static class CommandContextHelper {
    public static CommandContext Create() {
        var args = new[] { "dummy" };
        var remaining = Substitute.For<IRemainingArguments>();
        return new CommandContext(args, remaining, "dummy", null);
    }
}