#!/usr/bin/env pwsh
# publish-employeeapi.ps1 - publish EmployeeAPI

$project = Join-Path $PSScriptRoot '..\apis\EmployeeAPI\EmployeeAPI.csproj'
$output  = Join-Path $PSScriptRoot '..\publish\employeeapi'
$zipFile = Join-Path $PSScriptRoot '..\publish\employeeapi.zip'

# Publish
dotnet publish $project `
    -c Release `
    -r linux-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $output

Write-Host "Publish complete to: $output" -ForegroundColor Green

# Add Procfile for Elastic Beanstalk (self-contained binary)
$procfilePath = Join-Path $output 'Procfile'
Set-Content -Path $procfilePath -Value "web: ./EmployeeAPI"
Write-Host "Added Procfile for Elastic Beanstalk: $procfilePath" -ForegroundColor Yellow

# Zip the published output
Write-Host "Zipping published artifacts..." -ForegroundColor Yellow
Compress-Archive -Path $output -DestinationPath $zipFile -Force
Write-Host "Zip complete: $zipFile" -ForegroundColor Green
