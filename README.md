# AI-First Incident Management Platform

A backend REST API for managing incidents, capturing structured logs, running AI-driven root cause analysis, and uploading file attachments to AWS S3. Built with .NET 8 using Clean Architecture across 5 dedicated projects.

---

## What It Does

- **Incident management** — Create, read, update, and delete incidents stored in SQL Server.
- **Log ingestion** — Attach structured log entries to incidents, stored in MongoDB Atlas.
- **AI analysis** — Analyze an incident's logs to produce a severity rating, incident category, root cause assessment, suggested fix, and recommended tests.
- **File attachments** — Upload files (reports, screenshots, diagnostics) directly to AWS S3, linked to an incident.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 / ASP.NET Core Web API |
| Relational DB | SQL Server + EF Core 8.0 |
| Document DB | MongoDB Atlas (MongoDB.Driver 3.7) |
| File storage | AWS S3 (AWSSDK.S3 4.0) |
| API docs | Swagger / Swashbuckle 6.6 |
| Testing | xUnit 2.5 + Moq 4.20 + FluentAssertions 8.8 |

---

## Architecture

The solution is split into 5 projects following Clean Architecture:

```
src/
├── IncidentPlatform.Domain/
│   └── Entities/             # Incident, LogEntry (BSON), AIAnalysisResult, IncidentDTO
│
├── IncidentPlatform.Application/
│   ├── Interfaces/           # IIncidentRepository, IIncidentService, ILogRepository,
│   │                         # ILogService, IAIAnalysisService
│   └── Services/             # IncidentService, LogService, AIAnalysisService
│
├── IncidentPlatform.Infrastructure/
│   ├── Data/                 # IncidentDbContext (EF Core), MongoLogContext, MongoSettings
│   └── Repositories/         # IncidentRepository (SQL), LogRepository (MongoDB)
│
├── IncidentPlatform.API/
│   ├── Controllers/          # IncidentsController, LogController, AIController,
│   │                         # AttachmentController
│   ├── Models/               # AwsSettings
│   └── Services/             # IAwsFileService, AwsFileService
│
tests/
└── IncidentPlatform.Tests/
    ├── Services/             # IncidentServiceTests, LogServiceTests, AIAnalysisServiceTests
    └── Controllers/          # IncidentControllerTests, AIControllerTests
```

**Dependency rule:** Domain ← Application ← Infrastructure ← API (outer layers depend inward, never the reverse).

**Data flow for AI analysis:**

```
POST /api/ai/analyze/{incidentId}
   → fetch Incident (SQL Server via EF Core)
   → fetch Logs     (MongoDB Atlas)
   → AIAnalysisService (keyword scoring → category + severity)
   → return AIAnalysisResult { Severity, Category, RootCause, SuggestedFix, RecommendedTests }
```

**Data flow for file upload:**

```
POST /api/incidents/{incidentId}/attachments
   → validate IFormFile
   → AwsFileService.UploadFileAsync (AWSSDK.S3 → PutObjectRequest)
   → return { IncidentId, FileKey }
```

---

## Endpoints

### Incidents

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/incidents` | List all incidents |
| `GET` | `/api/incidents/{id}` | Get incident by ID |
| `POST` | `/api/incidents` | Create a new incident |
| `PUT` | `/api/incidents/{id}` | Update an incident |
| `DELETE` | `/api/incidents/{id}` | Delete an incident |

**Request body (POST / PUT):**
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

**Request body (POST):**
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
  "category": "database",
  "rootCause": "Multiple database connection timeouts detected. Possible connection pool exhaustion or unreachable DB host.",
  "suggestedFix": "Check connection pool limits, verify DB host availability, review recent schema migrations.",
  "recommendedTests": [
    "Add integration tests for DB connection retry logic.",
    "Simulate connection pool exhaustion in a staging environment.",
    "Add health check endpoint for database connectivity."
  ]
}
```

**Severity levels:** `Low` (no logs) → `Medium` → `High` → `Critical`  
**Categories detected:** `database`, `infrastructure`, `authentication`, `network`, `general`

### File Attachments

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/incidents/{incidentId}/attachments` | Upload a file to AWS S3 |

**Request:** `multipart/form-data`, field name `file`

**Response:**
```json
{
  "incidentId": "7511f161-72f8-440f-b1e7-ed3b7cfefa6a",
  "fileKey": "a8f3b3f2-1234-..._report.pdf"
}
```

Swagger UI is always available at `/swagger`.

---

## Run Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Docker)
- MongoDB (local or [MongoDB Atlas](https://www.mongodb.com/atlas))
- AWS account with an S3 bucket (for file upload feature)
- AWS credentials configured locally (`aws configure` or environment variables)

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
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "IncidentLogsDB",
    "LogsCollectionName": "Logs"
  }
}
```

