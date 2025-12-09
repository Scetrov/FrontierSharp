# FrontierSharp.CommandLine

## Description

`frontierctl` is a cross-platform command line application designed for interacting with the EVE Frontier world via the
API, Blockchain and Mud Tables.

## Downloading Binaries

You can download binaries from the [Releases](https://github.com/Scetrov/FrontierSharp/releases) page.

## Running with Docker

You can run `frontierctl` using Docker. The following command will run the latest version of `frontierctl`:

```sh
docker run ghcr.io/scetrov/frontiersharp:latest rider --name Scetrov
```

## Building from Source

To build `frontierctl` from source, you can use the following steps:

1. Clone the repository:
    ```sh
    git clone https://github.com/scetrov/FrontierSharp.git
    ```

2. Navigate to the project directory:
    ```sh
    cd FrontierSharp.CommandLine
    ```

3. Install the required dependencies:
    ```sh
    dotnet restore
    ```

4. Build the project:
    ```sh
    dotnet build --configuration Release
    ```

### Usage

After installation, you can use the CLI tool by finding `frontierctl` in the `bin/Release/net10.0` directory of the
project. You can run it directly from the command line:

```sh
frontierctl --help
```

For detailed usage examples, see [EXAMPLES.md](EXAMPLES.md).

## Commands

`frontierctl` provides several commands for interacting with the EVE Frontier World API:

### Tribe Command

Query tribe (corporation) data from the World API with fuzzy name matching.

```sh
# List all tribes
frontierctl tribe --show-all

# Get tribe by exact ID
frontierctl tribe --id 98000314

# Search for a tribe by name (supports fuzzy matching)
frontierctl tribe --name "Reapers"

# Aliases: t, corporation, corp
frontierctl t --name "Reapers"
```

### Solar System Command

Query solar system data from the World API.

```sh
# List all solar systems
frontierctl solarsystem --show-all

# Get solar system by ID
frontierctl solarsystem --id 30024077

# Search for a solar system by name (supports fuzzy matching)
frontierctl solarsystem --name "Q:50K9"

# Aliases: system, ss
frontierctl ss --name "Q:50K9"
```

### Smart Character Command

Query smart character data including balances and smart assemblies.

**Note:** Assembly names are often empty in the API, so the command displays assembly types instead.

```sh
# List all characters
frontierctl character --show-all

# Get character by wallet address
frontierctl character --address "0xb40b47e10a771cb0d997d866440459baab32df9c"

# Search for a character by name (supports fuzzy matching)
frontierctl character --name "Scetrov"

# Aliases: char, c
frontierctl c --address "0xb40b47e10a771cb0d997d866440459baab32df9c"
```

### Smart Assembly Command

Query smart assembly (structures/ships) data from the World API.

```sh
# List all assemblies
frontierctl assembly --show-all

# Get assembly by ID
frontierctl assembly --id "assembly-id-123"

# Search for an assembly by name (supports fuzzy matching)
frontierctl assembly --name "Titan"

# Aliases: asm, a
frontierctl asm --name "Titan"
```

### Killmail Command

Query killmail data from the World API.

```sh
# List all killmails
frontierctl killmail --show-all

# Filter killmails by victim name (supports fuzzy matching)
frontierctl killmail --victim-name "Scetrov"

# Aliases: km
frontierctl km --show-all
```

### Type Command

Query game type (item/ship/structure types) data from the World API.

```sh
# List all types
frontierctl type --show-all

# Get type by ID
frontierctl type --id 88765

# Search for a type by name (supports fuzzy matching)
frontierctl type --name "Shipyard L"

# Aliases: tpe, tp
frontierctl tp --name "Silicate Minerals"
```

### Fuel Command

Query fuel efficiency data for different fuel types.

**Note:** Efficiency is a rating value (with 90 being the practical maximum) where higher efficiency values mean better fuel economy (less fuel consumed per lightyear).

```sh
# List all fuels
frontierctl fuel --show-all

# Get fuel data by type ID
frontierctl fuel --id 123

# Search for fuel by type name (supports fuzzy matching)
frontierctl fuel --name "Hydrogen"

# Aliases: f
frontierctl f --show-all
```

### Config Command

View World API configuration including chain information and API endpoints.

```sh
# Show detailed config information
frontierctl config

# Show all configs in table format
frontierctl config --show-all

# Aliases: cfg
frontierctl cfg
```

### Data Commands

Commands for managing static game data.

```sh
# List static resources
frontierctl data static resources list

# Unpickle (extract) a static resource
frontierctl data static resources unpickle --name "resource-name"

# Aliases for resources: res, r
frontierctl data static res l
```

## Common Options

Most commands support the following options:

- `--show-all`: Display all items in paginated format
- `--id <id>`: Query by specific ID
- `--name <name>`: Search by name with fuzzy matching support
- `--page-size <size>`: Control pagination size (default varies by command)

**Fuzzy Matching**: When searching by name, the tool will warn you if the match distance exceeds the configured threshold (default: 3). You can adjust this in `config.json`:

```json
{
  "Configuration": {
    "FuzzyWarningThreshold": 3,
    "TribeMembersLimit": 25
  }
}
```

## Contributing

We welcome contributions to FrontierSharp.CommandLine! If you would like to contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Make your changes and commit them with clear and concise messages.
4. Push your changes to your fork.
5. Create a pull request to the main repository.

Please ensure that your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
