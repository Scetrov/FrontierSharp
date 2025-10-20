# FrontierSharp

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/scetrov/frontiersharp/build-and-test.yml?style=flat-square) ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/scetrov/frontiersharp/total?style=flat-square&label=github%20release%20downloads) ![NuGet Downloads](https://img.shields.io/nuget/dt/FrontierSharp.HttpClient?style=flat-square&label=all%20nuget%20downloads) ![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/scetrov/frontiersharp?style=flat-square) ![GitHub License](https://img.shields.io/github/license/scetrov/frontiersharp?style=flat-square) ![GitHub Release](https://img.shields.io/github/v/release/scetrov/frontiersharp?style=flat-square)

FrontierSharp is a .NET library that provides access to basic Character and Starmap Information for EVE Frontier. It
includes an API client and a command-line tool for interacting with the EVE Frontier services via both official and
third-party services.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
    - [API Client](#api-client)
    - [Command-Line Tool](#command-line-tool)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Features

- Access to character information,
- Access to starmap information,
- Command-line interface for easy interaction from the prompt.

## Installation

### NuGet Packages

You can install the FrontierSharp packages via NuGet:

```sh
dotnet add package FrontierSharp.FrontierDevTools.Api
```

## Usage

### API Client

FrontierShip is Dependency Injection ready, so all you need to do to add it to an existing project is Install the NuGet
packages and setup the DI container, for example:

```csharp
services.AddHttpClient();
services.AddFusionCache().AsHybridCache();
services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(FrontierDevToolsClient))
    .Configure<FrontierSharpHttpClientOptions>(options => {
        options.BaseUri = "https://api.frontierdevtools.com/";
        options.HttpClientName = "FrontierDevTools";
    });
services.AddSingleton<IFrontierDevToolsClient, FrontierDevToolsClient>();
```

### Command-Line Tool

FrontierSharp comes with a command-line tool that can be used to interact with the EVE Frontier services. The tool is
available from the Releases page.

## Configuration

The project uses the standard .NET configuration system. You can configure the API client by adding the following
configuration to your `appsettings.json` file:

```json
{
  "FrontierSharp": {
    "BaseUri": "https://api.frontierdevtools.com/",
    "HttpClientName": "FrontierDevTools"
  }
}
```

You can then load this into the dependency injection provider with:

```csharp
services.Configure<FrontierSharpHttpClientOptions>(Configuration.GetSection("FrontierSharp"));
```

## Contributing

Contributions are welcome! For bug reports, feature requests, or questions, please open an issue. For code
contributions, please create a pull request.

> [!IMPORTANT]
> If you find a security issue with the solution, I would appreciate it if you followed a responsible disclosure process
> and contacted me directly or via the GitHub Security Reporting programme.

## License

FrontierSharp is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.
