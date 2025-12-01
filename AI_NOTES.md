# AI Notes — DemoSkills

This file logs ideas, questions, and insights from AI interactions.

---

## 2025-11-04 — Initial Setup
- Decided to start with Markdown files before GitHub repo.
- Tools: ChatGPT for design, Copilot for inline coding.
- Project name under consideration: **DemoSkills** vs **DemoProject**.

---

## 2025-11-05 — Planning Harness Integration
Goal: have Harness deploy to AWS and also run a teardown pipeline.
- Harness YAMLs will live in `/harness/`.
- Dockerfiles will live in `/docker/`.
- Each API will be in its own subfolder.

---

## 2025-11-06 - AI Workflow Goals
- Use Markdown to guide Copilot understanding.
- Use workspace for multiple APIs.
- Combine C# and Python projects in one repo.

---

## 2025-11-19 - ECS/Harness Pivot
- Chose ECS Fargate (over EKS/EC2) for cost/control balance; will keep deployments repeatable and cheap.
- Keep default VPC alive; if recreating, use AWS console “Create default VPC.” Default SG is self-referencing only—leave it unused; attach custom SG (`demo-employeeapi-sg`) for public 80/443 as needed.
- No local Docker on Windows; rely on Harness/remote Linux runner to build/push images. Dockerfile lives at `docker/services/EmployeeAPI.Dockerfile`.
- Harness focus: connectors (GitHub/AWS/registry), build stage (login/build/push), deploy stage (ECS), teardown stage (scale down/delete, optional ECR cleanup). Registry can be ECR (with lifecycle policy) or external if cost is a concern.
