#!/bin/sh
echo "=== Knowverse Portal Starting ==="
echo "PORT=$PORT"
echo "Frontend path=$ApiSettings__FrontendPath"

# Set ASPNETCORE_URLS from Render's PORT variable
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-5200}"
echo "Listening on: $ASPNETCORE_URLS"

# Run dotnet as PID 1 (exec replaces shell so signals work correctly)
exec dotnet backend.dll
