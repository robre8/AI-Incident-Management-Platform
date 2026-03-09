# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files first — restores independently (better layer caching)
COPY AI-Incident-Management-Platform.sln .
COPY backend/src/IncidentPlatform.Domain/IncidentPlatform.Domain.csproj                       backend/src/IncidentPlatform.Domain/
COPY backend/src/IncidentPlatform.Application/IncidentPlatform.Application.csproj             backend/src/IncidentPlatform.Application/
COPY backend/src/IncidentPlatform.Infrastructure/IncidentPlatform.Infrastructure.csproj       backend/src/IncidentPlatform.Infrastructure/
COPY backend/src/IncidentPlatform.API/IncidentPlatform.API.csproj                             backend/src/IncidentPlatform.API/

RUN dotnet restore backend/src/IncidentPlatform.API/IncidentPlatform.API.csproj

# Copy full source and publish
COPY backend/src/ backend/src/
RUN dotnet publish backend/src/IncidentPlatform.API/IncidentPlatform.API.csproj \
    -c Release \
    -o /app/out \
    --no-restore

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IncidentPlatform.API.dll"]
