# Deployment Steps (Learning Log)

Goal: deploy EmployeeAPI to a single EC2 instance, route traffic through longranch.com, and keep everything inexpensive and repeatable. BeanStalk worked, but it tried to recreate infrastructure whenever I redeployed. This log is the reset plan for doing the same by hand with the AWS console.

## Quick notes

EC2 (Elastic Compute Cloud): Raw virtual machines in the cloud - I manage the OS, patching, scaling, and deployments myself. Think "AWS's version of a server."  
ECS (Elastic Container Service): AWS's container orchestration service. It runs Docker containers for me on a cluster of EC2 instances (or serverless using Fargate). I focus on containers, not OS.  
NGINX: lightweight web server/reverse proxy. I will eventually use it to forward HTTP traffic to Kestrel.  
VPC: Virtual Private Cloud (virtual network). I can use the AWS-provided default VPC for now.  
Subnet: Network segment. Use one of the default public subnets so the instance can have a public IPv4.  
Public IPv4: Enable auto-assign on the subnet, then later attach an Elastic IP so DNS never breaks.  
Internet Gateway: Provides outbound/Internet reachability. The default VPC already has it attached.

## High level plan

1. Baseline the default VPC and security groups so I know exactly what network path exists.
2. Publish the EmployeeAPI locally so I have an artifact that can be uploaded through the console.
3. Launch a small, inexpensive EC2 instance (Amazon Linux 2023 or Windows if absolutely needed) in the default VPC.
4. Connect through Session Manager/SSH, install the .NET runtime, and drop the published files in `/var/www/employeeapi`.
5. Configure systemd + NGINX so the API keeps running after reboot.
6. Allocate an Elastic IP and update Route 53 records for longranch.com to route traffic to the instance.

The next set of notes will show each step in more detail, using only the AWS console (and the EC2 instance shell once it is running).

## Step 1 - Baseline the network

1. In the AWS console open **VPC > Your VPCs** and confirm the default VPC exists in my preferred region.
2. Under **Subnets**, pick one of the default public subnets (where `Auto-assign public IPv4 address` is set to `Yes`). Edit it if needed so future EC2 instances automatically get a public IPv4.
3. Verify the default route table associated with that subnet has a `0.0.0.0/0` route that targets the Internet Gateway (igw-).
4. In **Security Groups**, create an `employeeapi-sg` with:
   - Inbound: HTTP (80) from 0.0.0.0/0, HTTPS (443) from 0.0.0.0/0 "Anywhere", and RDP/SSH locked to "My IP" if I want direct access. For remote shell without open ports, I can rely on Session Manager.
   - Outbound: allow all traffic so the instance can pull updates.
   - Later, if I front the API with an ALB or API Gateway, I would keep the public inbound rules on the load balancer security group and change the backend security group to only allow inbound traffic from the load balancer security group (or its private IP range). That way the app server is never exposed directly to the Internet.
5. Optional but recommended (actually required if I want Session Manager): create an IAM role + instance profile with the **AmazonSSMManagedInstanceCore** policy attached.
   - AWS Console path: **IAM > Roles > Create role > AWS service > EC2**.
   - Attach policy **AmazonSSMManagedInstanceCore** (and anything else I need, such as CloudWatch logs later).
   - Name it `employeeapi-ssm-role`, finish creation, then go to **EC2 > Instances > Actions > Security > Modify IAM role** and attach it to the instance (or choose it during launch).
   - Session Manager also needs the SSM Agent (already on Amazon Linux 2023) and an outbound Internet path. Confirm outbound access either by checking the route table for `0.0.0.0/0 -> igw-...` or by connecting over SSH and running `curl https://s3.amazonaws.com` (or `ping 8.8.8.8`). If that fails, the instance cannot reach SSM and the agent will stay offline.

## Step 2 - Prepare the EmployeeAPI artifact

1. Run `pwsh .\commands\publish-employeeapi-ec2.ps1` (defaults to `linux-x64`, framework-dependent). Override with `-Runtime win-x64` or `-SelfContained $true` if needed.
2. The script produces a folder `publish\employeeapi-ec2` and `publish\employeeapi-ec2.zip`. Upload the zip through the AWS console or SCP when the server is ready.
3. Keep a copy of `appsettings.json` with production values (connection strings, etc.). I can edit it on the server after upload if needed.

## Step 3 - Launch the EC2 instance

1. In the AWS console go to **EC2 > Instances > Launch instances**.
2. Name it `employeeapi-ec2`.
3. AMI: select **Amazon Linux 2023 (x86)**. The console currently shows kernel 6.1 and 6.12 variants—pick the latest general availability option (6.12 at the moment) so you get current drivers and patches. Both include the SSM agent.
4. Instance type: start with `t3.micro` (eligible for the free tier but still enough to test the API).
5. Key pair: create/download a new one only if I plan to SSH. Session Manager works without it when the IAM role is attached.
6. Network settings:
   - VPC: default.
   - Subnet: the public subnet from Step 1.
   - Auto-assign public IP: enabled.
   - Security group: select `employeeapi-sg`.
7. Storage: keep the default 8 GiB gp3 volume (I can expand later).
8. Advanced: attach the IAM instance profile with SSM permissions if I created it earlier.
9. Launch and wait until the status checks pass.

## Step 4 - Configure the OS

