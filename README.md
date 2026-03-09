# AI Incident Management Platform

[![CI](https://github.com/robre8/AI-Incident-Management-Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/robre8/AI-Incident-Management-Platform/actions/workflows/ci.yml) [![Deploy Backend](https://github.com/robre8/AI-Incident-Management-Platform/actions/workflows/deploy-backend.yml/badge.svg)](https://github.com/robre8/AI-Incident-Management-Platform/actions/workflows/deploy-backend.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Production-grade incident management system built with **AI-first engineering practices**. Full-stack C#/.NET 8 backend (Clean Architecture) with a React 19 SPA frontend deployed across AWS. Polyglot persistence (SQL Server + MongoDB), S3 file storage, AI-powered root cause analysis, and **118 automated tests** across both stacks.

Built entirely through AI-assisted development using GitHub Copilot — from architecture scaffolding to unit test generation, infrastructure configuration, and CI/CD pipeline authoring.

> **Live:** [incidentplatform.space](https://incidentplatform.space) | **API:** [api.incidentplatform.space/swagger](https://api.incidentplatform.space/swagger)

---

## Key Engineering Decisions

- **Clean Architecture with strict dependency inversion** — 5-project solution with inside-out dependency rule. Domain has zero external references; Infrastructure implements interfaces defined in Application.
- **Polyglot persistence** — SQL Server (EF Core) for relational incident data, MongoDB Atlas for high-throughput log ingestion. Each store chosen for its strengths, not forced into one model.
- **Graceful degradation** — MongoDB or AI failures return 503 with contextual error messages. The UI stays functional even when secondary services are down.
- **Status lifecycle with server-side validation** — Incident status transitions are normalized and validated against an allowlist (`Open`, `In Progress`, `Resolved`, `Closed`). Invalid values throw `ArgumentException` before hitting the database.
- **Comprehensive test coverage (118 tests)** — Backend: 47 xUnit tests (service + controller layers). Frontend: 71 Vitest + React Testing Library tests (API client, components, full-page integration). All tests use AAA pattern, behavior-driven assertions, and clean mocking — no trivial "renders without crash" tests.
- **Dark mode with system detection** — Theme toggle persisted in `localStorage`, auto-detects `prefers-color-scheme`. All components styled with Tailwind `dark:` variants. Smooth transitions, zero layout shift.
- **CI/CD with zero-downtime deployments** — GitHub Actions builds, packages, and deploys to Elastic Beanstalk on every push to `main`. Frontend auto-deploys to Vercel on merge.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 / ASP.NET Core Web API (Clean Architecture) |
| Frontend | React 19 + Vite 7 + Tailwind CSS 4 + React Router 7 |
| Relational DB | SQL Server + EF Core 8.0 (AWS RDS) |
| Document DB | MongoDB Atlas (MongoDB.Driver 3.7) |
| File storage | AWS S3 (AWSSDK.S3 4.0) |
| API docs | Swagger / Swashbuckle 6.6 |
| Backend testing | xUnit 2.5 + Moq 4.20 + FluentAssertions 8.8 + coverlet |
| Frontend testing | Vitest 4.0 + React Testing Library + @testing-library/user-event |
| Container | Docker + docker-compose |
| CI/CD | GitHub Actions → AWS Elastic Beanstalk (backend) · Vercel (frontend) |
| Infrastructure | AWS RDS, S3, Elastic Beanstalk, Elastic IP, MongoDB Atlas |

---

## Architecture

5-project Clean Architecture — one project per layer, strict inside-out dependency rule:

```
backend/
 src/
  IncidentPlatform.Domain/
     entities/             # Incident, LogEntry (BSON), AIAnalysisResult, IncidentDTO

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
     Services/             # IAwsFileService, AwsFileService

 tests/
  IncidentPlatform.Tests/
     Services/             # IncidentServiceTests, LogServiceTests, AIAnalysisServiceTests
     Controllers/          # IncidentControllerTests, LogControllerTests,
                           # AttachmentControllerTests, AIControllerTests

incident-platform-frontend/
 src/
   api/                    # Axios client + typed API functions
   context/                # ThemeContext (dark mode provider + useTheme hook)
   components/             # IncidentCard, IncidentForm, LogForm, LogList,
                           # AttachmentUpload, AIAnalysisPanel, Layout
   pages/                  # DashboardPage, CreateIncidentPage, IncidentDetailPage
   __tests__/              # 71 tests: api/, components/, pages/
```

**Dependency rule:** Domain → Application → Infrastructure → API. Outer layers depend inward, never the reverse.

### AI Analysis Data Flow

```
POST /api/ai/analyze/{incidentId}
  → fetch Incident from SQL Server (EF Core)
  → fetch Logs from MongoDB Atlas (graceful 503 on failure)
  → AIAnalysisService: keyword scoring → category + severity
  → return AIAnalysisResult { Severity, Category, RootCause, SuggestedFix, RecommendedTests }
```

### File Upload Data Flow

```
POST /api/incidents/{incidentId}/attachments
  → validate IFormFile (null + empty checks)
  → AwsFileService.UploadFileAsync → AWS S3 PutObjectRequest
  → return { IncidentId, FileKey }
```

---

## API Endpoints

### Incidents (CRUD + Status Lifecycle)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/incident` | List all incidents |
| `GET` | `/api/incident/{id}` | Get incident by ID |
| `POST` | `/api/incident` | Create a new incident |
| `PUT` | `/api/incident/{id}` | Update an incident (including status) |
| `DELETE` | `/api/incident/{id}` | Delete an incident |

```json
// POST / PUT body
{
  "title": "Payment service down",
  "description": "Checkout flow failing for all users",
  "status": "Open"
}
```

**Status values:** `Open` → `In Progress` → `Resolved` → `Closed`. Invalid values are rejected with 400.

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

Swagger UI is available at `/swagger`.

---

## Security Considerations

This application is a **portfolio demo** intended to showcase full-stack architecture, AI-powered analysis, polyglot persistence, and CI/CD practices to recruiters and hiring managers. As such, some security trade-offs were made intentionally to keep the live demo publicly accessible and easy to evaluate:

| Area | Status | Notes |
|------|--------|-------|
| **Authentication / Login** | Not implemented | No user accounts or login flow — all endpoints are publicly accessible so recruiters can interact with the live demo without credentials. |
| **Authorization / RBAC** | Not implemented | No role-based access control — any visitor can create, read, update, and delete incidents. |
| **Rate limiting** | Not implemented | API endpoints have no throttling — acceptable for a low-traffic demo. |
| **CSRF protection** | Not implemented | Typical for SPA + API architecture with token-based auth (which would be added in production). |
| **File upload validation** | Implemented | File type whitelist, 5 MB size limit, and filename sanitization are enforced. |
| **Input validation** | Implemented | Title, description, and log fields have length constraints via `DataAnnotations`. |
| **Security headers** | Implemented | `X-Content-Type-Options`, `X-Frame-Options`, and `Referrer-Policy` headers are set on all responses. |
| **Error handling** | Implemented | Internal exception details are never exposed to clients — only generic error messages are returned. |
| **HTTPS** | Enforced | HTTPS redirection is active in all environments. |
| **Swagger** | Public | Swagger UI is available in all environments so recruiters can explore the API interactively. |
| **Docker** | Hardened | Container runs as a non-root user. |
| **Secrets management** | Proper | All credentials are managed via environment variables and GitHub Secrets — nothing is hardcoded. |

> **In a production system**, authentication (e.g. JWT / OAuth 2.0), role-based authorization, rate limiting, CSRF tokens, and audit logging would be implemented before any real data is handled.

---

## Run Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (or Docker  see below)
- MongoDB (or [MongoDB Atlas](https://www.mongodb.com/atlas) free tier)
- AWS account with an S3 bucket (for file upload feature only)

### 1. Clone the repo

```bash
git clone https://github.com/robre8/AI-Incident-Management-Platform.git
cd AI-Incident-Management-Platform
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

## Run the Frontend

```bash
cd incident-platform-frontend
cp .env.example .env        # set VITE_API_URL=http://localhost:5116
npm install
npm run dev
```

Frontend: `http://localhost:5173`

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

## Testing

### Run All Tests

```bash
# Backend (47 tests — no database or AWS connection required)
dotnet test backend/tests/IncidentPlatform.Tests

# Frontend (71 tests)
cd incident-platform-frontend && npm test
```

### Backend Test Suite (47 tests · xUnit + Moq + FluentAssertions)

| File | Tests | What it covers |
|------|-------|----------------|
| `Services/IncidentServiceTests.cs` | 14 | Full CRUD, status lifecycle (valid transitions, invalid rejection, null/empty defaults to Open) |
| `Services/LogServiceTests.cs` | 2 | Log creation field assignment, data preservation |
| `Services/AIAnalysisServiceTests.cs` | 8 | Critical/High/Medium per category, multi-keyword scoring, empty logs |
| `Controllers/IncidentControllerTests.cs` | 8 | GetAll, GetById (happy + not found), Create, Update (happy + not found), Delete (happy + not found) |
| `Controllers/LogControllerTests.cs` | 5 | GET/POST happy paths, MongoDB 503 graceful degradation, empty list |
| `Controllers/AttachmentControllerTests.cs` | 4 | Upload success, null file, empty file, response shape validation |
| `Controllers/AIControllerTests.cs` | 2 | Missing incident → 404, OK with full analysis result |

### Frontend Test Suite (71 tests · Vitest + React Testing Library + userEvent)

| File | Tests | What it covers |
|------|-------|----------------|
| `api/incidents.test.js` | 11 | All 9 API functions: correct URLs/payloads, FormData construction, error propagation |
| `components/IncidentCard.test.jsx` | 10 | Render, link href, 4 status badge color variants, missing status default, delete flow, disabled state |
| `components/IncidentForm.test.jsx` | 6 | Submit payload with status, form reset after success, loading state, native HTML validation |
| `components/LogForm.test.jsx` | 7 | Disabled when empty, enabled with content, trimmed payload, service persistence, error display, loading |
| `components/LogList.test.jsx` | 5 | Render logs, loading state, empty state, default loading=false |
| `components/AttachmentUpload.test.jsx` | 8 | File selection, upload flow, success display, no-file validation, API error (3-level fallback), loading |
| `components/AIAnalysisPanel.test.jsx` | 5 | Hint text, analyze trigger, loading, full result render, empty recommendedTests edge case |
| `components/Layout.test.jsx` | 3 | Nav links, children rendering, correct route hrefs |
| `pages/DashboardPage.test.jsx` | 5 | Loading → cards, empty state, delete w/ confirmation → removal, cancel skips API, delete failure alert |
| `pages/CreateIncidentPage.test.jsx` | 3 | Render, submit → API call → navigate to detail, loading state |
| `pages/IncidentDetailPage.test.jsx` | 8 | Load incident + logs, status update (success + failure), log creation, AI analysis (success + failure), logs 503 banner |

### Testing Philosophy

- **Behavior-driven, not implementation-driven** — tests assert what the user sees and experiences, not internal state or CSS classes
- **AAA pattern (Arrange-Act-Assert)** — every test follows strict structure with clear separation
- **`userEvent` over `fireEvent`** — simulates real user interactions (typing character-by-character, focus/blur, click sequences)
- **Semantic queries** — `getByRole`, `getByPlaceholderText`, `getByText` instead of `data-testid` — tests break when UX breaks, not when implementation changes
- **Error path coverage** — API failures, MongoDB down, network errors, empty states, invalid inputs
- **Loading state verification** — buttons disabled during async operations, spinners shown, using Promises that never resolve to test intermediate states

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

This project was built end-to-end using **GitHub Copilot** as an AI pair-programming partner — not as an autocomplete tool, but as an active collaborator in design decisions, implementation, testing, and deployment.

### How AI Was Used

| Phase | AI Contribution |
|-------|-----------------|
| **Architecture** | Full 5-project Clean Architecture scaffolding, interface definitions, dependency injection wiring — described in natural language, generated and iterated conversationally |
| **Feature development** | Service implementations, repository patterns, controller actions, DTO design with status lifecycle validation — built from defined inputs/outputs |
| **Testing (118 tests)** | AI-assisted TDD: described expected behaviors, generated test cases covering happy paths + edge cases + error scenarios. Refined prompts to improve assertion quality |
| **Frontend** | Complete React SPA with component architecture, API client, routing, state management, dark mode with system preference detection — all generated through conversational prompting |
| **DevOps** | Dockerfile, docker-compose, GitHub Actions CI/CD pipeline, CORS policy, `.gitignore` hardening, Elastic Beanstalk configuration |
| **Security** | MongoDB Atlas IP whitelisting with Elastic IP, RDS security group review, S3 bucket policy audit |
| **Debugging** | API route mismatches, CORS issues, MongoDB connectivity, deployment failures — diagnosed and resolved through AI-guided troubleshooting |

### AI-First Workflow

1. **Describe intent in natural language** — "Add status lifecycle with server-side validation for Open, In Progress, Resolved, Closed"
2. **AI generates implementation** — Service logic, DTO changes, controller updates, frontend selectors
3. **Critical review** — Validate AI output against requirements, check edge cases, verify security implications
4. **AI generates tests** — Behavior-driven test cases covering happy paths, error handling, and edge cases
5. **Iterate** — Refine prompts based on test failures or missing coverage

Every feature in this project went through this cycle. AI multiplied productivity; engineering judgment ensured quality.
