#!/usr/bin/env pwsh
# setup-employeeapi-ec2.ps1 - configure Amazon Linux EC2 host to serve EmployeeAPI

param(
    [string]$ZipPath = '/tmp/employeeapi-ec2.zip',
    [string]$AppDirectory = '/var/www/employeeapi',
    [string]$ServiceName = 'employeeapi',
    [string]$AppUser = 'ec2-user',
    [string]$DotnetRuntimePackage = 'dotnet-runtime-8.0'
)

function Assert-Root {
    $uid = (& id -u).Trim()
    if ($uid -ne '0') {
        Write-Error 'Run this script with sudo (root privileges).'
        exit 1
    }
}

function Invoke-Step {
    param(
        [string]$Message,
        [scriptblock]$Action
    )

    Write-Host "==> $Message" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "Step failed: $Message"
    }
}

Assert-Root

if (-not (Test-Path $ZipPath)) {
    Write-Error "Zip file not found at $ZipPath. Upload employeeapi-ec2.zip to the instance first."
    exit 1
}

$servicePath = "/etc/systemd/system/$ServiceName.service"
$nginxConf   = '/etc/nginx/conf.d/employeeapi.conf'

Invoke-Step 'Update system packages' { dnf update -y }
Invoke-Step 'Install .NET runtime, NGINX, unzip' { dnf install -y $DotnetRuntimePackage nginx unzip }

Invoke-Step "Prepare application directory $AppDirectory" {
    if (-not (Test-Path $AppDirectory)) {
        New-Item -ItemType Directory -Path $AppDirectory -Force | Out-Null
    } else {
        Get-ChildItem -Path $AppDirectory -Force | Remove-Item -Recurse -Force
    }
}

Invoke-Step 'Extract published zip' { unzip -oq $ZipPath -d $AppDirectory }
Invoke-Step 'Set ownership on app directory' { chown -R "$AppUser:$AppUser" $AppDirectory }

$serviceDefinition = @"
[Unit]
Description=EmployeeAPI
After=network.target

[Service]
WorkingDirectory=$AppDirectory
ExecStart=/usr/bin/dotnet $AppDirectory/EmployeeAPI.dll
Restart=always
Environment=ASPNETCORE_URLS=http://localhost:5000
User=$AppUser

[Install]
WantedBy=multi-user.target
"@

Invoke-Step 'Configure systemd service' {
    $serviceDefinition | Set-Content -Path $servicePath -Encoding UTF8
}

Invoke-Step 'Enable and start EmployeeAPI service' {
    systemctl daemon-reload
    systemctl enable --now $ServiceName
}

Invoke-Step 'Check EmployeeAPI service status' { systemctl status $ServiceName --no-pager }

$nginxConfig = @"
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade \$http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host \$host;
        proxy_cache_bypass \$http_upgrade;
    }
}
"@

Invoke-Step 'Write NGINX reverse proxy config' {
    $nginxConfig | Set-Content -Path $nginxConf -Encoding UTF8
}

Invoke-Step 'Enable and reload NGINX' {
    nginx -t
    systemctl enable --now nginx
    systemctl reload nginx
}

Invoke-Step 'Smoke test via curl' { curl -I http://127.0.0.1 }

Write-Host 'EmployeeAPI is deployed and fronted by NGINX.' -ForegroundColor Green
