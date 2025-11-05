# DemoSkills

DemoSkills is a hands-on learning and showcase project that demonstrates modern backend development, DevOps, and AI-assisted coding workflows.

## Project Status
- **Phase**: Initial Setup
- **Last Updated**: November 5, 2025
- **Next Milestone**: Basic API Setup

The project combines:
- **C# and Python APIs**
- **AWS deployment (Lambda + EC2/Web)**
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
1. Create a simple API.
2. Set up Harness deployment to AWS (with budget awareness <$20/month).
3. Add teardown pipeline to remove AWS resources automatically.
4. Add unit and BDD tests.
5. Add logging and token-based security.

---

## 🧠 About the Approach
This project uses AI tools to help design, build, and automate code:
- **ChatGPT (GPT-5)** – for architecture, planning, and documentation.
- **GitHub Copilot Chat** – for inline coding and local automation.

---

## 🚀 Planned Stack
| Layer | Technology |
|-------|-------------|
| Backend APIs | C# (.NET 8), Python (FastAPI) |
| Infrastructure | AWS (Lambda, EC2), Docker |
| CI/CD | Harness |
| Auth | Token-based (JWT) |
| Testing | Unit + BDD (SpecFlow, Pytest) |
| Version Control | GitHub |
| Frontend | TBD (later phase) |

---

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
