#!/bin/bash
# Knowverse Portal — Render Start Script
# Runs the self-contained .NET backend (no dotnet CLI needed)
# Render sets PORT automatically

# Set ASPNETCORE_URLS from Render's PORT (default 5200 for local)
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-5200}"
export ASPNETCORE_HTTP_PORTS=""

# Frontend path (relative to repo root where process runs)
export ApiSettings__FrontendPath="./frontend"
export ApiSettings__MuseApiKey="${ApiSettings__MuseApiKey:-e267525eb7f40f0bdee9a81184697d495998da702a5742cc233667e688e74212}"

echo "======================================"
echo " Knowverse Portal Starting"
echo " PORT      = $PORT"
echo " URLS      = $ASPNETCORE_URLS"
echo " Frontend  = $ApiSettings__FrontendPath"
echo "======================================"

# Make the self-contained binary executable
chmod +x ./out/backend

# Run self-contained native binary — includes .NET runtime, no dotnet CLI needed
exec ./out/backend
