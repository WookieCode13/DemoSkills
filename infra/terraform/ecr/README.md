# ECR Terraform Module (Minimal)

Creates a single ECR repository to host `EmployeeAPI` images.

Prereqs:
- AWS credentials exported locally (or SSO): `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`.
- Terraform >= 1.5 installed.

Usage:

- Apply:
  - `./apply.ps1` (uses env vars or defaults: region `us-east-1`, repo `employee-api`)
- Destroy:
  - `./destroy.ps1`

Note: ECR repositories are low-cost; destroy when not needed to keep a clean account.

