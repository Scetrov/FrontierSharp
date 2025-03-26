using FrontierSharp.CommandLine;
using FrontierSharp.CommandLine.Commands;
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
    .ConfigureAppConfiguration((context, config) => { config.AddJsonFile("config.json", true); })
    .ConfigureServices((context, services) => {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        services.AddSingleton(AnsiConsole.Console);

        services.Configure<ConfigurationOptions>(context.Configuration.GetSection("Configuration"));
        services.AddHttpClient();
        services.AddFusionCache().AsHybridCache();
        services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(FrontierDevToolsClient))
            .Configure<FrontierSharpHttpClientOptions>(options => {
                options.BaseUri = "https://api.frontierdevtools.com/";
                options.HttpClientName = "FrontierDevTools";
            });
        services.AddSingleton<IFrontierDevToolsClient, FrontierDevToolsClient>();

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config => { config.AddCommand<GetCharacterCommand>("character").WithAlias("char").WithAlias("c"); });
        services.AddSingleton<ICommandApp>(app);
    })
    .Build();

var spectreApp = host.Services.GetRequiredService<ICommandApp>();
await Task.Run(() => spectreApp.RunAsync(args));