# AI-First Incident Management Platform

A full-stack incident management system built with .NET 8 (Clean Architecture) and a Next.js 14 frontend. Manages incidents across SQL Server (AWS RDS), captures structured logs in MongoDB Atlas, runs AI-driven root cause analysis, and stores file attachments in AWS S3. Deployed to AWS Elastic Beanstalk via GitHub Actions CI/CD.

---

## What It Does

- **Incident management**  Create, read, update, and delete incidents (SQL Server / AWS RDS via EF Core).
- **Log ingestion**  Attach structured log entries to incidents, stored in MongoDB Atlas.
- **AI analysis**  Analyze an incident's logs to produce severity, category, root cause, fix suggestion, and recommended tests.
- **File attachments**  Upload files (reports, screenshots, diagnostics) to AWS S3, linked to an incident.
- **Frontend dashboard**  Next.js 14 UI for browsing incidents, viewing logs, and triggering AI analysis.
- **CI/CD**  GitHub Actions pipeline builds and deploys the backend to AWS Elastic Beanstalk automatically on every push to `main`.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend runtime | .NET 8 / ASP.NET Core Web API |
| Relational DB | SQL Server + EF Core 8.0 (AWS RDS in production) |
| Document DB | MongoDB Atlas (MongoDB.Driver 3.7) |
| File storage | AWS S3 (AWSSDK.S3 4.0) |
| API docs | Swagger / Swashbuckle 6.6 |
| Frontend | Next.js 14 + TypeScript + Tailwind CSS |
| Container | Docker + docker-compose |
| CI/CD | GitHub Actions  AWS Elastic Beanstalk |
| Testing | xUnit 2.5 + Moq 4.20 + FluentAssertions 8.8 |

---

## Architecture

5-project Clean Architecture  one project per layer, strict inside-out dependency rule:

```
backend/
 src/
  IncidentPlatform.Domain/
     Entities/             # Incident, LogEntry (BSON), AIAnalysisResult, IncidentDTO

  IncidentPlatform.Application/
     Interfaces/           # IIncidentRepository, IIncidentService, ILogRepository,
                             # ILogService, IAIAnalysisService
     Services/             # IncidentService, LogService, AIAnalysisService

  IncidentPlatform.Infrastructure/
     Data/                 # IncidentDbContext (EF Core), MongoLogContext, MongoSettings
     Repositories/         # IncidentRepository (SQL), LogRepository (MongoDB)

  IncidentPlatform.API/
     Controllers/          # IncidentsController, LogController, AIController,
                             # AttachmentController
     Models/               # AwsSettings
     Services/             # IAwsFileService, AwsFileService

 tests/
  IncidentPlatform.Tests/
      Services/             # IncidentServiceTests, LogServiceTests, AIAnalysisServiceTests
      Controllers/          # IncidentControllerTests, AIControllerTests

frontend/
 app/
     incidents/            # Incident list + create + delete
        [id]/             # Incident detail + logs + AI analysis
     lib/
         api.ts            # Typed API client
```

**Dependency rule:** Domain  Application  Infrastructure  API. Outer layers depend inward, never the reverse.

### AI Analysis Data Flow

```
POST /api/ai/analyze/{incidentId}
    fetch Incident from SQL Server (EF Core)
    fetch Logs from MongoDB Atlas
    AIAnalysisService: keyword scoring  category + severity
    return AIAnalysisResult { Severity, Category, RootCause, SuggestedFix, RecommendedTests }
```

### File Upload Data Flow

```
POST /api/incidents/{incidentId}/attachments
    validate IFormFile
    AwsFileService.UploadFileAsync  AWS S3 PutObjectRequest
    return { IncidentId, FileKey }
```

---

## API Endpoints

### Incidents

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/incidents` | List all incidents |
| `GET` | `/api/incidents/{id}` | Get incident by ID |
| `POST` | `/api/incidents` | Create a new incident |
| `PUT` | `/api/incidents/{id}` | Update an incident |
| `DELETE` | `/api/incidents/{id}` | Delete an incident |

```json
// POST / PUT body
{
  "title": "Payment service down",
  "description": "Checkout flow failing for all users"
}
```

### Logs

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/incidents/{incidentId}/logs` | Get all logs for an incident |
| `POST` | `/api/incidents/{incidentId}/logs` | Add a log entry |

```json
// POST body
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

```json
// Response
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

**Severity levels:** `Low`  `Medium`  `High`  `Critical`  
**Categories detected:** `database`, `infrastructure`, `authentication`, `network`, `general`

### File Attachments

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/incidents/{incidentId}/attachments` | Upload a file to AWS S3 |

Request: `multipart/form-data`, field name `file`

Swagger UI is always available at `/swagger`.

---

## Run Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (or Docker  see below)
- MongoDB (or [MongoDB Atlas](https://www.mongodb.com/atlas) free tier)
- AWS account with an S3 bucket (for file upload feature only)

### 1. Clone the repo

```bash
git clone https://github.com/robre8/AI-First-Incident-Management-Platform.git
cd AI-First-Incident-Management-Platform
```

### 2. Configure credentials

`appsettings.Development.json` is gitignored. Create it:

```bash
cp backend/src/IncidentPlatform.API/appsettings.Example.json backend/src/IncidentPlatform.API/appsettings.Development.json
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
  },
  "AwsSettings": {
    "BucketName": "your-s3-bucket",
    "Region": "us-east-1"
  }
}
```

For MongoDB Atlas replace ConnectionString with the `mongodb+srv://...` URI.

### 3. Set AWS credentials (file upload only)

