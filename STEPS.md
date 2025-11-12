# Deployment Steps (Learning Log)

Goal: expose EmployeeAPI publicly at https://longranch.com/employee and see Swagger load. Start manual and cheap to learn; no Harness yet. Keep costs near zero by using a short‑lived EC2 you terminate after testing. Later we’ll automate and harden.

## Step 1 — Cheap AWS smoke test (manual, UI‑first)

- Plan: use a temporary EC2 with Nginx reverse proxy to serve `/employee` and load Swagger. Keep it simple and terminate when done.

- Create Security Group (EC2 > Security Groups)
  - Inbound rules: `HTTP (80/tcp)` from your IP, `SSH (22/tcp)` from your IP.

- Create VPC (if needed)
  - If you already have a default VPC with at least one public subnet, skip this section. If your VPC shows 0 subnets, create one of the following:
  - Create default VPC: VPC console → Actions → Create default VPC (fastest).
  - Or minimal public VPC: VPC → Create VPC → “VPC and more” → IPv4 CIDR `10.0.0.0/16`, AZs `1`, Public subnets `1`, Private subnets `0`, NAT gateways `0`. This creates an Internet Gateway and public route table for you.
  - Using an existing VPC? Quick checks: has an Internet Gateway attached; public subnet’s route table has `0.0.0.0/0 → igw-…`; the subnet is associated to that route table; subnet setting “Auto-assign public IPv4” is Enabled (or enable public IP at launch).

- Launch EC2 (EC2 > Launch Instance)
  - AMI: Amazon Linux 2023 (x86_64)
  - Instance type: `t3.micro`
  - Network: default VPC and a public subnet; Auto‑assign Public IP: Enabled
  - Security group: the one above
  - Key pair: create/download
  - Launch instance

- (Optional) Allocate Elastic IP (EC2 > Elastic IPs)
  - Allocate, then Associate to your instance (keeps DNS stable until teardown).

- Publish artifact locally
  - Use your script: `commands/publish-employeeapi.ps1` (or run):
    - `dotnet publish apis/EmployeeAPI/EmployeeAPI.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/employeeapi`
  - Zip the folder `publish/employeeapi`.

- Upload ZIP to S3 (Console only)
  - Create a private bucket; upload the ZIP.
  - Select the object > Actions > Create presigned URL, copy the URL.

- Connect to instance (no local SSH required)
  - EC2 > Instances > Select instance > Connect > EC2 Instance Connect > Connect.

- On the instance (paste commands)
  - `sudo dnf -y install nginx && sudo systemctl enable --now nginx`
  - `cd ~ && curl -L "<YOUR_PRESIGNED_URL>" -o app.zip && unzip -o app.zip -d app`
  - `cd ~/app && ASPNETCORE_URLS=http://0.0.0.0:8080 nohup ./EmployeeAPI > app.log 2>&1 &`
  - `sudo tee /etc/nginx/conf.d/employee.conf > /dev/null <<'NGINX'
server {
    listen 80;
    server_name longranch.com;
    location /employee/ {
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        rewrite ^/employee/?(.*)$ /$1 break;
        proxy_pass http://127.0.0.1:8080/;
    }
}
NGINX
sudo nginx -t && sudo systemctl reload nginx`

- DNS
  - Point `longranch.com` A record to the instance’s Elastic IP (or its current public IP).

- Verify
  - Open `http://longranch.com/employee/swagger` (or the EC2 public DNS + `/employee/swagger`).
  - If Swagger redirects to HTTPS and fails, temporarily remove `app.UseHttpsRedirection()` in `apis/EmployeeAPI/Program.cs`, republish, and re‑upload.

- Teardown (avoid costs)
  - Delete A record, disassociate/release Elastic IP, terminate the instance, delete the S3 object/bucket.

Later: add HTTPS (Let’s Encrypt), then a managed service (App Runner/ECS), and finally Harness/Terraform for repeatability.

My notes:
EC2 (Elastic Compute Cloud)	Raw virtual machines in the cloud — you manage the OS, patching, scaling, and deployments yourself. Think “AWS’s version of a server.”
ECS (Elastic Container Service)	AWS’s container orchestration service. It runs Docker containers for you on a cluster of EC2 instances (or serverless using Fargate). You focus on containers, not OS.
NGINX: is lightweight software that can: Serve static websites.
VPC:	Virtual network	Use default VPC
SUBNET:	Network segment	Use public subnet
IP:	Public access	Enable auto-assign public IP
Internet Gateway:	Gives web access	Already attached to default VPC
