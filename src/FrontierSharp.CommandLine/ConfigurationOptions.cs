namespace FrontierSharp.CommandLine;

public class ConfigurationOptions {
    public Uri BaseUri { get; init; } = new("https://api.frontierdevtools.com/");
}