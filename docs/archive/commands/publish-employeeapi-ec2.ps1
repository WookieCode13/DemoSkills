#!/usr/bin/env pwsh
# publish-employeeapi-ec2.ps1 - publish EmployeeAPI for manual EC2 uploads

param(
    [string]$Runtime = 'linux-x64',
    [bool]$SelfContained = $false
)

$project  = Join-Path $PSScriptRoot '..\apis\EmployeeAPI\EmployeeAPI.csproj'
$output   = Join-Path $PSScriptRoot '..\publish\employeeapi-ec2'
$zipFile  = Join-Path $PSScriptRoot '..\publish\employeeapi-ec2.zip'

if (-not (Test-Path $project)) {
    Write-Error "Project file not found: $project"
    exit 1
}

Write-Host "Publishing EmployeeAPI ($Runtime, SelfContained=$SelfContained)..." -ForegroundColor Cyan

if (Test-Path $output) { Remove-Item -Recurse -Force $output }
New-Item -ItemType Directory -Force -Path $output | Out-Null

$publishArgs = @(
    'publish', $project,
    '-c', 'Release',
    '-r', $Runtime,
    '--self-contained', $SelfContained.ToString().ToLower(),
    '-o', $output
)

$publishResult = dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error 'dotnet publish failed'
    exit $LASTEXITCODE
}

Write-Host 'Publish complete.' -ForegroundColor Green

$runtimeConfig = Join-Path $output 'EmployeeAPI.runtimeconfig.json'
if (-not (Test-Path $runtimeConfig)) {
    Write-Warning 'runtimeconfig.json not found; did you publish framework-dependent?'
}

if (Test-Path $zipFile) { Remove-Item -Force $zipFile }
Write-Host "Creating zip: $zipFile" -ForegroundColor Yellow
Compress-Archive -Path (Join-Path $output '*') -DestinationPath $zipFile -Force
Write-Host 'Zip complete.' -ForegroundColor Green

Write-Host ''
Write-Host 'Next steps:' -ForegroundColor Cyan
Write-Host " 1. Upload $zipFile via the AWS console or scp." -ForegroundColor Cyan
Write-Host ' 2. Unzip into /var/www/employeeapi on the EC2 instance.' -ForegroundColor Cyan