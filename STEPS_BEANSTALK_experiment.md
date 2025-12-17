# Deployment Steps (Learning Log)

Goal: deploy EmployeeAPI quickly using Elastic Beanstalk, verify Swagger, and keep costs low. We’ll add domains and path routing later. Keep this simple and manual for learning.

# UPDATE: 
this worked, my site ran. but the no good way to tear down and redploy each day. Moving back to EC2 and ECS.

## Prerequisites

- AWS account with console access
- .NET 8 SDK installed locally
- This repo checked out; publishing script: `commands/publish-employeeapi.ps1`

## Step 1 — Publish artifact (self‑contained)

- Run: `commands/publish-employeeapi.ps1`
- Outputs: `publish/employeeapi` folder and `publish/employeeapi.zip`
- The script writes a `Procfile` so Beanstalk can start the app: `web: ./EmployeeAPI`

## Step 2 — Create Elastic Beanstalk app (UI only)

- Console → Elastic Beanstalk → Create application
  - Application name: ` `
  - Environment: `Web server`
  - Platform: `.NET on Linux` (latest, AL2023/.NET 8)
  - Environment type: `Single instance` (cheapest)
  - Application code: Upload `publish/employeeapi.zip`
  - Create environment and wait for Health: `Ok`
- Verify: open the provided URL like `http://<env>.elasticbeanstalk.com/swagger`

Notes
- If your publish produced `EmployeeAPI.dll` instead of a single binary, update Procfile to `web: dotnet EmployeeAPI.dll` and re‑zip.
- If Swagger redirects to HTTPS and fails, temporarily remove `app.UseHttpsRedirection()` in `apis/EmployeeAPI/Program.cs`, republish, re‑upload.

## Step 3 — Optional domain (later)

- Quick win: use the Beanstalk URL for learning and move on.
- When ready for a custom domain:
  - Easiest: use a subdomain CNAME (e.g., `api.longranch.com` → EB CNAME).
  - Path `/employee` requires a proxy or router (e.g., CloudFront or ALB path rules) and is best done later.

## Cleanup / Cost control

- Elastic Beanstalk: Actions → Terminate environment; then delete the application.
- Remove any Route 53 records you added.

## Next (when this is green)

- Add HTTPS on Beanstalk.
- Map `api.longranch.com` via CNAME.
- If you specifically need `/employee`: add CloudFront or ALB path‑based routing.
- Later switch to containers and Harness (IaC optional) for repeatable infra.
