# Deployment Steps (ECS Fargate + Harness)

Goal: containerize EmployeeAPI (and future services) with Docker, push images to a registry, and deploy to ECS Fargate via Harness. Keep it low-cost, reproducible, and easy to tear down nightly.

Already done:

- EmployeeAPI created
- VPC created (see VPC notes in `STEPS_EC2_old.md` and `STEPS_BEANSTALK_old.md`)
- EmployeeAPI Dockerfile created (`docker/services/EmployeeAPI.Dockerfile`)

## Phase 0 - VPC and Security Group Setup (manual, keep default running)

This setup is needed for runnign and accessing other parts of the AWS infrastructure.

Objective: Keep the default VPC alive, but be able to recreate it quickly if it disappears.

Note: I attempted to create a pipeline to setup and teardown VPCs and other infrstruture (ECS, ECR...), but decided that was getting beyond the scope of demoing skills, and my short timeline to create a demo. The cost savings of leaving them exists or a few months

1. **Confirm default VPC exists**: Console > VPC > Your VPCs. If missing, use **Actions > Create default VPC** (this also creates subnets, routes, IGW).
2. **Subnets**: Ensure at least two default public subnets exist and **Auto-assign public IPv4** is `Yes`. If not, edit each subnet to enable it.
3. **Route table**: For the default public route table, confirm `0.0.0.0/0 -> igw-*` exists.
4. **Security group for demos**: Create/reuse (in VPC menu) `demo-employeeapi-sg` in the default VPC with inbound HTTP 80/HTTPS 443 from 0.0.0.0/0 (demo only; tighten later). Outbound allow all.
   - Default SG Existing: will exist after creating a default vpc, leave it, its inbound is self referencing block.
   - New SG Inbound:
     - HTTP (80) from 0.0.0.0/0,
     - HTTPS (443) from 0.0.0.0/0 "Anywhere", and
     - RDP/SSH locked to "My IP" if I want direct access. For remote shell without open ports
   - New SG Outbound: allow all traffic so the instance can pull updates.
5. **Quick CLI check (optional)**:
   - `aws ec2 describe-vpcs --filters Name=isDefault,Values=true --query 'Vpcs[0].VpcId' --output text`
   - `aws ec2 describe-subnets --filters Name=default-for-az,Values=true --query 'Subnets[].SubnetId' --output text`
   - `aws ec2 describe-route-tables --filters Name=association.main,Values=true --query 'RouteTables[0].Routes' --output table`
   - `aws ec2 describe-security-groups --filters Name=group-name,Values=employeeapi-quickstart-sg --query 'SecurityGroups[0].GroupId' --output text`

## Phase 1 - ECR + ECS Console Setup (from scratch, no Harness yet)

Goal: create minimal ECR/ECS assets manually so Harness can reference them later. These will be used by Harness to create images and deploy API and Lambda's.

Note: using 1 repo with tags, again for a quick setup, if i have to tear down AWS for cost savings.

1. **ECR repository (single-repo strategy)**
   - Console > ECR > Repositories > Create repository.
   - Name: `demoskills` (one repo for all services). Private, scan-on-push enabled. Tag immutability optional (on is safer).
   - Tag format example: `demoskills:employeeapi-<git-sha>`, `demoskills:taxapi-<git-sha>`.
   - Optional lifecycle policy: expire untagged images; keep last N tags per prefix to save pennies.
2. **IAM roles**
   - Task execution role: IAM > Roles > Create role > AWS service > Elastic Container Service > Elastic Container Service Task.
     - Attach `AmazonECSTaskExecutionRolePolicy`.
     - Name: `employeeapi-ecs-execution`.
   - Task role (app permissions): create `employeeapi-task-role` now even if empty; add least-privilege access later (e.g., S3/DB).
3. **CloudWatch Logs**
   - Create log group `/ecs/employeeapi` (Console > CloudWatch > Logs > Log groups > Create).
4. **ECS cluster**
   - Console > ECS > Clusters > Create cluster.
   - Name: `demo-ecs`. Networking only (no EC2 capacity providers), default VPC, pick two public subnets.
5. **Task definition**
   - Console > ECS > Task definitions > Create (FARGATE).
   - Task family: `employeeapi-task`; Runtime: Fargate; CPU/memory: 0.5 vCPU / 1 GB (adjust later).
   - Task role: `employeeapi-task-role`; Execution role: `employeeapi-ecs-execution`.
   - Container:
     - Name: `employeeapi`.
     - Image: placeholder `public.ecr.aws/amazonlinux/amazonlinux:latest` until Harness pushes real image. Ports: 8080 TCP.
     - Logs: awslogs, region = target region, group = `/ecs/employeeapi`, stream prefix = `employeeapi`.
