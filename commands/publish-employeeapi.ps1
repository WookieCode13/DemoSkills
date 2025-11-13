#!/usr/bin/env pwsh
# publish-employeeapi.ps1 - publish EmployeeAPI

$project = Join-Path $PSScriptRoot '..\apis\EmployeeAPI\EmployeeAPI.csproj'
$output  = Join-Path $PSScriptRoot '..\publish\employeeapi'
$zipFile = Join-Path $PSScriptRoot '..\publish\employeeapi.zip'

# Publish
# Clean previous output to avoid stale artifacts
if (Test-Path $output) { Remove-Item -Recurse -Force $output }
New-Item -ItemType Directory -Force -Path $output | Out-Null

# Framework-dependent publish (Elastic Beanstalk expects *.runtimeconfig.json)
dotnet publish $project `
    -c Release `
    -o $output

Write-Host "Publish complete to: $output" -ForegroundColor Green

# Add Procfile for Elastic Beanstalk (framework-dependent)
$procfilePath = Join-Path $output 'Procfile'
Set-Content -Path $procfilePath -Value "web: dotnet EmployeeAPI.dll"
Write-Host "Added Procfile for Elastic Beanstalk: $procfilePath" -ForegroundColor Yellow

# Basic validation
if (-not (Test-Path (Join-Path $output 'EmployeeAPI.runtimeconfig.json'))) {
  Write-Error 'Expected EmployeeAPI.runtimeconfig.json in publish output; framework-dependent publish may have failed.'
  exit 1
}

# Zip the contents of the publish folder (no top-level directory)
Write-Host "Zipping published artifacts (flattened to root)..." -ForegroundColor Yellow
if (Test-Path $zipFile) { Remove-Item -Force $zipFile }
Compress-Archive -Path (Join-Path $output '*') -DestinationPath $zipFile -Force
Write-Host "Zip complete: $zipFile" -ForegroundColor Green
