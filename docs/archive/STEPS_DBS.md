NOTE: Keep the DB simple, manageable, and cost effective. Use one DB that all services share (demo-only; normally each service would have its own). For cost control, you may clone/tear down the DB when idle and back it up to S3 if needed.

## RDS PostgreSQL via Full configuration (recommended selections)

### Phase 1 - Create the Postgres database

#### Step 1) Engine options
* Engine type: PostgreSQL (not Aurora)
* Version: Latest minor in current LTS major (e.g., 16.x)

#### Step 2) Templates
* Pick Dev/Test (or Free tier if shown and you qualify)
  * Dev/Test keeps defaults simple and low-cost.

#### Step 3) Availability and durability
* Deployment option: Single DB instance (Single-AZ)
  * Do not choose Multi-AZ at this stage.

#### Step 4) Settings
* DB instance identifier: `lr-dev-demoskills-pg` (or similar)
* Master username: `postgresadmin` (anything but plain `postgres` is fine)
* Credentials management: set a password now; later move to Secrets Manager / Harness secrets

#### Step 5) Instance configuration
* DB instance class: Burstable
  * Prefer t4g.micro (Graviton) if available, otherwise t3.micro
  * Small is fine; you can scale later.

#### Step 6) Storage
* Storage type: General Purpose SSD (gp3 if selectable)
* Allocated storage: 20 GB
* Storage autoscaling: enable and cap around 100 GB (optional but safe)

#### Step 7) Connectivity (important)
* Compute resource: select “Don’t connect to an EC2 compute resource” (no EC2 attachment)
* VPC: same VPC as your ECS services (likely default VPC)
* DB subnet group: default DB subnet group is fine for now
* Public access: No
* VPC security group: choose an existing SG or create new; edit inbound after creation
    * create one here (i used default and had to come back an create one)
    * create 1 SG, add a rule per servcie SVC sg in ECS service.
        * type: postgres
    * leave the defautl outbound rule.
    * Also added 'launch-wizard-1' SG from teh ECS SG. 
         type: postgres
* Availability Zone: No preference (auto)
* RDS proxy: Off (not needed now)

#### Step 8) Database authentication
* Password auth: Enabled
* IAM auth: Off (can add later)

#### Step 9) Additional configuration
* Initial database name: `demoskills` (optional but convenient)
* DB parameter / option group: defaults are fine
* Backup retention: 1 day (keeps cost down but gives a rollback point)
* Deletion protection: Off (demo environment—be careful)
* Performance Insights: Off (saves cost)
* Monitoring / CloudWatch logs: Basic/disabled to save cost (enable later if needed)

Create the database.

---

## Immediately after creation: do these 3 fixes

### 1) Security Group rule (only allow your ECS services)
Skip if: created a SG in the DB setup and its inbound rules are set to each ECS servcie.
Go to: RDS instance -> Connectivity & security -> VPC security groups -> click the SG

Edit inbound rules:
* Add inbound rule: Type: PostgreSQL | Port: 5432 | Source: Security Group of your ECS tasks/services (NOT 0.0.0.0/0)

This makes RDS reachable only from your services.

### 2) Verify you can connect from ECS (not from your laptop)
* Your apps should connect via the RDS endpoint (hostname), not an IP
* Use your DB credentials from secrets

### 3) Turn on a cost-control habit
* Keep backups at 1 day initially
* Take a manual snapshot before major migrations; delete old snapshots once stable

---

## One decision you should make now (but you can keep it simple)

Where will you run DB migrations from?
* Option A: run migrations as part of each service startup (fastest for early demo)
* Option B: run migrations in CI/CD (Harness) as a dedicated step (more “real world”)

For DemoSkills, Option B is a strong portfolio signal, but Option A is fastest to get going.

---

If you tell me what you used for ECS networking:
* VPC name (or "default VPC")
* Your ECS service SG name (or the SG id)

...then I can give you the exact inbound rule configuration and a clean connection string pattern for .NET (Npgsql) and Python (SQLAlchemy/asyncpg) that matches your setup.

---

### Phase 2 - TODO (cloning and tearing down)
Temporary notes / thoughts:
* Maybe a YAML + Harness flow to clone and pause.
* Be mindful of migration scripts.
* Might recreate from scratch using JSON and seed data.
