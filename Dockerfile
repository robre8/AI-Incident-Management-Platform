# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files first — restores independently (better layer caching)
COPY AI-First-Incident-Management-Platform.sln .
COPY src/IncidentPlatform.Domain/IncidentPlatform.Domain.csproj                       src/IncidentPlatform.Domain/
COPY src/IncidentPlatform.Application/IncidentPlatform.Application.csproj             src/IncidentPlatform.Application/
COPY src/IncidentPlatform.Infrastructure/IncidentPlatform.Infrastructure.csproj       src/IncidentPlatform.Infrastructure/
COPY src/IncidentPlatform.API/IncidentPlatform.API.csproj                             src/IncidentPlatform.API/

RUN dotnet restore src/IncidentPlatform.API/IncidentPlatform.API.csproj

# Copy full source and publish
COPY src/ src/
RUN dotnet publish src/IncidentPlatform.API/IncidentPlatform.API.csproj \
    -c Release \
    -o /app/out \
    --no-restore

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IncidentPlatform.API.dll"]
