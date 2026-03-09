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

# Get system by ID
frontierctl system --id 30000142
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
# Get a specific solar system and inspect the output
frontierctl system --name "Q:50K9" | grep -A 10 "USR-21H"
```

#### Chain Commands

```sh
# Look up a tribe, then inspect a solar system
frontierctl tribe --name "Scetrov" && frontierctl ss --name "Q:50K9"
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