For MongoDB Atlas, replace `ConnectionString` with your Atlas URI:
```
mongodb+srv://<user>:<password>@cluster0.xxxxx.mongodb.net/IncidentLogsDB?retryWrites=true&w=majority
```

> `appsettings.Development.json` is gitignored — never commit real credentials.

### 3. Configure AWS credentials

The S3 upload uses the AWS default credential chain. Set environment variables before running:

```powershell
$env:AWS_ACCESS_KEY_ID     = "your-access-key"
$env:AWS_SECRET_ACCESS_KEY = "your-secret-key"
```

Or configure via `aws configure` (requires [AWS CLI](https://aws.amazon.com/cli/)).

Update `appsettings.json` with your bucket name and region:
```json
"AwsSettings": {
  "BucketName": "your-bucket-name",
  "Region": "us-east-1"
}
```

### 4. Apply EF Core migrations

```bash
dotnet ef database update --project src/IncidentPlatform.Infrastructure --startup-project src/IncidentPlatform.API
```

### 5. Run the API

```bash
dotnet run --project src/IncidentPlatform.API
```

The API starts at `http://localhost:5116`. Swagger is available at `http://localhost:5116/swagger`.

---

## Run Tests

```bash
dotnet test tests/IncidentPlatform.Tests/IncidentPlatform.Tests.csproj
```

All tests are pure unit tests using mocked dependencies — no database or AWS connection required.

**Test coverage (22 tests):**

| File | Tests | What's covered |
|------|-------|----------------|
| `Services/IncidentServiceTests.cs` | 7 | Full CRUD, update/delete not-found paths |
| `Services/LogServiceTests.cs` | 4 | Field assignment, data preservation, empty list |
| `Services/AIAnalysisServiceTests.cs` | 7 | Critical/High/Medium per category (database, network, auth, infrastructure), multi-keyword scoring, empty logs |
| `Controllers/IncidentControllerTests.cs` | 2 | NotFound, CreatedAtAction |
| `Controllers/AIControllerTests.cs` | 2 | NotFound when incident missing, Ok with full analysis result |

---

## AI Analysis Logic

`AIAnalysisService` uses keyword scoring across the last 20 log messages:

1. **Category detection** — scores each log against 4 keyword dictionaries (`database`, `infrastructure`, `authentication`, `network`). The highest-scoring category wins.
2. **Severity detection** — within the detected category, checks for critical-tier keywords first, then high-tier keywords.
3. **Context-aware diagnostics** — `(category, severity)` pairs map to specific root cause explanations, suggested fixes, and recommended tests.

| Category | Example keywords |
|----------|-----------------|
| `database` | `deadlock`, `connection pool`, `query timeout`, `replica`, `mongo` |
| `infrastructure` | `out of memory`, `oom`, `pod crash`, `kubernetes`, `health check` |
| `authentication` | `unauthorized`, `401`, `jwt`, `invalid credentials`, `oauth` |
| `network` | `timeout`, `connection refused`, `502`, `503`, `dns`, `upstream` |

This is a heuristic placeholder — designed to be replaced by an LLM-backed implementation (e.g. OpenAI or Azure OpenAI).

---

## AI-Assisted Development

This project was built using **GitHub Copilot** inside VS Code as the primary AI development assistant.

- **Scaffolding**: Generated the full Clean Architecture folder structure, domain models, EF Core contexts, MongoDB integration, and AWS S3 service from natural language specifications.
- **CRUD & services**: Service interfaces, repository implementations, and controller actions generated and iterated conversationally.
- **AI analysis logic**: The `AIAnalysisService` keyword scoring system (category detection → severity mapping → context-aware diagnostics) was designed collaboratively with Copilot.
- **Test authoring**: All 22 unit tests (xUnit + Moq + FluentAssertions) written with Copilot, including Arrange/Act/Assert patterns, mock setup, and edge cases.
- **Security review**: Copilot identified credential exposure risks, implemented `.gitignore` coverage for `appsettings.Development.json`, certificates, and `.env.*` variants, and created `appsettings.Example.json` as a safe template.

The entire workflow was conversational — each feature described in natural language, Copilot handled code generation, file creation, build validation, and commit preparation.


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
