# Commands

PowerShell helper scripts to manage the Employee API from the repo root.

## Publish Scripts

### Publish EmployeeAPI
```powershell
pwsh -File commands/publish-employeeapi.ps1
```
- Output location: `publish/employeeapi`
- Creates a self-contained Release build for Linux x64
- Single executable file (no .NET installation needed on target)
- Ready for deployment to EC2 or other Linux environments

## Script Directory Structure

```
commands/
├── README.md                    # This file
└── publish-employeeapi.ps1      # Publish EmployeeAPI for Linux deployment
```