1. Connect via **EC2 > Connect > Session Manager**. No SSH key required. If I later prefer SSH, I can open port 22 to my IP and use the same commands (including PowerShell scripts) after copying them to the instance. Either way, once connected I can run any shell script to automate the remaining setup.
   - Optional automation: upload `publish\employeeapi-ec2.zip` to `/tmp/employeeapi-ec2.zip`, copy `commands\setup-employeeapi-ec2.ps1`, install PowerShell (`sudo dnf install -y powershell`), then run `sudo pwsh ./setup-employeeapi-ec2.ps1 -ZipPath /tmp/employeeapi-ec2.zip`. The script executes the rest of Step 4 and Step 5. Keep the manual instructions below as a reference when troubleshooting.
2. Update packages:
   ```bash
   sudo dnf update -y
   ```
3. Install the .NET runtime needed by EmployeeAPI (example for .NET 8):
   ```bash
   sudo dnf install -y dotnet-runtime-8.0
   ```
4. Install NGINX:
   ```bash
   sudo dnf install -y nginx
   ```
5. Create directories and copy the published artifact:
   ```bash
   sudo mkdir -p /var/www/employeeapi
    # Upload the zip from the console (S3 upload, SSM file copy, or scp) and unzip it here
    sudo unzip employeeapi-ec2.zip -d /var/www/employeeapi
   ```
6. Verify the API starts:
   ```bash
   cd /var/www/employeeapi
   dotnet EmployeeAPI.dll
   ```
   Once it runs locally, stop it (Ctrl+C). This confirms the binaries work on the instance.

## Step 5 - Keep the API running

1. Create a systemd service file `/etc/systemd/system/employeeapi.service`:
   ```
   [Unit]
   Description=EmployeeAPI
   After=network.target

   [Service]
   WorkingDirectory=/var/www/employeeapi
   ExecStart=/usr/bin/dotnet /var/www/employeeapi/EmployeeAPI.dll
   Restart=always
   Environment=ASPNETCORE_URLS=http://localhost:5000
   User=ec2-user

   [Install]
   WantedBy=multi-user.target
   ```
2. Enable and start it:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable --now employeeapi
   sudo systemctl status employeeapi
   ```
3. Configure NGINX as a reverse proxy by editing `/etc/nginx/conf.d/employeeapi.conf`:
   ```
   server {
       listen 80;
       server_name _;

       location / {
           proxy_pass         http://127.0.0.1:5000;
           proxy_http_version 1.1;
           proxy_set_header   Upgrade $http_upgrade;
           proxy_set_header   Connection keep-alive;
           proxy_set_header   Host $host;
           proxy_cache_bypass $http_upgrade;
       }
   }
   ```
4. Test and reload NGINX:
   ```bash
   sudo nginx -t
   sudo systemctl enable --now nginx
   sudo systemctl reload nginx
   ```
5. Confirm `curl http://localhost` on the instance returns the API response. Then hit the public IPv4 from a browser.

## Step 6 - Route longranch.com traffic

1. Allocate an Elastic IP in **EC2 > Network & Security > Elastic IPs** and associate it with `employeeapi-ec2`. This prevents the public IP from changing on reboots.
2. In Route 53:
   - If a hosted zone for `longranch.com` already exists, open it. Otherwise create a public hosted zone and update the registrar with the new name servers.
   - Create an `A` record (and optional `AAAA`) named `api.longranch.com` or the root record depending on what I want to expose. Point it to the Elastic IP.
3. Wait for DNS to propagate (usually a few minutes). Test with `nslookup api.longranch.com` and then `curl https://api.longranch.com`.
4. When ready for HTTPS, request a certificate in AWS Certificate Manager and set up NGINX with Let's Encrypt or use CloudFront/ALB to terminate TLS.

## Step 7 - Day-2 routines

Think about the work in three buckets: first-time setup, restarting the server after stopping it to save money, and deploying code changes.

### 1. First-time setup
- Run through Steps 1-6 above (or upload and run `setup-employeeapi-ec2.ps1`) so the OS, systemd service, and NGINX are configured once.
- All configuration lives on the EBS volume, so stopping/starting the instance later will not erase it.

### 2. Starting the server again after it was stopped
1. In the EC2 console choose **Actions > Instance state > Start**.
2. Make sure the Elastic IP is still associated (it should be if I attached it in Step 6).
3. Once the instance shows "running" and both status checks are green, connect via Session Manager or SSH.
4. The `employeeapi` service and NGINX should start automatically (systemd). Verify with:
   ```bash
   sudo systemctl status employeeapi
   sudo systemctl status nginx
   curl -I http://127.0.0.1
   ```
5. Hit the public DNS/Elastic IP from the browser to confirm routing still works.

### 3. Deploying code changes
1. Run the local publish script again: `pwsh .\commands\publish-employeeapi-ec2.ps1`.
2. Upload the new `employeeapi-ec2.zip` to the server (Session Manager file transfer, scp, S3 → wget, etc.).
3. On the instance:
   ```bash
   sudo systemctl stop employeeapi
   sudo unzip -o /tmp/employeeapi-ec2.zip -d /var/www/employeeapi
   sudo chown -R ec2-user:ec2-user /var/www/employeeapi
   sudo systemctl start employeeapi
   sudo systemctl status employeeapi
   sudo systemctl reload nginx   # only if NGINX config changed
   ```
4. Run the smoke test and then test externally.
5. If desired, re-run `setup-employeeapi-ec2.ps1 -ZipPath /tmp/employeeapi-ec2.zip` which automates steps 2-4 (it will reinstall packages if already present but that is harmless).

## Next steps (future log entries)

1. Snapshot the working EC2 instance or bake an AMI so I can rebuild quickly.
2. Automate uploads with S3 + CodeDeploy or a simple GitHub Action when I am ready.
3. Reuse the same VPC/security group when experimenting with ECS so networking remains identical.
