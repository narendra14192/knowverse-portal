# ── Stage 1: Build the C# backend ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore backend dependencies
COPY backend/backend.csproj ./backend/
RUN dotnet restore ./backend/backend.csproj

# Copy all source files
COPY backend/ ./backend/
COPY frontend/ ./frontend/

# Publish release build
RUN dotnet publish ./backend/backend.csproj -c Release -o /app/publish

# Copy frontend static files into the published output location
RUN cp -r ./frontend /app/frontend

# ── Stage 2: Runtime image ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published backend
COPY --from=build /app/publish .

# Copy frontend static files
COPY --from=build /app/frontend ./frontend

# ── Environment Configuration ─────────────────────────────────────────────────
# Clear default ASP.NET port bindings — our app reads PORT env var directly
ENV ASPNETCORE_HTTP_PORTS=""
ENV ASPNETCORE_URLS=""

# Frontend path inside container
ENV ApiSettings__FrontendPath=./frontend

# Memory conservation for Render free tier (512MB limit)
ENV DOTNET_GCConserveMemory=9
ENV DOTNET_GCHeapHardLimit=419430400

# Render injects PORT at runtime; default to 5200 for local use
ENV PORT=5200

EXPOSE 5200

# Use CMD (not ENTRYPOINT) so Render can override if needed
CMD ["dotnet", "backend.dll"]
