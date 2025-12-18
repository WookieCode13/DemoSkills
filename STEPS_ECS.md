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
8. **ALB - Load Balancer (per-port targets, shared ALB)**
   - new security group `demoskills-alb-sg`.
      - EC2 > Security group > name and use same vpc as ECS.
   - Target groups (one per API/port):
     - EC > Load Balancing > Target Groups.
     - `demoskills-tg-8080`  (should have named it demoskills-emp-tg-8080)
     - Type: IP; Protocol: HTTP; Port: match container port (EmployeeAPI 8080; future 8081/8082...).
     - same VPC
     - Health check: path `/swagger` or `/health`; port: traffic port.
   - Create ALB (EC2 console > Load Balancers):
     - `demoskills-alb`
     - same VPC, add alb-sg, add target group.
     - Scheme: internet-facing; Type: Application.
     - Listeners: HTTP 80 (add HTTPS 443 later with cert).
     - Subnets: pick two public subnets; SG: new SG allowing 80/443 from 0.0.0.0/0.
   - Wire listeners to target groups:
     - Default action: forward to EmployeeAPI TG (8080) or add path rules like `/employeeapi` → TG8080, `/companyapi` → TG8081.
   - ECS service updates:
     - ECS > my ecs > my servcie > update > down to load balancing
       - type: Application Load Balancer > add my ALB, http 80, and my Target grp.
     - For each service, set network mode awsvpc, assign public IP, and attach the matching target group with container port set to that API’s port.
     - Service/task SG: allow inbound from the ALB SG on the app port (8080/8081/8082), not from the internet directly.
   - NOTE: 
      - SG ecs task
         - Custom TCP, 8080, select the ALB (Source: your ALB security group (e.g., demoskills-alb-sg), not an IP range, may have to create new if error cidr.)
      - SG ALB
         - 80/443
   - Test:
     - Hit `http://<ALB-DNS>/swagger` (or path-based rule), or `curl -v http://<ALB-DNS>:80/`.
     - Verify target health in the TG; if unhealthy, check port mapping, SG rules, and health-check path.
9. 8. **Route 53 - <use my domain>**
   - Route 53 > Hosted Zones > create zone > 2 records NS SOA > create new record > Alias is there. alias to ALB.
      - TYPE:a, BLANK (sub here future), alias: yes, Route traffic to: target my Alb (app load bal, us-east, my alb) , policy simple

## Phase 2 - Harness Connector Prep (before pipelines)

Goal: set up the Harness project, delegate, connectors, and validate ECR/ECS access before building real pipelines. (I created connectors at the account scope; could also scope to project.)

1. Create Project + Docker delegate
   - Project (not guided): e.g., `demo skills project`.
   - On the Linux box: ensure Docker is running (`docker ps`).
   - In Harness: Project Settings > Delegates > Install > Docker; name e.g., `harness-docker-delegate-demoskills`; copy/run the install command on Linux.
   - If issues: `docker logs <container> --tail 50`, `curl -v https://app.harness.io/ -o /dev/null`, or reinstall the delegate and retest connectivity.
2. Git connector (repo source)
   - Project Settings > Connectors; type Git (HTTP).
   - Use PAT with `repo` scope stored as a Harness secret (e.g., `github_pat_demoskills`).
   - ~~Assign the docker delegate and run the test after the delegate is healthy.~~
   - use platform not delgate, paid for free, 2000 free credits for cloud usage
3. AWS connector (ECR + ECS access)
   - Scope: project (or account if reused). Type: AWS with Access Key/Secret.
   - IAM user (example `harness-demoskills`): programmatic access only. For now attach `AmazonECS_FullAccess` and `AmazonEC2ContainerRegistryFullAccess` (tighten later).
   - Store key/secret as Harness secrets (e.g., `aws_demoskills_creds`), set backoff (fixed 5s, 3 retries), and bind to the docker delegate.
   - use platform not delgate, paid for free, 2000 free credits for cloud usage
   - Minimum perms for production hardening:
     - ECR push: `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`, `ecr:PutImage`, `ecr:DescribeRepositories` (+ `ecr:CreateRepository` if pipelines create repos).
     - ECS deploy: `ecs:Describe*`, `ecs:RegisterTaskDefinition`, `ecs:UpdateService`, `ecs:ListClusters`, `ecs:ListServices`.
     - iam:PassRole for `demoskillsapi-ecs-execution` and `demoskillsapi-task-role`.
     - CloudWatch Logs (if Harness may create streams): `logs:CreateLogGroup`, `logs:CreateLogStream`, `logs:PutLogEvents`.
