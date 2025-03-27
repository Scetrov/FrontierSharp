# FrontierSharp

FrontierSharp is a .NET library that provides access to basic Character and Starmap Information for EVE Frontier. It includes an API client and a command-line tool for interacting with the EVE Frontier services via both official and third-party services.

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

FrontierShip is Dependency Injection ready, so all you need to do to add it to an existing project is Install the NuGet packages and setup the DI container, for example:

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


