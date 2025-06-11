using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands.Data.Static;

[SuppressMessage("System.IO.Abstractions", "IO0003:Replace Directory class with IFileSystem.Directory for improved testability")]
public class BaseStaticDataCommandSettings : CommandSettings {
    [CommandOption("--root <root>")] public required string Root { get; set; } = @"C:\CCP\EVE Frontier";

    public override ValidationResult Validate() {
        if (!Directory.Exists(Root))
            return ValidationResult.Error(
                $"Directory '{Root}' does not exist, specify the path to the EVE Frontier folder with the --root option.");

        return ValidationResult.Success();
    }
}