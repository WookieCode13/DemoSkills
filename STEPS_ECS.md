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

## Phase 2 - Harness Connector Prep (before pipelines)

1. Git connector (repo source)
   - Harness: Project > Connectors > New > GitHub (or Git). HTTP/HTTPS using PAT secret with `repo` scope only. Repo-only scope; shallow clone ok.
   - Disable LFS unless needed; name e.g., `github_demoskills`.
2. AWS connector (ECR + ECS)
   - Type: AWS. Auth: Access Key/Secret (least-privilege IAM user) or Assume Role (preferred if delegate in AWS). Set default region (e.g., us-east-1).
   - IAM permissions (minimum):
     - ECR push: `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`, `ecr:PutImage`, `ecr:DescribeRepositories` (and `ecr:CreateRepository` if repo might be created by pipeline).
     - ECS deploy: `ecs:Describe*`, `ecs:RegisterTaskDefinition`, `ecs:UpdateService`, `ecs:ListClusters`, `ecs:ListServices`.
     - IAM pass role: `iam:PassRole` allowed for `demoskillsapi-ecs-execution` and `demoskillsapi-task-role` ARNs.
     - CloudWatch Logs (if Harness may create streams): `logs:CreateLogGroup`, `logs:CreateLogStream`, `logs:PutLogEvents`.
   - Name e.g., `aws_demoskills`.
3. ECR registry connector
   - Type: AWS ECR (Docker Registry). Use the AWS connector above. Registry URL: `<account>.dkr.ecr.<region>.amazonaws.com/demoskills`.
   - Enable connectivity test; ensure runner can reach ECR (NAT/VPC endpoints if private).
4. Delegate/runner placement
   - Ideally same region/VPC as ECR/ECS. Outbound 443 required; if private subnets, ensure NAT or VPC endpoints for ECR/ECS/STS. Delegate role/keys must carry the permissions above.
5. Secrets and naming
   - Store PAT and AWS creds as Harness secrets; predictable names (`github_pat_demoskills`, `aws_demoskills_creds`). Reference them in connectors.
6. Quick validation
   - From delegate shell with those creds: `aws sts get-caller-identity`; `aws ecr describe-repositories --repository-names demoskills`; `aws ecs list-clusters`.
   - In Harness UI, run connector tests for Git and ECR.

## Phase 3 - Harness Pipelines (build/deploy/pause)

Goal: automate image build/push and ECS updates; keep costs down by pausing (desired count 0) when idle.

1. **Connectors/secrets**
   - Ensure Phase 2 connectors exist: Git first (repo PAT), then AWS (ECR/ECS + iam:PassRole), and ECR registry using the AWS connector. Secrets stored in Harness.
2. **Build stage**

   - (my notes) setting up harness, may need a new step or phase, maybe moves to the end of phase 2? not sure.
   - (my notes) start project (currently free account) > name project > select VM > AWS > SSH

   - 2.1 Checkout: use the Git connector to pull the repo; shallow clone ok.
   - 2.2 Login to ECR: `aws ecr get-login-password --region <region> | docker login --username AWS --password-stdin <account>.dkr.ecr.<region>.amazonaws.com`.
   - 2.3 Build image: `docker build -f docker/services/EmployeeAPI.Dockerfile -t <account>.dkr.ecr.<region>.amazonaws.com/demoskills:employeeapi-latest .`
   - 2.4 Tag stable: add a rollout tag `employeeapi-<build>` (build number or short SHA) for rollback.
   - 2.5 Push images: push `employeeapi-latest` and the stable tag.
   - 2.6 Export tag: set output variable `IMAGE_TAG` to the chosen deploy tag (e.g., `employeeapi-<build>`). Use this in deploy stage.

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

---
