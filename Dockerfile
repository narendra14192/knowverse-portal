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

# Render sets PORT automatically; default to 5200 locally
ENV PORT=5200
ENV ApiSettings__FrontendPath=./frontend

EXPOSE $PORT

ENTRYPOINT ["dotnet", "backend.dll"]
