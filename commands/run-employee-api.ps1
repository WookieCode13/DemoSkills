param(
    [switch]$Watch,
    [switch]$NoBuild,
    [string]$Configuration = "Debug",
    [string]$Project = "apis/EmployeeAPI/EmployeeAPI.csproj",
    [int]$Port,
    [int]$HttpsPort
)

$ErrorActionPreference = 'Stop'

# Move to repo root (script is expected in repoRoot/commands)
Push-Location (Join-Path $PSScriptRoot '..')
try {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    if ($Port) {
        if (-not $HttpsPort) { $HttpsPort = $Port + 1 }
        $env:ASPNETCORE_URLS = "http://localhost:$Port;https://localhost:$HttpsPort"
    }

    if ($Watch) {
        # Hot reload on file changes
        if ($NoBuild) {
            dotnet watch --project $Project run --configuration $Configuration --no-build
        } else {
            dotnet watch --project $Project run --configuration $Configuration
        }
    } else {
        if ($NoBuild) {
            dotnet run --project $Project --configuration $Configuration --no-build
        } else {
            dotnet run --project $Project --configuration $Configuration
        }
    }
}
finally {
    Pop-Location | Out-Null
}

# USAGE EXAMPLES
#   pwsh -File commands/run-employee-api.ps1
#   pwsh -File commands/run-employee-api.ps1 -Watch
#   pwsh -File commands/run-employee-api.ps1 -Watch -Port 5219   # binds http://localhost:5219 and https://localhost:5220
#   pwsh -File commands/run-employee-api.ps1 -Watch -Port 5219 -HttpsPort 7220
#   pwsh -File commands/run-employee-api.ps1 -Configuration Release
