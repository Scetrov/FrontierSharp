# Plan: Expand CLI with Full WorldApiClient Coverage

Implement new CLI commands exposing all `IWorldApiClient` methods as top-level commands with fuzzy name matching, using shared base classes for both commands and settings to reduce duplication. All commands will have comprehensive tests.

## Steps

1. **Rename fuzzy threshold in [ConfigurationOptions.cs](src/FrontierSharp.CommandLine/ConfigurationOptions.cs)** — Rename `TribeFuzzyWarningThreshold` to `FuzzyWarningThreshold`; update reference in `GetTribeCommand`.

2. **Create [BaseWorldApiSettings.cs](src/FrontierSharp.CommandLine/Commands/BaseWorldApiSettings.cs)** — Abstract settings base class with common options: `--page-size` for pagination control; `Validate()` base implementation for mutual exclusivity checks.

3. **Create [BaseWorldApiCommand.cs](src/FrontierSharp.CommandLine/Commands/BaseWorldApiCommand.cs)** — Abstract generic base `BaseWorldApiCommand<TSettings> : AsyncCommand<TSettings> where TSettings : BaseWorldApiSettings` containing:
   - `BuildFuzzyCandidates<T>(items, name, nameSelector)` — Generic fuzzy matching
   - `LoadAllPagesAsync<T>(pageFunc, pageSize)` — Generic pagination helper
   - `RenderFuzzyWarning(name, distance)` — Shared warning output
   - `RenderMultipleMatches<T>(candidates, nameSelector, idSelector)` — Disambiguation rendering

4. **Refactor [GetTribeCommand.cs](src/FrontierSharp.CommandLine/Commands/GetTribeCommand.cs)** — Inherit from `BaseWorldApiCommand<GetTribeCommand.Settings>`; have `Settings` extend `BaseWorldApiSettings`; replace inline fuzzy/pagination logic with base class methods.

5. **Create [SolarSystemCommand.cs](src/FrontierSharp.CommandLine/Commands/SolarSystemCommand.cs)** — Extends `BaseWorldApiCommand` with nested `Settings : BaseWorldApiSettings` containing `--show-all`, `--id`, `--name`; render system details and smart assemblies.

6. **Create [SmartCharacterCommand.cs](src/FrontierSharp.CommandLine/Commands/SmartCharacterCommand.cs)** — Extends `BaseWorldApiCommand` with nested `Settings : BaseWorldApiSettings` containing `--show-all`, `--address`, `--name`; show balances and assemblies.

7. **Create [SmartAssemblyCommand.cs](src/FrontierSharp.CommandLine/Commands/SmartAssemblyCommand.cs)** — Extends `BaseWorldApiCommand` with nested `Settings : BaseWorldApiSettings` containing `--show-all`, `--id`, `--name`; render solar system, type, manufacturing, location.

8. **Create [KillmailCommand.cs](src/FrontierSharp.CommandLine/Commands/KillmailCommand.cs)** — Extends `BaseWorldApiCommand` with nested `Settings : BaseWorldApiSettings` containing `--show-all`, `--victim`, `--killer` fuzzy name filters.

9. **Create [TypeCommand.cs](src/FrontierSharp.CommandLine/Commands/TypeCommand.cs), [FuelCommand.cs](src/FrontierSharp.CommandLine/Commands/FuelCommand.cs), [ConfigCommand.cs](src/FrontierSharp.CommandLine/Commands/ConfigCommand.cs)** — Each extends `BaseWorldApiCommand` with own nested `Settings : BaseWorldApiSettings`; support `--show-all` and `--name`/`--id` where applicable.

10. **Register commands in [Program.cs](src/FrontierSharp.CommandLine/Program.cs)** — Add all commands top-level after `tribe`:
    - `solarsystem`/`system`/`ss`
    - `character`/`char`/`c`
    - `assembly`/`asm`/`a`
    - `killmail`/`km`
    - `type`/`ty`
    - `fuel`/`fu`
    - `config`/`cfg`

11. **Create test classes in [FrontierSharp.Tests/CommandLine/Commands/](src/FrontierSharp.Tests/CommandLine/Commands/)** — Add `BaseWorldApiCommandTests.cs` (test shared fuzzy/pagination), `SolarSystemCommandTests.cs`, `SmartCharacterCommandTests.cs`, `SmartAssemblyCommandTests.cs`, `KillmailCommandTests.cs`, `TypeCommandTests.cs`, `FuelCommandTests.cs`, `ConfigCommandTests.cs` using NSubstitute mocks following `GetTribeCommandTests` pattern.

