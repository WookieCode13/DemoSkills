Param(
  [string]$Region = $env:AWS_REGION,
  [string]$RepoName = $env:ECR_REPO_NAME
)

if (-not $Region) { $Region = "us-east-1" }
if (-not $RepoName) { $RepoName = "employee-api" }

Push-Location $PSScriptRoot
try {
  if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) {
    Write-Error "Terraform not found. Install Terraform and retry."
    exit 1
  }

  terraform destroy -auto-approve -var "region=$Region" -var "repo_name=$RepoName"
}
finally {
  Pop-Location
}

