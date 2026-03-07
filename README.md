# AI-First Incident Management Platform

A backend REST API for managing incidents, capturing structured logs, and running AI-driven root cause analysis. Built with .NET 8 and Clean Architecture principles.

---

## What It Does

- **Incident management** — Create, read, update, and delete incidents stored in SQL Server.
- **Log ingestion** — Attach structured log entries to incidents, stored in MongoDB.
- **AI analysis** — Analyze an incident's logs to produce a severity rating, root cause assessment, suggested fix, and recommended tests.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 / ASP.NET Core Web API |
| Relational DB | SQL Server (EF Core 8) |
| Document DB | MongoDB (MongoDB.Driver 2.28) |
| API docs | Swagger / Swashbuckle 6.6 |
| Testing | xUnit 2.5 + Moq 4.20 + FluentAssertions 8.8 |

---

## Architecture

The solution lives under `src/IncidentPlatform.API` and follows Clean Architecture with vertical slicing:

```
src/IncidentPlatform.API/
├── Controllers/          # HTTP layer (IncidentController, LogController, AIController)
├── Services/             # Business logic interfaces + implementations
├── Repositories/         # Data access interfaces + implementations
├── Models/               # Domain models and DTOs
└── Infrastructure/
    └── Data/             # EF Core DbContext (SQL Server) + MongoDB context

tests/
└── IncidentPlatform.Tests/
    ├── Controllers/      # Controller unit tests
    └── Services/         # Service and AI unit tests (19 tests total)
```

**Data flow for AI analysis:**

```
POST /api/ai/analyze/{incidentId}
   → fetch Incident (SQL Server via EF Core)
   → fetch Logs     (MongoDB)
   → AIAnalysisService (keyword heuristics)
   → return AIAnalysisResult { Severity, RootCause, SuggestedFix, RecommendedTests }
```

---

## Main Endpoints

### Incidents

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/incidents` | List all incidents |
| `GET` | `/api/incidents/{id}` | Get incident by ID |
| `POST` | `/api/incidents` | Create a new incident |
| `PUT` | `/api/incidents/{id}` | Update an incident |
| `DELETE` | `/api/incidents/{id}` | Delete an incident |

**Request body for POST / PUT:**
```json
{
  "title": "Payment service down",
  "description": "Checkout flow failing for all users"
}
```

### Logs

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/incidents/{incidentId}/logs` | Get all logs for an incident |
| `POST` | `/api/incidents/{incidentId}/logs` | Add a log entry to an incident |

**Request body for POST:**
```json
{
  "service": "PaymentService",
  "logLevel": "ERROR",
  "message": "Database connection timeout after 30s"
}
```

### AI Analysis

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/ai/analyze/{incidentId}` | Analyze an incident's logs |

**Response:**
```json
{
  "severity": "Critical",
  "rootCause": "Possible issue detected from logs related to 'Payment service down'.",
  "suggestedFix": "Review failing dependencies, inspect recent deployments...",
  "recommendedTests": [
    "Add unit tests for the affected service method.",
    "Add integration tests for the failing workflow.",
    "Add regression tests covering the reported incident scenario."
  ]
}
```

Swagger UI is always available at `/swagger` when the API is running.

---

## Run Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Docker)
- MongoDB (local or Docker)

### 1. Clone the repo

```bash
git clone https://github.com/robre8/AI-First-Incident-Management-Platform.git
cd AI-First-Incident-Management-Platform
```

### 2. Configure local secrets

Copy the example config and fill in your credentials:

```bash
cp src/IncidentPlatform.API/appsettings.Example.json src/IncidentPlatform.API/appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=IncidentPlatformDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "MongoSettings": {
    "ConnectionString": "mongodb://localhost:27017"
  }
}
```

> `appsettings.Development.json` is gitignored. Never commit real credentials.

### 3. Apply EF Core migrations

```bash
cd src/IncidentPlatform.API
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run --project src/IncidentPlatform.API
```

The API starts at `https://localhost:7xxx` (port shown in terminal). Swagger is available at `/swagger`.

---

## AI-Assisted Development

This project was built using **GitHub Copilot** inside VS Code as the primary AI development assistant.

- **Scaffolding**: Copilot generated the full Clean Architecture folder structure, model classes, EF Core contexts, and MongoDB integration from a natural language specification.
- **CRUD generation**: Service interfaces, repository implementations, and controller actions were generated and iterated conversationally.
- **Test writing**: All 19 unit tests (xUnit + Moq + FluentAssertions) were authored with Copilot, including Arrange/Act/Assert patterns and mock setup.
- **Security review**: Copilot identified that `appsettings.Development.json` with real credentials was unprotected, suggested creating `.gitignore`, cleaning `appsettings.json`, and adding `appsettings.Example.json` as a safe template.
- **AI analysis logic**: The `AIAnalysisService` heuristic (keyword scanning → severity mapping) was designed collaboratively with Copilot as a placeholder for a future LLM-backed implementation.

The workflow was fully conversational — each feature was described in natural language and Copilot handled code generation, file creation, build validation, and commit preparation.

---

## Run Tests

```bash
dotnet test tests/IncidentPlatform.Tests/IncidentPlatform.Tests.csproj
```

**Test coverage (19 tests):**

| File | Tests |
|------|-------|
| `Services/IncidentServiceTests.cs` | 7 — full CRUD including update/delete not-found paths |
| `Services/LogServiceTests.cs` | 4 — field assignment, data preservation, empty list |
| `Services/AIAnalysisServiceTests.cs` | 4 — Critical / High / Medium / empty logs |
| `Controllers/IncidentControllerTests.cs` | 2 — NotFound, CreatedAtAction |
| `Controllers/AIControllerTests.cs` | 2 — NotFound when incident missing, Ok with full analysis |

All tests are pure unit tests using mocked dependencies — no database required.