6. **Service (optional placeholder)**
   - Console > ECS > Clusters > `demo-ecs` > Create service.
   - Launch type: Fargate; Task definition: `employeeapi-task`; Service name: `employeeapi-svc`.
   - Desired count: 0 or 1 (set 0 if no real image yet).
   - Networking: default VPC, two public subnets, assign public IP = ENABLED, security group = `demo-employeeapi-sg`.
   - No load balancer for now; add ALB later when image is ready.
7. **Ready for Harness**
   - Capture: region, cluster `demo-ecs`, task family `employeeapi-task`, service `employeeapi-svc`, roles, log group, and ECR repo URL.
   - Decide registry (ECR vs external). Harness will update the task definition image, set desired count, and add ALB later if needed.

## old phases - keep for now as notes will delete later.

## Phase 0 - Prerequisites & Decisions

1. **AWS accounts/roles**: confirm an AWS account with permissions to manage ECR, ECS, IAM, VPC, and Route 53. Decide on a primary region (us-east-1 unless a different one is already standard).
2. **Networking**: reuse the default VPC + public subnets initially. Long term we can create dedicated subnets and security groups via IaC.
3. **Container registry**: choose ECR for production and optionally Docker Hub/GitHub Container Registry for local experiments.
4. **Harness access**: ensure Harness has connections set up for GitHub, Docker/ECR, and AWS (either an IAM user with access keys or a cross-account role).

## Phase 1 - Harness Scaffolding (incremental)

Goal: create minimal Harness plumbing, while keeping VPC setup manual (cheaper/faster than automating it). The API will be containerized and deployed in later phases.

1. VPC handling (manual, console):
   - Confirm a default VPC exists in the target region; if missing, click “Actions -> Create default VPC”.
   - Verify at least two public subnets have auto-assign public IPv4 enabled.
   - Create or reuse a simple SG (e.g., `employeeapi-quickstart-sg`) with HTTP/HTTPS inbound for demo use; tighten later.
2. Registry step:
   - Preferred: ECR repo (`employeeapi`) with lifecycle policy; create via Harness step (CLI/IaC).
   - Alternative: external registry connector (GHCR/Docker Hub) if we want to avoid ECR storage.
3. Build stage (Harness):
   - Checkout -> login to registry -> `docker build` -> `docker push`; emit image tag/digest as output.
4. Deploy stage (Harness):
   - Create/update ECS cluster, task definition, and service (optionally ALB) referencing the pushed image.
5. Teardown stage (Harness):
   - Stop/delete ECS service (and ALB/target group if created).
   - Delete ECR repo (if using ECR) or leave external registry untouched.
   - Keep inexpensive/shared bits (VPC) unless we intentionally nuke everything.

Immediate Harness actions:

- Create/verify Harness project and service for EmployeeAPI.
- Add connectors: GitHub (this repo), AWS (keys or role), and chosen registry (ECR or external).
- Create a minimal pipeline skeleton with build + deploy + teardown stages (even if steps are placeholders).

## Phase 2 - Dockerize the APIs

1. For each API (start with `EmployeeAPI`):
   - Add a `Dockerfile` targeting `linux-x64`, multi-stage (restore/build/publish + runtime stage).
   - Include environment-variable driven config and expose port `8080` (or similar) consistently.
   - Since Docker is not installed locally, rely on Harness (or another remote runner/Linux box) for `docker build`/`docker run` throughout CI, and keep helper scripts focused on publishing artifacts only.
2. Create a root-level `docker/` folder with shared compose files and `.dockerignore` for future local Linux usage.
3. Validation:
   - **Remote path (default)**: Let Harness build/test the container in its build stage and surface logs in the pipeline.
   - **Optional local Linux box**: When available, run `docker build`/`docker run -p 8080:8080 employeeapi:local` to test manually.
4. Once stable, standardize tags: `demo/<service>:<git-sha>` for dev, `release-<version>` for prod.

## Phase 3 - AWS Building Blocks

1. **ECR repositories**: create `employeeapi` (and placeholders for future APIs). Enable scan-on-push.
2. **IAM roles/policies**:
   - Task execution role allowing `ecr:GetAuthorizationToken`, `logs:CreateLogStream`, etc.
   - Task role for the API (least privilege, e.g., S3 or DB access later).
3. **CloudWatch logs**: define log groups `/ecs/employeeapi`.
4. **Security groups**:
   - `employeeapi-alb-sg`: inbound 80/443 from `0.0.0.0/0`.
   - `employeeapi-service-sg`: inbound from the ALB SG only.
5. **Load balancer (optional initial)**: create an Application Load Balancer with HTTP listener -> target group (IP type) pointing at ECS tasks. Keep idle timeout low until HTTPS is added.

## Phase 4 - ECS Fargate Configuration

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

## Phase 5 - Harness Pipeline

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

## Phase 6 - Automation & Future Enhancements

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