4. ECR registry connector (prep for builds)
   - Type: AWS ECR (Docker Registry). If you do not see the AWS-connector option (only “Other” shows), it’s usually a scope mismatch; try recreating the AWS connector at the same scope (project) and retry. If still unavailable, skip the registry connector and rely on the CLI login in the build stage.
   - If you must use “Other,” set Registry URL: `https://<account>.dkr.ecr.<region>.amazonaws.com/demoskills`, delegate = docker delegate, and leave auth anonymous (the build stage docker login supplies auth).
   - Run the connectivity test on the docker delegate if the UI allows; if private networking, ensure NAT/VPC endpoints reach ECR.
5. Secrets and naming
   - Keep predictable secret names (`github_pat_demoskills`, `aws_demoskills_creds`) and reference them in connectors.
6. Quick validation (delegate shell or Harness tests)
   - `aws sts get-caller-identity`
   - `aws ecr describe-repositories --repository-names demoskills`
   - `aws ecs list-clusters`
   - In Harness UI, run tests for Git, AWS, and ECR connectors.
7. Sanity pipeline (hello world)
   - Create a tiny pipeline (one step: `echo hello from delegate`) using the docker delegate to confirm execution works before adding build/push/deploy stages.

## Phase 3 - Harness Pipelines (build/deploy/pause)

Goal: automate image build/push and ECS updates; keep costs down by pausing (desired count 0) when idle.
Note: successful ECR image thru single stage cloud ci.
Note: successful ECR image thru multi stage cloud ci. (much slower and higher creit cost)


1. **Connectors/secrets**
   - Ensure Phase 2 connectors exist: Git first (repo PAT), then AWS (ECR/ECS + iam:PassRole), and ECR registry using the AWS connector. Secrets stored in Harness.
2. **Build stage**
   - Option 1: Stage type: CI (Build); infrastructure: docker delegate on the Linux box.
   - (Working local setup) Run runner + delegate on host network so localhost:3000 is reachable:
     - `docker rm -f harness-ci-runner harness-docker-delegate-demoskills`
     - Runner: `docker run -d --name harness-ci-runner --restart unless-stopped --network host -v /var/run/docker.sock:/var/run/docker.sock harness/ci-lite-engine:latest`
     - Delegate: `docker run -d --name harness-docker-delegate-demoskills --restart unless-stopped --privileged --network host -v /var/run/docker.sock:/var/run/docker.sock -e ACCOUNT_ID=YOUR_ACCOUNT_ID -e DELEGATE_TOKEN=YOUR_DELEGATE_TOKEN -e MANAGER_HOST_AND_PORT=https://app.harness.io -e DELEGATE_NAME=harness-docker-delegate-demoskills -e DELEGATE_TAGS=harness-docker-delegate-demoskills -e NEXT_GEN=true -e DELEGATE_TYPE=DOCKER us-docker.pkg.dev/gar-prod-setup/harness-public/harness/delegate:25.11.87301`
     - Sanity: `docker ps | grep ci-lite` and `sudo ss -lntp | grep :3000`
   - Option 2: Stage type: CI (Build); infrastructure: cloud (have script for single and multistage).
   - TBD 
   - 2.1 Checkout: use the Git connector; shallow clone ok.
   - 2.2 ECR login: `aws ecr get-login-password --region <region> | docker login --username AWS --password-stdin <account>.dkr.ecr.<region>.amazonaws.com`.
   - 2.3 Build: `docker build -f docker/services/EmployeeAPI.Dockerfile -t <account>.dkr.ecr.<region>.amazonaws.com/demoskills:employeeapi-latest .`
   - 2.4 Tag rollout: `TAG=employeeapi-${HARNESS_BUILD_ID:-$HARNESS_BUILD_NUMBER}` then `docker tag <account>.dkr.ecr.<region>.amazonaws.com/demoskills:employeeapi-latest <account>.dkr.ecr.<region>.amazonaws.com/demoskills:$TAG` (or use short git SHA if preferred).
   - 2.5 Push: `docker push <account>.dkr.ecr.<region>.amazonaws.com/demoskills:employeeapi-latest` and `docker push <account>.dkr.ecr.<region>.amazonaws.com/demoskills:$TAG`.
   - 2.6 Export tag: `echo \"IMAGE_TAG=$TAG\" >> $HARNESS_ENV_EXPORTS` (or set an output variable) for the deploy stage to consume.

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
