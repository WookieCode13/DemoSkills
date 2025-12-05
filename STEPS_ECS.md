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
   - Tag format example (valid chars: letters/numbers/.\_- only): `employeeapi-latest` plus a stable tag like `employeeapi-abc123` (short git SHA) or `employeeapi-42` (build number). I used `-latest`. for all 4 apis i plan to deploy.
   - Optional lifecycle policy: expire untagged images; keep last N tags per prefix to save pennies.
2. **IAM roles**
   - Task execution role (shared): IAM > Roles > Create role > AWS service > Elastic Container Service > (use case) Elastic Container Service Task.
     - Attach `AmazonECSTaskExecutionRolePolicy`.
     - Name: `demoskillsapi-ecs-execution` (reuse for all services for simplicity).
   - Task role (app permissions): create `demoskillsapi-task-role` now even if empty; add least-privilege access later (e.g., S3/DB). For simplicity you can reuse one later, but per-service roles are safer when you add permissions.
     - IAM > Roles > Create role > AWS service > Elastic Container Service > (use case) Elastic Container Service Task.
     - leave perms empty for now
     - will reuse this role for now, simpler for the demo app than creating roles for every api and usage.
3. **CloudWatch Logs**
   - Create log group `/ecs/employeeapi` (Console > CloudWatch > Logs > Log groups (at top column) > Create).
   - taxcalculatorapi, payapi, companyapi
4. **ECS cluster**
   - Console > ECS > Clusters > Create cluster.
   - Name: `demoskills-001-ecs`. (Fargate only). Networking only (no EC2 capacity providers), default VPC, pick two public subnets.
   - note: i tried `demoskills-ecs`, failed first try. Not in list, but would not let me create, says existed. (still not in list)
5. **Task definition**
   - Console > ECS > Task definitions > Create (FARGATE).
   - Task family: `employeeapi-task`; Runtime: Fargate; CPU/memory: 0.5 vCPU / 1 GB (adjust later).
   - Task role: `employeeapi-task-role`; Execution role: `employeeapi-ecs-execution`.
   - Container:
     - Name: `employeeapi`.
     - Image: placeholder `public.ecr.aws/amazonlinux/amazonlinux:latest` until Harness pushes real image. Ports: 8080 TCP.
     - Logs: awslogs, region = target region, group = `/ecs/employeeapi`, stream prefix = `employeeapi`.
6. **Service (optional placeholder)**
   - Console > ECS > Clusters > `demoskills-001-ecs` > Create service.
   - Launch type: Fargate; Task definition: `employeeapi-task`; Service name: `employeeapi-svc`.
   - Desired count: 0 or 1 (set 0 if no real image yet).
   - Networking: default VPC, pick two of the 6 default public subnets, assign public IP = ENABLED, security group = just `demo-employeeapi-sg`.
   - No load balancer for now; add ALB later when image is ready.
7. **Ready for Harness**
   - Capture: region, cluster `demoskills-001-ecs`, task family `employeeapi-task`, service `employeeapi-svc`, roles, log group, and ECR repo URL.
   - Decide registry (ECR vs external). Harness will update the task definition image, set desired count, and add ALB later if needed.

## Phase 2 - Harness Pipelines (build/deploy/pause)

Goal: automate image build/push and ECS updates; keep costs down by pausing (desired count 0) when idle.

1. **Connectors/secrets**
   - Git connector for this repo.
   - AWS connector for ECR/ECS (keys or role).
   - Registry login (ECR) using the same AWS connector.
2. **Build stage**
   - Steps: checkout -> `aws ecr get-login-password | docker login` -> `docker build -f docker/services/EmployeeAPI.Dockerfile -t <repo>/demoskills:employeeapi-latest .` -> push `employeeapi-latest` and a stable tag (e.g., `employeeapi-<build>`).
   - Output the chosen image tag for deploy (e.g., `IMAGE_TAG` variable).
3. **Deploy stage**
   - Update task definition image to `<repo>/demoskills:<IMAGE_TAG>`; use execution role `demoskillsapi-ecs-execution` and task role `demoskillsapi-task-role`.
   - Update service `employeeapi-svc` on cluster `demoskills-001-ecs` and scale desired count to 1.
   - Networking: default VPC, two public subnets, SG `demo-employeeapi-sg`. No ALB initially.
4. **Pause/teardown stage**
   - Scale service desired count to 0.
   - Optional: clean old images per lifecycle policy; do not delete repo/cluster.
5. **Notes**
   - If you later add an ALB, include health check grace period and SG rules (ALB SG 80/443; service SG allows from ALB SG).
   - Even if you default to `employeeapi-latest`, keep a stable tag for rollback (`employeeapi-<seq>`).
