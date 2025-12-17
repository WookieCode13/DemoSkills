# Project Plan — DemoSkills

Main goal is to build a demo portfolio project that showcases my resume skills. The plan is to deploy several simple APIs, provide Swagger documentation, and eventually add a frontend. I will incorporate AWS services such as Lambda, SNS, SQS, and S3. The tech stack will include VS Code (C# and Python), GitHub, Harness, and Docker. For container orchestration, I will use ECS with Fargate instead of EKS to reduce costs. The project will be designed for easy deployment and teardown, making it ideal for interviews and demonstrations.

## 🧩 Phase 0 — Setup

- [x] Create project folder structure
- [x] Add Markdown files (README, PLAN, AI_NOTES)
- [x] Enable GitHub Copilot Chat in VS Code
- [x] Initialize Git repository

## 🧩 Phase 1 — AWS Setup

- [x] Confirm default VPC/subnets/route table and demo security group (`demo-employeeapi-sg`)
- [x] Create shared ECR repo (`demoskills`) with simple tag scheme and lifecycle policy
- [x] Create shared execution role (`demoskillsapi-ecs-execution`) and placeholder task role
- [x] Create ECS cluster (`demoskills-001-ecs`), log group (`/ecs/employeeapi`), and placeholder task/service
- [ ] Create Tasks for pay, company and tax.
- [ ] Create ALB (load balancer) in EC2 80/443. will attach to ECS. Containers will 808x...

## ⚙️ Phase 2 — API Foundations

- [x] Create C# Employee API project
- [ ] Create Python Company API project
- [ ] Create C# Tax Calculator API project
- [ ] Add Swagger / OpenAPI documentation
- [ ] Add sample endpoints and tests
- [ ] Add workspaces for each testable sub project.
- [ ] Add a swagger config SwaggerUIBundle for one swagger page with tabs.

## 🐳 Phase 3 — Dockerization

- [ ] Create Dockerfiles for each API
- [ ] Test local Docker builds
- [ ] Add docker-compose file

## 🚀 Phase 4 — Harness Deployment

- [x] Create Harness pipeline YAML
- [x] Deploy API to AWS
- [ ] Create teardown pipeline for resource cleanup

## 🧪 Phase 5 — Testing & Security

- [ ] Add unit and BDD tests
- [ ] Implement token-based authentication
- [ ] Add logging and monitoring

## 🗄️ Phase 6 — Database Integration

- [ ] Connect APIs to local Linux PostgreSQL DB
- [ ] Add migration script to create first tables
- [ ] Add PostgreSQL DB in AWS
- [ ] Connect APIs to DB (POST and GET)

## 🪁 Phase 7 — Serverless & Messaging

- [ ] Add Lambda functions (Python and C#)
- [ ] Add SNS and SQS handling
- [ ] Create/update pipelines to deploy, build, and tear down all AWS (cost saving)

## 🖥️ Phase 8 — Frontend (Future)

- [ ] Choose frontend framework
- [ ] Build simple UI to consume APIs
- [ ] Integrate authentication

---

## 🧭 Notes

Keep AWS usage under $20/month.
Focus on reusable patterns and automation.
