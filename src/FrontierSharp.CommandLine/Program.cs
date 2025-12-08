using System.IO.Abstractions;
using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
using FrontierSharp.CommandLine.Commands.Data.Static;
using FrontierSharp.CommandLine.Utils;
using FrontierSharp.Data.Static;
using FrontierSharp.HttpClient;
using FrontierSharp.WorldApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using ZiggyCreatures.Caching.Fusion;

var host = Host.CreateDefaultBuilder(args)
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
        services.AddHttpClient("WorldApi", config => config.Timeout = TimeSpan.FromMinutes(5));
        services.AddFusionCache().AsHybridCache();
        services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(WorldApiClient))
            .Configure<FrontierSharpHttpClientOptions>(options => {
                options.HttpClientName = "WorldApi";
            });
        services.AddSingleton<IWorldApiClient, WorldApiClient>();
        services.AddSingleton<IFrontierResourceHiveFactory, FrontierResourceHiveFactory>();
        services.AddSingleton<IFileSystem, FileSystem>();

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config => {
            config.AddCommand<GetTribeCommand>("tribe").WithAlias("t").WithAlias("corporation").WithAlias("corp");

            config.ConfigureExceptions();

            config.AddBranch("data", data => {
                data.SetDescription("Commands for data management");
                data.AddBranch("static", staticData => {
                    staticData.SetDescription("Commands for static data");
                    staticData.AddBranch("resources",
                        resources => {
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