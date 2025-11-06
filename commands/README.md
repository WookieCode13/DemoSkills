# Commands

Helper scripts to run the Employee API from the repo root.

PowerShell
- Run once: `pwsh -File commands/run-employee-api.ps1`
- Hot reload: `pwsh -File commands/run-employee-api.ps1 -Watch`
- Pick port: `pwsh -File commands/run-employee-api.ps1 -Watch -Port 5219`
- Release build: `pwsh -File commands/run-employee-api.ps1 -Configuration Release`

Python
- Run once: `python commands/run-employee-api.py`
- Hot reload: `python commands/run-employee-api.py --watch`
- Pick port: `python commands/run-employee-api.py --watch --port 5219`
- Release build: `python commands/run-employee-api.py --configuration Release`

Notes
- Sets `ASPNETCORE_ENVIRONMENT=Development` automatically.
- Override port with `--port` to avoid conflicts.
- For debugging in VS Code, consider adding a `launch.json` later; these scripts are for quick run/watch.
