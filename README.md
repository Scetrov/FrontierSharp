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
dotnet add package FrontierSharp.WorldApi
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
    options.BaseUri = "https://blockchain-gateway-stillness.live.tech.evefrontier.com";
    options.HttpClientName = nameof(WorldApiClient);
  });
services.AddSingleton<IWorldApiClient, WorldApiClient>();
```

### Command-Line Tool

FrontierSharp comes with a command-line tool that can be used to interact with the EVE Frontier services. The tool is
available from the Releases page and is documented in the [FrontierSharp.CommandLine README.md](./src/FrontierSharp.CommandLine/README.md).

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

## Complete Example

Here is a complete example of how to use the FrontierSharp API client in a .NET application:

```csharp
var services = new ServiceCollection();

services.AddHttpClient();
services.AddFusionCache().AsHybridCache();
services.AddKeyedSingleton<IFrontierSharpHttpClient, FrontierSharpHttpClient>(nameof(WorldApiClient))
  .Configure<FrontierSharpHttpClientOptions>(options => {
    options.BaseUri = "https://blockchain-gateway-stillness.live.tech.evefrontier.com";
    options.HttpClientName = nameof(WorldApiClient);
  });
services.AddSingleton<IWorldApiClient, WorldApiClient>();

var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IWorldApiClient>();
var result = await client.GetTribesPage();

if (result.IsFailed) {
  Console.WriteLine("Failed with the following reasons:");
  foreach (var reason in result.Reasons) {
    Console.WriteLine($" - {reason.Message}");
  }
  return;
}

foreach (var member in result.Value.Data) {
  Console.WriteLine($"{member.Id}: {member.Name} [{member.NameShort}]");
}
```

## ASP.NET Core Integration

You can easily integrate FrontierSharp into an ASP.NET Core application by adding the necessary services in the `Startup.cs` file, then consuming the `IResult` returned by the FrontierSharp:

```csharp
[HttpGet]
public async Task<ActionResult<Tribe>> GetTribes() {
    return await client.GetTribes().ToActionResult();
}
```

## Contributing

Contributions are welcome! For bug reports, feature requests, or questions, please open an issue. For code
contributions, please create a pull request.

> [!IMPORTANT]
> If you find a security issue with the solution, I would appreciate it if you followed a responsible disclosure process
> and contacted me directly or via the GitHub Security Reporting programme.

## License

FrontierSharp is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.
