# DemoSkills

DemoSkills is a hands-on learning and showcase project that demonstrates modern backend development, DevOps, and AI-assisted coding workflows.

## Project Status

- **Phase**: API + AWS deployment hardening
- **Last Updated**: February 16, 2026
- **Next Milestone**: Employee tenant schema routing and security integration

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
- [AI Development Guide](./AI_NOTES.md)
- [Setup Instructions](./SETUP_NOTES.md)
- [Project Roadmap](./PLAN.md)

---

## 🔧 Current Focus

1. Implement tenant-aware employee persistence model (schema strategy).
2. Wire company lifecycle events to employee tenant setup.
3. Add Cognito-backed token security for APIs.
4. Add TypeScript UI path in deployment workflow.
5. Add first practical Lambda workflow.

---

## 🧠 About the Approach

This project uses AI tools to help design, build, and automate code:

- **ChatGPT (GPT-5)** – for architecture, planning, and documentation.
- **GitHub Copilot Chat** – for inline coding and local automation.

---

## 🚀 Planned Stack

| Layer           | Technology                    |
| --------------- | ----------------------------- |
| Backend APIs    | C# (.NET 8), Python (FastAPI) |
| Infrastructure  | AWS (Lambda, EC2), Docker     |
| CI/CD           | Harness                       |
| Auth            | AWS Cognito (JWT/OIDC)        |
| Testing         | Unit + BDD (SpecFlow, Pytest) |
| Version Control | GitHub                        |
| Frontend        | TBD (later phase)             |

---

## Local Docker (Linux)

Local compose uses subdomain routing to mirror AWS. Map your LAN IP to a local domain (for example, `longranch.wookie`) in your hosts file or local DNS, then run the stack with env vars.

Required env vars (no secrets in repo):
- `DEMOSKILLS_POSTGRES_DB`
- `DEMOSKILLS_POSTGRES_USER`
- `DEMOSKILLS_POSTGRES_PASSWORD`

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
- `/docker` – Dockerfiles and build scripts
- `/harness` – Harness pipelines, YAML configs
- `/infrastructure` – AWS/K8s setup scripts

---

## 🤖 AI Development Notes

- This project actively uses AI-assisted development tools
- See [AI_NOTES.md](./AI_NOTES.md) for detailed AI development guidelines
- Key AI integrations:
  - GitHub Copilot for real-time code assistance
  - ChatGPT for architecture and planning
  - Custom prompts for project-specific tasks

## 📜 License
