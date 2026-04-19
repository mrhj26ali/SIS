# SIS

SIS (Student Information System)
================================

Project overview
----------------
SIS is a simple Student Information System implemented with .NET 10 and Entity Framework Core. It follows a layered clean-architecture with separate domain, application, infrastructure, and API projects. The solution manages students, courses, and enrollments using repositories and a unit-of-work pattern.

Repository structure
--------------------
- `SIS.Domain` - Domain entities and repository interfaces.
- `SIS.Application` - DTOs and application-level services.
- `SIS.Infrastructure` - EF Core `DbContext`, repository implementations, and migrations.
- `SIS.APIs` - ASP.NET Core Web API project and configuration.

Prerequisites
-------------
- .NET 10 SDK
- SQL Server (local or remote)
- Optional: `dotnet-ef` tool for migrations (`dotnet tool install --global dotnet-ef`)

Configuration
-------------
- The main connection string is in `SIS.APIs/appsettings.json`. Update the `Default` connection string to point to your SQL Server instance.
- Development-specific settings can be found in `SIS.APIs/appsettings.Development.json`.

Build and run
-------------
From the repository root:

- Build solution:

  `dotnet build`

- Run the API (from repo root):

  `dotnet run --project SIS.APIs/SIS.APIs.csproj`

Database migrations
-------------------
When the EF Core model changes, create and apply migrations using the following commands from the repository root:

- Add a migration:

  `dotnet ef migrations add <MigrationName> --project SIS.Infrastructure --startup-project SIS.APIs`

- Apply migrations to the database:

  `dotnet ef database update --project SIS.Infrastructure --startup-project SIS.APIs`

Notes
-----
- The solution uses a repository and unit-of-work pattern. See `SIS.Infrastructure/Repositories` for implementations and `SIS.Domain/Common/Interfaces` for interfaces.
- If you change connection strings or provider settings, ensure `ApplicationDbContext` in `SIS.Infrastructure/Persistence/Contexts` is configured accordingly.