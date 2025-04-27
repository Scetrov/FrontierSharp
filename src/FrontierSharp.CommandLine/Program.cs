using System.IO.Abstractions;
using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.CommandLine.Commands.Data.Static;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Data.Static;
using FrontierSharp.FrontierDevTools.Api;
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
        services.AddSingleton<IFrontierResourceHiveFactory, FrontierResourceHiveFactory>();
        services.AddSingleton<IFileSystem, FileSystem>();

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config => {
            config.AddCommand<GetCharacterCommand>("rider").WithAlias("r").WithAlias("character").WithAlias("char");
            config.AddCommand<GetCorporationCommand>("tribe").WithAlias("t").WithAlias("corporation").WithAlias("corp");
            config.AddCommand<GetGateNetworkCommand>("gates").WithAlias("g");
            config.AddCommand<OptimizeStargateNetworkPlacementCommand>("optimize-placement").WithAlias("o")
                .WithAlias("op");
            config.AddCommand<FindTravelRouteCommand>("route").WithAlias("fr").WithAlias("rt");
            config.AddCommand<CalculateDistanceCommand>("distance").WithAlias("d");
            config.AddCommand<FindSystemsWithinDistanceCommand>("systems-within-distance").WithAlias("sd");
            config.AddCommand<FindCommonSystemsWithinDistanceRequestCommand>("common-systems").WithAlias("common")
                .WithAlias("cs");
            config.AddCommand<CalculateTravelDistanceCommand>("calculate-travel-distance")
                .WithAlias("calc-travel-distance").WithAlias("td");
            config.AddCommand<CalculateFuelRequiredCommand>("calculate-fuel").WithAlias("calc-fuel").WithAlias("cf");
            config.AddCommand<CalculateFuelPerLightyearCommand>("calculate-fuel-per-ly").WithAlias("calc-fuel-ly")
                .WithAlias("cfl");
            config.AddCommand<OptimalStargateNetworkAndDeploymentCommand>("optimal-stargate-network").WithAlias("osn")
                .WithAlias("plan");

            config.ConfigureExceptions();

            config.AddBranch("data", data => {
                data.SetDescription("Commands for data management");
                data.AddBranch("static", staticData => {
                    staticData.SetDescription("Commands for static data");
                    staticData.AddBranch("resources",
                        resources => {
                            ;
                            resources.AddCommand<ResourceListCommand>("list").WithAlias("l").WithAlias("ls");
                            resources.AddCommand<ResourceUnpickleCommand>("unpickle").WithAlias("u").WithAlias("unp");
                        }).WithAlias("res").WithAlias("r");
                });
            });
        });
        services.AddSingleton<ICommandApp>(app);
    })
    .Build();

var spectreApp = host.Services.GetRequiredService<ICommandApp>();
await Task.Run(() => spectreApp.RunAsync(args));