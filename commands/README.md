# Commands

PowerShell helper to run the Employee API from the repo root.

PowerShell
- Run once: `pwsh -File commands/run-employee-api.ps1`
- Hot reload: `pwsh -File commands/run-employee-api.ps1 -Watch`
- Pick port (binds http+https): `pwsh -File commands/run-employee-api.ps1 -Watch -Port 5219`
- Override HTTPS port: `pwsh -File commands/run-employee-api.ps1 -Watch -Port 5219 -HttpsPort 7220`
- Release build: `pwsh -File commands/run-employee-api.ps1 -Configuration Release`

Notes
- Sets `ASPNETCORE_ENVIRONMENT=Development` automatically.
- When you set `-Port`, the script binds both `http://localhost:<Port>` and `https://localhost:<Port+1>` (or `-HttpsPort`).
- If you don’t set `-Port`, it uses launchSettings (typically `http://localhost:5016` and `https://localhost:7110`).
