# Tech Stack

## Core Platform

| Area | Current Stack |
| --- | --- |
| Backend (.NET) | C#, .NET 8, ASP.NET Core Web API |
| Backend (Python) | Python 3.12, FastAPI |
| Frontend | Lightweight dashboard app served through nginx |
| Database | PostgreSQL |
| Auth | JWT validation with AWS Cognito-oriented configuration |
| Containers | Docker, Docker Compose |
| Deployment | AWS ECS/Fargate, ALB, ECR, RDS |
| CI/CD | Harness |
| AI Tooling | ChatGPT and GitHub Copilot |

## Backend

### .NET APIs

- Runtime: `.NET 8`
- Framework: `ASP.NET Core Web API`
- Common packages and tooling:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Serilog`
  - `Swashbuckle.AspNetCore`
  - `FluentMigrator`
  - `xUnit`

### Python APIs

- Runtime: `Python 3.12`
- Framework: `FastAPI`
- Common packages and tooling:
  - `fastapi`
  - `uvicorn`
  - `pytest`
  - `SQLAlchemy` in Python service data access paths

## Frontend

- Current UI: simple dashboard app in [`dashboard/`](./dashboard)
- Delivery: served through nginx in local Docker and mirrored in hosted routing flows
- Role in the project: lightweight service visibility and integration surface, not a polished product frontend

## Data and Migrations

- Database: shared PostgreSQL
- .NET migrations: `FluentMigrator`
- Python persistence patterns: SQLAlchemy-based service code
- Shared auth data model: roles, permissions, user access mappings under the `_auth` schema

## Security

- Authentication: bearer JWT validation
- Identity provider direction: AWS Cognito
- Authorization direction: shared role/permission model with API policy checks
- Shared security modules:
  - `.NET`: [`Shared/Security/dotnet/Shared.Security.Net`](./Shared/Security/dotnet/Shared.Security.Net)
  - `Python`: [`Shared/Security/py/shared_security_py`](./Shared/Security/py/shared_security_py)

## Infrastructure

- Docker images:
  - `mcr.microsoft.com/dotnet/aspnet:8.0`
  - `python:3.12-slim`
  - `nginx:1.27-alpine`
- AWS services in active use or active practice flow:
  - `ECS/Fargate`
  - `ALB`
  - `ECR`
  - `RDS PostgreSQL`
  - `Cognito`
- Additional area of experimentation:
  - `Lambda`

## Development Workflow

- Editor: VS Code
- Version control: Git + GitHub
- Branching: feature branches merged back to main
- Local orchestration: `docker compose`
- API testing: Swagger, `.http` request files, and unit tests
- Documentation style: lightweight Markdown docs with older notes archived under [`docs/archive/`](./docs/archive/README.md)

## Configuration

- Local container setup uses environment variables for database and JWT settings.
- .NET services use `appsettings.*` plus environment overrides.
- Python services use environment-based configuration.
- Deployment secrets are expected to live outside the repo.
