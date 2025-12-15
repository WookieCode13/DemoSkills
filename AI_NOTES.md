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

---

## 2025-12-05 - ECS Harness Playbook
- Keep default VPC alive; if missing, recreate via "Create default VPC"; ensure two public subnets auto-assign IPv4, main route 0.0.0.0/0 -> IGW; use `demo-employeeapi-sg` (80/443 open for demo) instead of default SG.
- Manual AWS prep: ECR repo `demoskills` (shared; scan-on-push; lifecycle to prune); roles `demoskillsapi-ecs-execution` (AmazonECSTaskExecutionRolePolicy) and `demoskillsapi-task-role` (perms TBD); CloudWatch log groups `/ecs/employeeapi` etc.; ECS cluster `demoskills-001-ecs` (Fargate, default VPC); task def `employeeapi-task` with placeholder `public.ecr.aws/amazonlinux/amazonlinux:latest`, port 8080, awslogs.
- Service stub: `employeeapi-svc` on that cluster, assign public IP, two public subnets, SG `demo-employeeapi-sg`, desired count 0/1; ALB later if needed.
- Harness pipeline sketch: connectors (Git, AWS/ECR); build stage login -> docker build `docker/services/EmployeeAPI.Dockerfile` -> push `employeeapi-latest` plus stable tag output as `IMAGE_TAG`; deploy updates task definition image, scales service to 1 using the roles; pause stage scales to 0; optional cleanup of old images and ALB/health check notes; keep stable tag for rollbacks even when defaulting to latest.
- Cost/ops: keep ECR/ECS/VPC in place for speed; nightly pause by setting desired count to 0; single-repo ECR strategy for all services.

---

## 2025-12-05 - Harness Connectors Checklist
- Git: GitHub (or Git) connector using PAT with repo scope; shallow clone ok; repo-only scope.
- AWS: connector with access keys or role; perms include ECR push (GetAuth, Upload*, PutImage), ECS deploy (Describe*, RegisterTaskDefinition, UpdateService), iam:PassRole for execution/task roles, and logs create/put if Harness makes groups.
- ECR registry: ECR-type connector using same AWS connector; registry URL for `demoskills`; verify `aws ecr get-login-password` works from runner.
- Delegate/runner: place in same region/VPC if possible; ensure outbound 443 and NAT/VPC endpoints if private; role/keys carry above perms.
- Secrets: store PAT/AWS creds/registry creds as Harness secrets; name predictably (e.g., aws_demoskills_creds, github_pat_demoskills).
- Quick checks: `aws sts get-caller-identity`, `aws ecr describe-repositories --repository-names demoskills`, `aws ecs list-clusters`; run Harness connector tests for Git and ECR.
