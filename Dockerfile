# ── Stage 1: Build ───────────────────────────────────────────────────────────
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/backend.csproj ./backend/
RUN dotnet restore ./backend/backend.csproj

COPY backend/ ./backend/
COPY frontend/ ./frontend/
COPY entrypoint.sh .

# Fix line endings in case of Windows CRLF
RUN sed -i 's/\r$//' entrypoint.sh

RUN dotnet publish ./backend/backend.csproj -c Release -o /app/out

# ── Stage 2: Runtime (use full SDK — avoids stripped-runtime crashes) ─────────
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .
COPY --from=build /src/frontend ./frontend
COPY --from=build /src/entrypoint.sh .
RUN chmod +x entrypoint.sh

# Disable aggressive JIT for CPU compatibility on Render free tier
ENV DOTNET_TieredCompilation=0
ENV DOTNET_ReadyToRun=0
ENV DOTNET_GCConserveMemory=5

# Clear conflicting defaults
ENV ASPNETCORE_HTTP_PORTS=""
ENV ASPNETCORE_URLS=""

# Frontend path
ENV ApiSettings__FrontendPath=./frontend

EXPOSE 5200

ENTRYPOINT ["./entrypoint.sh"]
