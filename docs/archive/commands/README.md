# Commands

PowerShell helper scripts to manage the Employee API from the repo root.

## Publish Scripts

### publish-employeeapi-ec2.ps1
```powershell
pwsh -File commands/publish-employeeapi-ec2.ps1
```
- Output: `publish/employeeapi-ec2` plus `publish/employeeapi-ec2.zip`
- Framework-dependent Release build (`linux-x64` default) ready for manual EC2 uploads
- Optional parameters: `-Runtime win-x64`, `-SelfContained $true`

### publish-employeeapi-beanstalk.ps1
```powershell
pwsh -File commands/publish-employeeapi-beanstalk.ps1
```
- Output: `publish/employeeapi` plus `publish/employeeapi.zip`
- Adds the Procfile that Elastic Beanstalk expects
- Same framework-dependent publish used in earlier deployments

### setup-employeeapi-ec2.ps1
*Run this on the EC2 instance after uploading the published zip.*
```powershell
sudo pwsh ./setup-employeeapi-ec2.ps1 -ZipPath /tmp/employeeapi-ec2.zip
```
- Installs system updates, .NET runtime, NGINX, and unzip
- Extracts the uploaded publish zip into `/var/www/employeeapi`
- Creates/starts the `employeeapi` systemd service and configures NGINX as the reverse proxy
- Ends with a `curl` smoke test so you can confirm the app is responding locally

## Script Directory Structure

```
commands/
|-- README.md                             # This file
|-- publish-employeeapi-beanstalk.ps1     # Publish EmployeeAPI for Elastic Beanstalk
|-- publish-employeeapi-ec2.ps1           # Publish EmployeeAPI zip for manual EC2 uploads
`-- setup-employeeapi-ec2.ps1             # Configure the EC2 host once the zip is uploaded
```
