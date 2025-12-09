using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class BaseWorldApiSettings : CommandSettings {
    [CommandOption("--page-size <size>")]
    [Description("How many items to fetch per page when streaming or searching")]
    public int? PageSize { get; set; }

    [CommandOption("--show-all")]
    [Description("Stream the list of every item")]
    public bool ShowAll { get; set; }

    /// <summary>
    ///     Validate basic mutual-exclusivity for derived classes by passing flags indicating which options are present.
    ///     Derived classes should call this from their override of Validate().
    /// </summary>
    public ValidationResult ValidateExclusive(params bool[] flags) {
        if (flags.Count(flag => flag) != 1)
            return ValidationResult.Error("Supply exactly one of the mutually exclusive options");

        return ValidationResult.Success();
    }
}