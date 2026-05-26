# SIS

SIS (Student Information System)
================================

Project Overview ( Describtion & How To USe )
----------------
SIS is a Student Information System built with .NET 10 and Entity Framework Core. It follows a clean layered architecture with separate Domain, Application, Infrastructure, and API projects. The system manages students, courses, and enrollments using a repository and unit-of-work pattern, with JWT-based authentication and role-based authorization.

The solution also includes a **Logging microservice** that captures all system activity asynchronously via a message broker, completely decoupled from the main SIS application.

---

Repository Structure
--------------------

```
SIS/
├── Microservices/
│   └── Logging/
│       ├── Logging.API/           - Logging HTTP API + RabbitMQ consumer
│       ├── Logging.Application/   - DTOs, service interfaces, AutoMapper profiles
│       ├── Logging.Domain/        - Log entity, repository interface
│       └── Logging.Infrastructure - EF Core DbContext, repository, migrations
├── SIS.APIs/                      - ASP.NET Core Web API (controllers, auth)
├── SIS.Application/               - Services, DTOs, validators, mappings
├── SIS.Contracts/                 - Shared message contract (LogMessage)
├── SIS.Domain/                    - Domain entities and repository interfaces
├── SIS.Infrastructure/            - EF Core contexts, repositories, seeding
├── Dockerfile.sis                 - Docker image for SIS.APIs
├── Dockerfile.logging             - Docker image for Logging.API
├── docker-compose.yml             - Runs the full system with one command
└── SIS.slnx
```

---

Prerequisites
-------------
- .NET 10 SDK
- Docker Desktop
- SQL Server — **only needed for the manual run approach**; Docker Compose brings its own SQL Server container
- Optional: `dotnet-ef` tool for migrations

  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

Running the Project
-------------------

There are two ways to run the system. **Docker Compose is the recommended approach** and requires no local SQL Server or manual setup.

---

### Option 1 — Docker Compose (Recommended)

One command starts everything: RabbitMQ, SQL Server, the SIS API, and the Logging microservice.

```bash
docker-compose up --build
```

That's it. Docker Compose handles startup ordering automatically — RabbitMQ and SQL Server are confirmed healthy before either application starts.

| Service | URL |
|---|---|
| SIS API + Swagger | http://localhost:5242/swagger |
| Logging API + Swagger | http://localhost:5113/swagger |
| RabbitMQ Management UI | http://localhost:15672 (guest / guest) |

To stop everything:

```bash
docker-compose down
```

To stop and also delete the database volumes (full reset):

```bash
docker-compose down -v
```

> **Note on database migrations with Docker Compose:** The containers run the published application — they do not auto-migrate. After `docker-compose up --build`, run the migrations manually once against the SQL Server container (see the Migrations section below, using `Server=localhost,1433;User Id=sa;Password=SIS_StrongPass123!` as the connection string).

---

### Option 2 — Manual (dotnet run)

Use this if you prefer running the APIs locally against your own SQL Server instance.

**Step 1 — Start RabbitMQ in Docker** (must be running before starting either app):

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

If the container already exists from a previous run:

```bash
docker start rabbitmq
```

**Step 2 — Apply database migrations** (from the solution root):

```bash
# Main SIS databases
dotnet ef database update --project SIS.Infrastructure --startup-project SIS.APIs

# Logging microservice database
dotnet ef database update \
  --project Microservices/Logging/Logging.Infrastructure/Logging.Infrastructure.csproj \
  --startup-project Microservices/Logging/Logging.API/Logging.API.csproj
```

**Step 3 — Run both applications** (two separate terminals):

```bash
# Terminal 1
cd Microservices/Logging/Logging.API
dotnet run
```

```bash
# Terminal 2
cd SIS.APIs
dotnet run
```

| Service | URL |
|---|---|
| SIS API + Swagger | http://localhost:5242/swagger |
| Logging API + Swagger | http://localhost:5113/swagger |
| RabbitMQ Management UI | http://localhost:15672 (guest / guest) |

---

Databases
---------
The solution uses three separate SQL Server databases:

| Database | Purpose | Connection String Key |
|---|---|---|
| `SIS_BusinessDB` | Students, courses, enrollments | `DefaultConnection` in SIS.APIs |
| `SIS_IdentityDB` | ASP.NET Identity users and roles | `DefaultConnection0` in SIS.APIs |
| `SIS_LogDb` | All log entries from the microservice | `Default` in Logging.API |

For the manual run, update the connection strings in:
- `SIS.APIs/appsettings.json`
- `Microservices/Logging/Logging.API/appsettings.json`

For Docker Compose, connection strings are injected automatically via environment variables — no manual editing required.

---

Authentication
--------------
The API uses JWT Bearer authentication.

