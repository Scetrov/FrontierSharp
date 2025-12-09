using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Common.Utils;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine.Commands;

public class ConfigCommand(
    ILogger<ConfigCommand> logger,
    IWorldApiClient worldApiClient,
    IAnsiConsole ansiConsole,
    IOptions<ConfigurationOptions> configuration) : BaseWorldApiCommand<ConfigCommand.Settings>(logger, worldApiClient, ansiConsole, configuration) {
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        var res = await WorldApiClient.GetConfig(cancellationToken);
        if (res.IsFailed) {
            Logger.LogError("Failed to fetch config: {Error}", res.ToErrorString());
            return 1;
        }

        var configs = res.Value.ToList();
        if (!configs.Any()) {
            Logger.LogInformation("No config available from World API");
            return 0;
        }

        if (settings.ShowAll) {
            var table = SpectreUtils.CreateAnsiTable("World API Configs", "ChainId", "Name", "Explorer", "Indexer");
            foreach (var c in configs) table.AddRow(c.ChainId.ToString(), c.Name, c.BlockExplorerUrl, c.IndexerUrl);
            AnsiConsole.Write(table);
            return 0;
        }

        // Show details of the first config by default (there's usually one)
        var cfg = configs.First();
        var detail = SpectreUtils.CreateAnsiTable(cfg.Name, "Key", "Value");
        detail.AddRow("ChainId", cfg.ChainId.ToString());
        detail.AddRow("Name", cfg.Name);
        detail.AddRow("BlockExplorerUrl", cfg.BlockExplorerUrl);
        detail.AddRow("IndexerUrl", cfg.IndexerUrl);
        detail.AddRow("VaultDappUrl", cfg.VaultDappUrl);
        detail.AddRow("MetadataApiUrl", cfg.MetadataApiUrl);
        detail.AddRow("IPFS API", cfg.IpfsApiUrl);
        detail.AddRow("CycleStart", cfg.CycleStartDate.ToAnsiString());
        AnsiConsole.Write(detail);
        return 0;
    }

    public class Settings : BaseWorldApiSettings {
        // reuse ShowAll from BaseWorldApiSettings
    }
}