# ── Stage 1: Build ───────────────────────────────────────────────────────────
# Force linux/amd64 to match Render infrastructure
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/backend.csproj ./backend/
RUN dotnet restore ./backend/backend.csproj

COPY backend/ ./backend/
COPY frontend/ ./frontend/

# Publish as linux-x64 self-aware binary
RUN dotnet publish ./backend/backend.csproj -c Release -o /app/out -r linux-x64 --no-self-contained

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .
COPY --from=build /src/frontend ./frontend
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

# Disable JIT features incompatible with older/constrained CPUs on free tier
ENV DOTNET_TieredCompilation=0
ENV DOTNET_ReadyToRun=0
ENV DOTNET_GCConserveMemory=5

# Clear ASP.NET default port bindings (entrypoint.sh sets ASPNETCORE_URLS)
ENV ASPNETCORE_HTTP_PORTS=""
ENV ASPNETCORE_URLS=""

# Frontend path inside container
ENV ApiSettings__FrontendPath=./frontend

EXPOSE 5200

ENTRYPOINT ["./entrypoint.sh"]
