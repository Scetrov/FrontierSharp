using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.FrontierDevTools.Api;
using FrontierSharp.FrontierDevTools.Api.RequestModels;
using FrontierSharp.HttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using ZiggyCreatures.Caching.Fusion;
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.ClearProviders())
    .ConfigureAppConfiguration((_, config) => { config.AddJsonFile("config.json", true); })
    .ConfigureServices((context, services) => {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(args.GetLogLevel())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        services.AddSingleton(AnsiConsole.Console);

        services.Configure<ConfigurationOptions>(context.Configuration.GetSection("Configuration"));
        services.AddHttpClient("FrontierDevTools", config => config.Timeout = TimeSpan.FromMinutes(5));
        services.AddFusionCache().AsHybridCache();
        services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(FrontierDevToolsClient))
            .Configure<FrontierSharpHttpClientOptions>(options => {
                options.BaseUri = "https://api.frontierdevtools.com/";
                options.HttpClientName = "FrontierDevTools";
            });
        services.AddSingleton<IFrontierDevToolsClient, FrontierDevToolsClient>();

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config => {
            config.AddCommand<GetCharacterCommand>("rider").WithAlias("r").WithAlias("character").WithAlias("char");
            config.AddCommand<GetCorporationCommand>("tribe").WithAlias("t").WithAlias("corporation").WithAlias("corp");
            config.AddCommand<GetGateNetworkCommand>("gates").WithAlias("g");
            config.AddCommand<OptimizeStargateNetworkPlacementCommand>("optimize-placement").WithAlias("o").WithAlias("op");
            config.AddCommand<FindTravelRouteCommand>("route").WithAlias("fr").WithAlias("rt");
            config.AddCommand<CalculateDistanceCommand>("distance").WithAlias("d");
            config.AddCommand<FindSystemsWithingDistanceCommand>("systems-within-distance").WithAlias("sd");
        });
        services.AddSingleton<ICommandApp>(app);
    })
    .Build();

var spectreApp = host.Services.GetRequiredService<ICommandApp>();
await Task.Run(() => spectreApp.RunAsync(args));