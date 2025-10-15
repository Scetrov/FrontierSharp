The goal: help an AI coding agent become productive quickly in the FrontierSharp repository.

Keep this concise and specific to patterns discoverable in the codebase.

- Big picture

  - FrontierSharp is a .NET 9 multi-project solution (see `src/FrontierSharp.sln`).
  - Major components:
    - `FrontierSharp.CommandLine` — CLI app (binary `frontierctl`) using Spectre.Console.Cli and DI (see `Program.cs`).
    - `FrontierSharp.HttpClient` — small HTTP wrapper providing a typed `Get<TRequest, TResponse>` that uses a named HttpClient, FusionCache and returns FluentResults (see `FrontierSharpHttpClient.cs`).
    - `FrontierSharp.FrontierDevTools.Api` and `FrontierSharp.WorldApi` — high-level API clients that compose the HttpClient requests (see `FrontierDevToolsClient.cs`, `WorldApiClient.cs`).
    - `FrontierSharp.Tests` — xUnit tests that embed large JSON fixtures for WorldApi payloads (see `FrontierSharp.Tests.csproj` and `WorldApi/payloads`).

- How to make small, safe changes

  - Prefer editing library projects (`.csproj`) or adding tests under `src/FrontierSharp.Tests`.
  - Preserve APIs: public interfaces in `IFrontierSharpHttpClient`, `IFrontierDevToolsClient`, and `IWorldApiClient` are used cross-project — update implementations and tests together.

- Important patterns and conventions

  - Configuration: options objects are bound via `services.Configure<...>(Configuration.GetSection(...))` and the default BaseUri + a named HttpClient "FrontierDevTools" are set in `Program.cs`.
  - Caching: HTTP GET results are cached using `ZiggyCreatures.FusionCache` (HybridCache). Use the request model's `GetCacheKey()` to control cache keys.
  - Errors: Methods return FluentResults (`Result.Ok`/`Result.Fail`). Follow existing error patterns (propagate errors instead of throwing where current code uses FluentResults).
  - Serialization: JSON is (de)serialized with System.Text.Json; failure paths log the raw content (see `FrontierSharpHttpClient`).
  - CLI: Commands are registered in `Program.cs` via `config.AddCommand<...>("name")` and often have multiple aliases (e.g., `rider`, `r`). Look there to add or find commands.

- Build / test / CI

  - The project targets net9.0. CI uses .NET 9 and the usual dotnet commands in `/.github/workflows/build-and-test.yml`:
    - dotnet restore (run in `./src`), dotnet build --configuration Release, dotnet test --configuration Release
    - Publishing: `dotnet publish -c Release -r <rid> --self-contained` for `FrontierSharp.CommandLine`.
  - Developer shortest path to iterate locally:
    - From repo root: `cd src` then `dotnet restore` then `dotnet build` then `dotnet test`.

- Integration points & external dependencies

  - External APIs: the FrontierDevTools API and World API — code calls them via `IFrontierSharpHttpClient` using a configured `HttpClient` name (default: "FrontierDevTools"). Update options in `FrontierSharp.CommandLine/Program.cs` or config files.
  - NuGet packages: Serilog, Spectre.Console, FluentResults, FusionCache, System.IO.Abstractions. Tests use NSubstitute and xUnit.
  - Artifacts: tests embed many `WorldApi` JSON payloads as EmbeddedResource — be cautious changing those files as tests depend on exact payloads.

- Quick examples agents can use

  - To add a new CLI command: add a `Command` class under `FrontierSharp.CommandLine/Commands`, register it in `Program.cs` using `config.AddCommand<YourCommand>("your-name").WithAlias("yn")` and add tests in `FrontierSharp.Tests/CommandLine`.
  - To call the API client from a new service: inject `IFrontierDevToolsClient` or `IWorldApiClient` (registered as singletons in `Program.cs`) and call the high-level methods (e.g., `CalculateDistance`, `FindTravelRoute`).

- Files to inspect first when debugging

  - `src/FrontierSharp.CommandLine/Program.cs` — DI, logging, command registration.
  - `src/FrontierSharp.HttpClient/FrontierSharpHttpClient.cs` — HTTP, caching, serialization, error handling.
  - `src/FrontierSharp.FrontierDevTools.Api/FrontierDevToolsClient.cs` — high-level API operations.
  - `src/FrontierSharp.WorldApi/WorldApiClient.cs` — paging helpers and payload handling.
  - `src/FrontierSharp.Tests/` — where fixtures and tests live; use them to validate behavior.

- What NOT to change

  - Don't change the `GetCacheKey()` semantics on request models without updating tests that expect stable cache behavior.
  - Avoid renaming public methods on API clients without updating all callers and tests.

- What to avoid
  - avoid adding unnecessary comments such as `// ... existing code ...` or `// TODO: implement this` unless it's a real TODO.
  - under no circumstances should you disable security controls including disabling GPG because a signing request failed - if a request fails inform the user and hold.

If anything here is unclear or you'd like this expanded (examples of tests, preferred logging format, or adding a local Docker workflow), tell me which area to expand and I'll iterate.
