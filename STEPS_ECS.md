# Deployment Steps (ECS Fargate + Harness)

Goal: containerize the EmployeeAPI (and future services) with Docker, push images to ECR, and deploy to AWS ECS using Fargate with Harness pipelines. Keep the footprint low-cost, fully reproducible, and easy to tear down.

## Phase 0 – Prerequisites & Decisions

1. **AWS accounts/roles**: confirm an AWS account with permissions to manage ECR, ECS, IAM, VPC, and Route 53. Decide on a primary region (us-east-1 unless a different one is already standard).
2. **Networking**: reuse the default VPC + public subnets initially. Long term we can create dedicated subnets and security groups via IaC.
3. **Container registry**: choose ECR for production and optionally Docker Hub/GitHub Container Registry for local experiments.
4. **Harness access**: ensure Harness has connections set up for GitHub, Docker/ECR, and AWS (either an IAM user with access keys or a cross-account role).

## Phase 1 – Dockerize the APIs

1. For each API (start with `EmployeeAPI`):
   - Add a `Dockerfile` targeting `linux-x64`, multi-stage (restore/build/publish + runtime stage).
   - Include environment-variable driven config and expose port `8080` (or similar) consistently.
   - Add `commands/build-docker.ps1` helper scripts for local builds/test runs.
2. Create a root-level `docker/` folder with shared compose files and `.dockerignore`.
3. Validate locally: `docker build`, `docker run -p 8080:8080 employeeapi:local` and hit Swagger.
4. Once stable, standardize tags: `demo/<service>:<git-sha>` for dev, `release-<version>` for prod.

## Phase 2 – AWS Building Blocks

1. **ECR repositories**: create `employeeapi` (and placeholders for future APIs). Enable scan-on-push.
2. **IAM roles/policies**:
   - Task execution role allowing `ecr:GetAuthorizationToken`, `logs:CreateLogStream`, etc.
   - Task role for the API (least privilege, e.g., S3 or DB access later).
3. **CloudWatch logs**: define log groups `/ecs/employeeapi`.
4. **Security groups**:
   - `employeeapi-alb-sg`: inbound 80/443 from `0.0.0.0/0`.
   - `employeeapi-service-sg`: inbound from the ALB SG only.
5. **Load balancer (optional initial)**: create an Application Load Balancer with HTTP listener -> target group (IP type) pointing at ECS tasks. Keep idle timeout low until HTTPS is added.

## Phase 3 – ECS Fargate Configuration

1. Create an ECS cluster `demo-ecs` (Fargate only).
2. Define task definition `employeeapi-task`:
   - Launch type `FARGATE`.
   - CPU/memory `0.5 vCPU / 1 GB` (adjust later).
   - Container referencing the ECR image, port mapping 8080, log configuration to CloudWatch.
   - Environment variables for ASP.NET configs (ASPNETCORE_URLS, ConnectionStrings, etc.).
3. Create service `employeeapi-svc`:
   - Desired count 1 (cheap baseline), enable auto-scaling hooks for later.
   - Attach to the ALB target group if using load balancing, otherwise assign a public IP via awsvpc.
   - Place service in two public subnets (or private + NAT when ready).
4. Verify deployment manually once to ensure logs flow and the health checks pass.

## Phase 4 – Harness Pipeline

1. **Build stage**:
   - Trigger on `main` branch merges (and optionally feature branches).
   - Steps: checkout, run tests, `docker build`, `docker tag`, `docker push` to ECR.
   - Capture metadata (image digest, git SHA) as pipeline outputs.
2. **Deploy stage**:
   - Harness ECS Fargate module: reference the cluster, service, and task definition.
   - Use variables for CPU/memory, desired count, and image tag.
   - Add verification steps (Harness CV or manual approval) before prod.
3. **Teardown flow**:
   - Separate Harness pipeline or stage that scales the service to zero or deletes Stack (via Terraform/CloudFormation script) to keep costs down.
4. **Secrets**: store AWS creds + Docker registry credentials in Harness secret manager.

## Phase 5 – Automation & Future Enhancements

1. Convert manual AWS setup into Terraform or CloudFormation under `infra/`.
2. Expand pipelines to handle additional APIs (Python Company API, Tax Calculator API).
3. Add automated smoke tests post-deploy (e.g., `pwsh commands/smoke-employeeapi.ps1` called from Harness).
4. Integrate Route 53 and ACM for HTTPS once ALB is in place.
5. Implement ECS Service Auto Scaling policies once load testing begins.

## Daily Routine

1. Developer pushes code -> Harness builds/pushes image -> ECS deploys automatically to dev.
2. For demo resets, run the teardown pipeline (delete service or entire stack) and re-run the deploy pipeline when needed.
3. Monitor CloudWatch logs and Harness execution logs to diagnose issues quickly.

This document replaces the EC2/Beanstalk step logs. Refer to `STEPS_EC2_old.md` and `STEPS_BEANSTALK_old.md` for historical notes.
