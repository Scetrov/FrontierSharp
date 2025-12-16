# FrontierSharp CommandLine Examples

This document provides practical examples for using `frontierctl` commands.

## Tribe Examples

### List All Tribes

```sh
# List all tribes with pagination
frontierctl tribe --show-all

# Control page size for faster/slower streaming
frontierctl tribe --show-all --page-size 50
```

### Query Specific Tribe

```sh
# Get tribe by exact ID
frontierctl tribe --id 42

# Search by name (fuzzy matching enabled)
frontierctl tribe --name "Mining Corp"

# Using aliases
frontierctl t --name "Explorers"
frontierctl corp --id 100
```

## Solar System Examples

### Explore Systems

```sh
# List all solar systems
frontierctl solarsystem --show-all

# Find a specific system by name
frontierctl ss --name "Jita"

# Get system by ID and see smart assemblies
frontierctl system --id 30000142
```

## Character Examples

### Character Information

```sh
# Get character by wallet address
frontierctl character --address "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb"

# Search by character name
frontierctl char --name "Scetrov"

# List all characters (warning: may be large)
frontierctl c --show-all --page-size 100
```

## Assembly Examples

### Smart Assemblies

```sh
# List all assemblies in the universe
frontierctl assembly --show-all

# Find specific assembly by name
frontierctl asm --name "Hauler"

# Get detailed assembly info by ID
frontierctl a --id "assembly-123456"
```

## Killmail Examples

### Combat History

```sh
# View all killmails
frontierctl killmail --show-all

# Filter by victim name
frontierctl km --victim-name "Scetrov"

# Stream recent killmails with custom page size
frontierctl km --show-all --page-size 50
```

## Type Examples

### Item and Ship Types

```sh
# List all game types
frontierctl type --show-all

# Search for specific item type
frontierctl tp --name "Tritanium"

# Get type details by ID
frontierctl tpe --id 34
```

## Fuel Examples

### Fuel Efficiency Data

> [!NOTE]
> Efficiency is a rating value (with 90 being the practical maximum). Higher efficiency values mean better fuel
> economy (less fuel consumed per lightyear).

```sh
# List all fuels and their efficiency ratings
frontierctl fuel --show-all

# Find fuel by type name
frontierctl f --name "EU-90 Fuel"

# Get fuel data by type ID
frontierctl fuel --id 78437
```

## Config Examples

### World API Configuration

```sh
# Show detailed config (default)
frontierctl config

# Show all configs in table format
frontierctl cfg --show-all
```

## Data Management Examples

### Static Resources

```sh
# List available static resources
frontierctl data static resources list

# Extract/unpickle a resource
frontierctl data static res unpickle --name "items"

# Using aliases for brevity
frontierctl data static r l
```

## Advanced Usage

### Fuzzy Matching

When searching by name, the tool uses Levenshtein distance for fuzzy matching:

```sh
# These will all find "EVE Frontier Corporation"
frontierctl tribe --name "EVE Frontier Corporation"  # Exact match
frontierctl tribe --name "eve frontier corp"         # Case insensitive
frontierctl tribe --name "EVE Frontir Corp"          # Typo tolerance
```

If the match distance exceeds the configured threshold (default: 3), you'll see a warning:

```
[WARNING] Closest match for 'EVE Frontir' has distance 4; rerun with --id for certainty.
```

### Configuration File

Create a `config.json` file in the same directory as `frontierctl`:

```json
{
  "Configuration": {
    "FuzzyWarningThreshold": 3,
    "TribeMembersLimit": 25
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Combining with Other Tools

#### Export to CSV (using PowerShell on Windows)

```powershell
# Export all tribes to CSV
frontierctl tribe --show-all | ConvertFrom-Csv > tribes.csv
```

#### Filter and Format (using bash/zsh)

```sh
# Get specific system and format output
frontierctl system --name "Q:50K9" | grep -A 10 "Smart Assemblies"
```

#### Chain Commands

```sh
# Get config and check a specific character
frontierctl config && frontierctl char --name "Scetrov"
```

## Docker Usage Examples

### Basic Docker Usage

```sh
# Run with Docker
docker run --rm ghcr.io/scetrov/frontiersharp:latest tribe --name "MyTribe"

# Mount config file
docker run --rm -v $(pwd)/config.json:/app/config.json ghcr.io/scetrov/frontiersharp:latest tribe --show-all
```

## Troubleshooting

### Common Issues

**Issue**: "Multiple close matches found"

```sh
# If you get multiple matches, use --id instead
frontierctl tribe --name "Corp"  # May return multiple matches
frontierctl tribe --id 000000... # Precise query
```

**Issue**: Rate limiting or slow responses

```sh
# Reduce page size to make smaller requests
frontierctl tribe --show-all --page-size 10
```

**Issue**: Need more verbose output

```sh
# Set log level in config.json or use environment variable
export ASPNETCORE_ENVIRONMENT=Development
frontierctl tribe --show-all
```
