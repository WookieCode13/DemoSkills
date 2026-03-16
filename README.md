# DemoSkills

DemoSkills is a personal learning and portfolio project for practicing modern backend development, DevOps, cloud deployment, and AI-assisted coding workflows.

It is intentionally a work in progress. The repo is used to explore architecture ideas, compare implementation approaches, and document what I am learning while building a multi-service system over time.

## Project Status

- **Phase**: Multi-service API development and deployment practice
- **Current Work**: Authorization, tenant routing, and deployment workflow cleanup
- **Next Milestone**: Stabilize shared auth/role setup and continue tenant-aware employee persistence

This is not presented as a production-ready product. It is a practice environment for building skills across APIs, infrastructure, authentication, testing, and delivery workflows.

The project combines:

- **C# and Python APIs**
- **AWS deployment (Lambda + ECS Fargate)**
- **Harness CI/CD pipelines**
- **Docker containers**
- **Git versioning**
- **Kubernetes (future)**
- **AI-assisted development using ChatGPT and GitHub Copilot**

## Quick Links

- [Technical Stack](./TECH_STACK.md)
- [Project Structure](./STRUCTURE.md)
- [Archived AI Notes](./docs/archive/AI_NOTES.md)
- [Archived Setup Notes](./docs/archive/SETUP_NOTES.md)
- [Archived Project Plan](./docs/archive/PLAN.md)
- [Archive Index](./docs/archive/README.md)

---

## 🔧 Current Focus

1. Clean up shared authorization roles, permissions, and migration flow.
2. Continue tenant-aware employee persistence and company-driven tenant setup.
3. Keep JWT/Cognito auth wiring consistent across APIs and local docker.
4. Expand deployment automation and document the current AWS path more clearly.
5. Continue filling in practical end-to-end workflows across APIs and supporting services.

---

## 🧠 About the Approach

This project uses AI tools to help design, build, review, and automate code as part of the learning process:

- **ChatGPT (GPT-5)** – for architecture, planning, and documentation.
- **GitHub Copilot Chat** – for inline coding and local automation.

Older planning, setup, and experiment notes are kept under [`docs/archive/`](./docs/archive/README.md) instead of the repo root.

---

## 🚀 Planned Stack

| Layer           | Technology                    |
| --------------- | ----------------------------- |
| Backend APIs    | C# (.NET 8), Python (FastAPI) |
| Infrastructure  | AWS (ECS/Fargate, Lambda, RDS), Docker |
| CI/CD           | Harness                       |
| Auth            | AWS Cognito (JWT/OIDC)        |
| Testing         | Unit + BDD (SpecFlow, Pytest) |
| Version Control | GitHub                        |
| Frontend        | Dashboard app (iterative)     |

---

## Local Docker (Linux)

Local compose uses subdomain routing to mirror AWS. Map your LAN IP to a local domain (for example, `longranch.wookie`) in your hosts file or local DNS, then run the stack with env vars.

Required env vars (no secrets in repo):
- `DEMOSKILLS_POSTGRES_DB`
- `DEMOSKILLS_POSTGRES_USER`
- `DEMOSKILLS_POSTGRES_PASSWORD`
- `DEMOSKILLS_JWT_AUTHORITY`
- `DEMOSKILLS_JWT_CLIENT_ID`

Example hosts entries:
- `longranch.wookie`
- `employee.longranch.wookie`
- `company.longranch.wookie`
- `pay.longranch.wookie`
- `tax.longranch.wookie`
- `report.longranch.wookie`

Start:
```bash
docker compose up --build -d
```

## Folder Overview

- `/apis` – backend API projects
- `/dashboard` – UI project and static assets
- `/docker` – Dockerfiles and build scripts
- `/lambdas` – serverless experiments and workflows
- `/harness` – Harness pipelines, YAML configs
- `/scripts` – local setup and utility scripts
- `/Shared` – shared auth, security, and cross-service code

---

## 🤖 AI Development Notes

- This project actively uses AI-assisted development tools
- See [AI_NOTES.md](./AI_NOTES.md) for detailed AI development guidelines
- Key AI integrations:
  - GitHub Copilot for real-time code assistance
  - ChatGPT for architecture and planning
  - Custom prompts for project-specific tasks

## 📜 License

This project is licensed under the terms in [LICENSE](./LICENSE).
