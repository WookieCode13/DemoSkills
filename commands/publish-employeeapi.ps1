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

# Zip the published output
Write-Host "Zipping published artifacts..." -ForegroundColor Yellow
Compress-Archive -Path $output -DestinationPath $zipFile -Force
Write-Host "Zip complete: $zipFile" -ForegroundColor Green
