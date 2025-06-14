#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE="$ROOT/src"
OUTPUT="$ROOT/nupkg"
WORKDIR="$ROOT/nupkg_inspect"
VERSION="0.0.99"
ASSEMBLYVERSION="${VERSION}.0"

rm -rf "$WORKDIR"
rm -rf "$OUTPUT"
mkdir -p "$WORKDIR"
mkdir -p "$OUTPUT"

cd "$SOURCE" || { echo "Source directory not found: $SOURCE"; exit 1; }

echo "=== dotnet clean ==="
dotnet clean
echo "=== dotnet restore ==="
dotnet restore
echo "=== dotnet build ==="
dotnet build --no-restore --configuration Release -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLYVERSION} -p:FileVersion=${ASSEMBLYVERSION}
echo "=== dotnet pack ==="
dotnet pack "./FrontierSharp.sln" --output "$OUTPUT" --no-build --configuration Release -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLYVERSION} -p:FileVersion=${ASSEMBLYVERSION}

cd "$ROOT" || { echo "Root directory not found: $ROOT"; exit 1; }

echo "=== Unzipping nupkgs ==="
for file in "$OUTPUT/*.nupkg"; do
  echo " == $(basename "$file") =="
  unzip -j "$file" '*.dll' -d "$WORKDIR"
done

echo "=== Assembly Info ==="
find "$WORKDIR" -type f -name '*.dll' | while read -r dll; do
  echo "== $(basename "$dll") =="
  exiftool "$dll" -Version -AssemblyVersion -FileVersion
done
