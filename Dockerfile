# ── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/backend.csproj ./backend/
RUN dotnet restore ./backend/backend.csproj

COPY backend/ ./backend/
COPY frontend/ ./frontend/

RUN dotnet publish ./backend/backend.csproj -c Release -o /app/out

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
# Use jammy (Ubuntu 22.04) for broadest native library compatibility
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
WORKDIR /app

COPY --from=build /app/out .
COPY --from=build /src/frontend ./frontend

# Clear conflicting defaults set by the base image
ENV ASPNETCORE_URLS=""
ENV ASPNETCORE_HTTP_PORTS=""

# Frontend path inside the container
ENV ApiSettings__FrontendPath=./frontend

# Mild memory conservation (safe for 512MB Render free tier)
ENV DOTNET_GCConserveMemory=5

# Use shell form so $PORT is expanded at runtime (Render sets PORT dynamically)
CMD dotnet backend.dll
