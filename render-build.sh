#!/bin/bash
set -e

echo "======================================"
echo " Knowverse Portal — Render Build"
echo "======================================"

# Install .NET 8 SDK
echo "[1/3] Installing .NET 8 SDK..."
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- \
  --channel 8.0 \
  --install-dir "$HOME/.dotnet" \
  --no-path

export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

echo ".NET version: $(dotnet --version)"

# Publish as self-contained linux-x64
# Self-contained = bundles .NET runtime inside the output, no runtime install needed at startup
echo "[2/3] Publishing self-contained release..."
dotnet publish backend/backend.csproj \
  -c Release \
  -o out \
  -r linux-x64 \
  --self-contained true \
  -p:PublishReadyToRun=false \
  -p:PublishSingleFile=false

echo "[3/3] Build complete!"
echo "Output files:"
ls -la out/backend* 2>/dev/null || ls -la out/*.dll | head -5
echo "Frontend files:"
ls frontend/
