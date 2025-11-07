# Deployment Steps (Learning Log)

Goal: expose EmployeeAPI publicly at https://longranch.com/employee and see Swagger load. Start manual and cheap to learn; no Harness yet. Keep costs near zero by using a short‑lived EC2 you terminate after testing. Later we’ll automate and harden.

## Step 1 — Cheap AWS smoke test (manual)

- Launch EC2 (temporary):
  - AMI: Amazon Linux 2023, Instance: `t3.micro` (x86_64), Security Group: allow `80/tcp` from your IP (or 0.0.0.0/0 for a quick test).
  - Assign a public IP. Create/download a key pair.
- Publish the API locally (self‑contained Linux x64):
  - `dotnet publish apis/EmployeeAPI/EmployeeAPI.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/employeeapi`
- Copy artifacts to EC2:
  - `scp -r publish/employeeapi ec2-user@<EC2_PUBLIC_IP>:~/app`
- On EC2: run the app and install Nginx for path `/employee`:
  - `sudo dnf -y install nginx`
  - `sudo systemctl enable --now nginx`
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
- DNS: point `longranch.com` A record to the EC2 public IP (or test first using the EC2 public DNS name instead of the domain).
- Verify: browse to `http://longranch.com/employee/swagger` (or the EC2 public DNS + `/employee/swagger`).
- Cost control: when done, stop or terminate the instance to avoid charges.

Later: add HTTPS with Let’s Encrypt, swap to App Runner/ECS, and automate with Harness/Terraform once the manual path is clear.