```powershell
$env:AWS_ACCESS_KEY_ID     = "your-access-key"
$env:AWS_SECRET_ACCESS_KEY = "your-secret-key"
```

Or configure via `aws configure`.

### 4. Apply EF Core migrations

```bash
dotnet ef database update \
  --project backend/src/IncidentPlatform.Infrastructure \
  --startup-project backend/src/IncidentPlatform.API
```

### 5. Run the API

```bash
dotnet run --project backend/src/IncidentPlatform.API
```

API: `http://localhost:5116` | Swagger: `http://localhost:5116/swagger`

### 6. Run the Frontend (optional)

```bash
cd frontend
cp .env.example .env.local   # set NEXT_PUBLIC_API_URL=http://localhost:5116
npm install
npm run dev
```

Frontend: `http://localhost:3000`

---

## Run with Docker Compose

The fastest way to start everything locally:

```bash
cp .env.example .env
# Edit .env: set SA_PASSWORD and optionally AWS credentials
docker compose up --build
```

Services started:
- **API** on `http://localhost:5116`
- **SQL Server 2022** on `localhost:1433`
- **MongoDB 7** on `localhost:27017`

The API auto-migrates the database on startup. Volumes `sqlserver_data` and `mongodb_data` persist data between restarts.

---

## Run Tests

```bash
dotnet test backend/tests/IncidentPlatform.Tests/IncidentPlatform.Tests.csproj
```

All 22 tests are pure unit tests  no database or AWS connection required.

| File | Tests | Coverage |
|------|-------|----------|
| `Services/IncidentServiceTests.cs` | 7 | Full CRUD, update/delete not-found paths |
| `Services/LogServiceTests.cs` | 4 | Field assignment, data preservation, empty list |
| `Services/AIAnalysisServiceTests.cs` | 7 | Critical/High/Medium per category, multi-keyword scoring, empty logs |
| `Controllers/IncidentControllerTests.cs` | 2 | NotFound, CreatedAtAction |
| `Controllers/AIControllerTests.cs` | 2 | NotFound on missing incident, Ok with full result |

---

## CI/CD Pipeline

The backend is automatically deployed using GitHub Actions. Every push to `main` triggers the deployment pipeline.

**Workflow:**
```
GitHub → Build (.NET 8) → Package → Upload to S3 → Deploy to AWS Elastic Beanstalk
```

**Technologies used:**
- GitHub Actions
- AWS Elastic Beanstalk
- Amazon S3
- .NET 8

GitHub Actions workflow: `.github/workflows/deploy-backend.yml`

**Triggers:** push to `main` with changes in `backend/src/**` or the workflow file itself.

**Pipeline steps:**

1. `build` job  `dotnet publish -r linux-x64 --self-contained true`  artifact ZIP
2. `deploy` job  upload ZIP to S3  create EB application version  `update-environment`  wait

**Required GitHub Secrets:**

| Secret | Description |
|---|---|
| `AWS_ACCESS_KEY_ID` | AWS IAM access key |
| `AWS_SECRET_ACCESS_KEY` | AWS IAM secret key |
| `EB_APP_NAME` | Elastic Beanstalk application name |
| `EB_ENV_NAME` | Elastic Beanstalk environment name |
| `EB_S3_BUCKET` | S3 bucket used by Elastic Beanstalk |

Configure at: `Settings  Secrets and variables  Actions  New repository secret`

---

## AI Analysis Logic

`AIAnalysisService` uses keyword scoring across the last 20 log messages:

1. **Category detection**  scores each log against 4 keyword dictionaries. The highest-scoring category wins.
2. **Severity escalation**  within the winning category, checks for critical-tier keywords first, then high-tier.
3. **Diagnostic context**  `(category, severity)` pairs map to specific root cause explanations, fix suggestions, and test recommendations.

| Category | Example keywords |
|----------|-----------------|
| `database` | `deadlock`, `connection pool`, `query timeout`, `replica`, `mongo` |
| `infrastructure` | `out of memory`, `oom`, `pod crash`, `kubernetes`, `health check` |
| `authentication` | `unauthorized`, `401`, `jwt`, `invalid credentials`, `oauth` |
| `network` | `timeout`, `connection refused`, `502`, `503`, `dns`, `upstream` |

This is a heuristic placeholder designed to be replaced with an LLM-backed implementation (e.g. Azure OpenAI or OpenAI API).

---

## CORS Configuration

Allowed origins are controlled via `appsettings.json`:

```json
"AllowedOrigins": [
  "http://localhost:3000",
  "https://localhost:3000",
  "http://localhost:5173",
  "https://localhost:5173",
  "https://incidentplatform.space",
  "https://www.incidentplatform.space"
]
```

Add production frontend URLs via environment variables:

```
AllowedOrigins__4=https://incidentplatform.space
AllowedOrigins__5=https://www.incidentplatform.space
```

---

## AI-Assisted Development

This project was built using **GitHub Copilot** inside VS Code as the primary AI development assistant.

- **Scaffolding**  Full 5-project Clean Architecture layout, domain models, EF Core and MongoDB contexts, AWS S3 service, generated from natural language.
- **Services & repositories**  Service interfaces, repository implementations, controller actions  generated and iterated conversationally.
- **AI analysis logic**  The `AIAnalysisService` keyword scoring system designed collaboratively with Copilot.
- **Unit tests**  All 22 xUnit tests (Moq + FluentAssertions) written with Copilot, including edge cases.
- **DevOps & security**  Dockerfile, docker-compose, GitHub Actions workflow, `.gitignore` hardening, CORS policy  all authored with Copilot.

The entire workflow was conversational  each feature described in natural language, Copilot handled code generation, validation, and commit preparation.