1. Register a student via `POST /api/auth/register`
2. Log in via `POST /api/auth/login` — you will receive a token
3. In Swagger, click **Authorize** and enter: `Bearer <your_token>`
4. Staff accounts are created by an Admin via `POST /api/auth/staff`

Roles seeded automatically on startup: `Admin`, `Staff`, `Student`.

---

Microservice Architecture — How Logging Works
----------------------------------------------

### The Two Approaches We Considered

When connecting SIS to the Logging microservice, there are two possible approaches:

**Approach A — Direct HTTP Call (the simpler option)**
SIS calls `POST http://localhost:5113/api/logs` directly using `HttpClient` every time something needs to be logged. This works, but has serious drawbacks:
- If the Logging service is down, the log is permanently lost — or worse, the exception propagates and disrupts the main SIS operation.
- SIS becomes tightly coupled to Logging, needing to know its URL and handle its failures.
- Every log call adds latency to the user-facing request because SIS must wait for the HTTP response.

**Approach B — Async Message Queue via RabbitMQ (what we built)**
SIS publishes a `LogMessage` to RabbitMQ and immediately continues. The Logging microservice listens on the queue and processes messages independently. We chose this because:
- **Fault tolerance** — if Logging is offline, RabbitMQ holds the messages in the queue and delivers them the moment Logging comes back. Nothing is ever lost.
- **Zero coupling** — SIS has no knowledge of the Logging service. It only knows about the shared `LogMessage` contract in `SIS.Contracts`. You could replace the entire Logging microservice and SIS would not require a single change.
- **No latency impact** — publishing to a queue is fire-and-forget. The user request completes instantly regardless of what Logging does with the message.
- **Correct responsibility** — logging is a side effect, not a core concern. It should never be able to slow down or break the main application.

We were fully aware that Approach A would have been a simpler and acceptable solution. We chose Approach B because it reflects how production microservice systems actually handle cross-service communication for non-critical side effects.

### How the Pipeline Works

```
User Action (e.g. POST /api/students)
        │
        ▼
SIS.APIs Controller
        │
        ▼
SIS.Application Service (StudentService / CourseService / AuthController)
        │  publishes:  new LogMessage("Student created", "user-id")
        ▼
MassTransit Publisher  ──────────────────────────────────►  RabbitMQ
(inside SIS.APIs process)                                   Queue: log-message
                                                                 │
                                                                 ▼
                                                      Logging.API (separate process)
                                                      LogMessageConsumer.Consume()
                                                                 │
                                                                 ▼
                                                      LogService.CreateLogAsync()
                                                                 │
                                                                 ▼
                                                      SQL Server — SIS_LogDb
```

### What Gets Logged

| Source | Events Logged |
|---|---|
| `AuthController` | Register success/failure, login success/failure, staff creation, rollbacks |
| `StudentService` | Create, update, delete, enroll, unenroll |
| `CourseService` | Create, update, delete |

### Shared Contracts Project

`SIS.Contracts` contains only the `LogMessage` record:

```csharp
namespace SIS.Contracts;

public record LogMessage(string Message, string CreatedBy);
```

Both `SIS.Application` and `Logging.API` reference this project. Neither references the other.

### The Logging API HTTP Endpoint

`Logging.API` still exposes `POST /api/logs` for direct HTTP access. In normal operation this endpoint is not used — all logs arrive through RabbitMQ. It is retained as a manual override for debugging or admin purposes.

---

Database Migrations Reference
------------------------------

Adding a new migration to the main SIS:

```bash
dotnet ef migrations add <MigrationName> \
  --project SIS.Infrastructure \
  --startup-project SIS.APIs
```

Adding a new migration to the Logging microservice:

```bash
dotnet ef migrations add <MigrationName> \
  --project Microservices/Logging/Logging.Infrastructure/Logging.Infrastructure.csproj \
  --startup-project Microservices/Logging/Logging.API/Logging.API.csproj
```

---

Notes
-----
- The solution uses a repository and unit-of-work pattern. See `SIS.Infrastructure/Repositories` for implementations and `SIS.Domain/Common/Interfaces` for interfaces.
- The Identity database (`SIS_IdentityDB`) and the business database (`SIS_BusinessDB`) are intentionally separate. `SecurityDbContext` handles Identity; `ApplicationDbContext` handles domain data.
- MassTransit version `8.3.2` is used consistently across all projects. Do not mix versions.
- The `SIS.Contracts` package references MassTransit so the `LogMessage` record is recognized as a proper message type during serialization.
- When running via Docker Compose, the RabbitMQ host is injected via the `RabbitMQ__Host` environment variable. Both `Program.cs` files read `builder.Configuration["RabbitMQ:Host"] ?? "localhost"` so they work correctly in both Docker and local `dotnet run` scenarios.
