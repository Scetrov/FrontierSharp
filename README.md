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
wdotnet add package FrontierSharp.WorldApi
```

## Usage

### API Client

FrontierShip is Dependency Injection ready, so all you need to do to add it to an existing project is install the NuGet
packages and configure the World API client, for example:

```csharp
services.AddHttpClient();
services.AddFusionCache().AsHybridCache();
services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(WorldApiClient))
    .Configure<FrontierSharpHttpClientOptions>(options => {
        options.BaseUri = "https://api.frontierdevtools.com/";
        options.HttpClientName = "WorldApi";
    });
services.AddSingleton<IWorldApiClient, WorldApiClient>();
```

### Command-Line Tool

FrontierSharp comes with a command-line tool that can be used to interact with the EVE Frontier services. The tool is
available from the Releases page.

#### Tribe Command

The CLI exposes a single `tribe` command (aliased as `t`, `corporation`, and `corp`) backed entirely by the WorldApi `/v2/tribes` endpoints. It supports three mutually exclusive modes:

- `--id <tribeId>`: show detailed tribe information, including members (capable of limiting output with `--members-limit` or removing the cap via `--show-all-members`).
- `--name <name>`: look up a tribe by name; the command first tries an exact match, then falls back to Levenshtein-based fuzzy search, warns when the match distance exceeds 3, and lists tie candidates with their IDs so you can rerun with `--id` for precision.
- `--show-all`: stream every tribe in pages of 100 entries (or a user-provided `--page-size`) to the console.

## Configuration

The project uses the standard .NET configuration system. You can configure the API client by adding the following
configuration to your `appsettings.json` file:

```json
{
  "FrontierSharp": {
    "BaseUri": "https://api.frontierdevtools.com/",
    "HttpClientName": "WorldApi",
    "TribeMembersLimit": 25,
    "TribeFuzzyWarningThreshold": 3
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
