# Deployment Steps (Learning Log)

Goal: deploy `EmployeeAPI` to AWS using Harness, with infra you can spin up and tear down. We’ll grow this in small, clear steps you can follow manually.

## Phase 1 — Containerize and run locally

- Prereqs: install `.NET 8 SDK`, `Docker Desktop`.
- Build the API locally:
  - `dotnet build apis/EmployeeAPI/EmployeeAPI.csproj`
- Add container (done in repo): `apis/EmployeeAPI/Dockerfile`.
- Build and run the image locally:
  - `docker build -t employee-api:local apis/EmployeeAPI`
  - `docker run --rm -p 8080:8080 employee-api:local`
  - Test: open `http://localhost:8080/swagger`.

## Phase 2 — Harness bootstrap

- In Harness: create an `Organization` and `Project` for DemoSkills.
- Install a Harness Delegate (Kubernetes or Docker) in your environment that can reach GitHub and AWS.
- Add Connectors:
  - Git: GitHub connector to this repo (`WookieCode13/DemoSkills`).
  - Cloud: AWS connector using an IAM user or role (use least-privilege; start with AdministratorAccess for learning, then tighten).
  - Artifact: ECR connector (or Docker Hub for a first pass).
- Add Secrets in Harness (Secret Manager):
  - `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` (or use an assumed role via the AWS connector).
  - Any app secrets later (none required yet).

## Phase 3 — Pipelines as Code skeleton

- This repo includes `.harness/api-ci.yaml` as a minimal pipeline skeleton for: build image, push to ECR, and (later) deploy.
- Fill in the placeholders in that YAML in the Harness UI or via code review:
  - `accountId`, `orgIdentifier`, `projectIdentifier`.
  - connectors: `harnessConnectorRef`, `awsConnectorRef`, `ecrConnectorRef`.
- First run: execute the pipeline just through the `build` step to verify CI runs.

## Phase 4 — Infra as Code (spin up / tear down)

- This repo includes `infra/terraform/ecr` to create an ECR repo for images.
- To apply (creates ECR):
  - Set env (or accept defaults): `setx AWS_REGION us-east-1`, `setx ECR_REPO_NAME employee-api`
  - In PowerShell: `infra/terraform/ecr/apply.ps1`
- To destroy (cleans up ECR):
  - `infra/terraform/ecr/destroy.ps1`
- Later we’ll add ECS Fargate + ALB with the same apply/destroy model.

## Phase 5 — Secrets and config

- Keep secrets out of git; reference them from Harness secrets or AWS Parameter Store/Secrets Manager.
- App config (e.g., connection strings) will be injected via environment variables in the container task definition.

## Next

- You can start at Phase 1 and confirm the container works locally.
- Once ready, set the Harness connectors/identifiers and try the CI pipeline.
- We’ll iterate: add ECR push, then ECS deploy, then teardown.

