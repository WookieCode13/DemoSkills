#!/usr/bin/env python3
import argparse
import os
import subprocess
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description="Run or watch the Employee API")
    parser.add_argument("--watch", action="store_true", help="Use dotnet watch for hot reload")
    parser.add_argument("--no-build", action="store_true", help="Skip build step")
    parser.add_argument("--configuration", default="Debug", help="Build configuration (Debug/Release)")
    parser.add_argument("--project", default="apis/EmployeeAPI/EmployeeAPI.csproj", help="Path to API csproj")
    parser.add_argument("--port", type=int, help="Port to bind (sets ASPNETCORE_URLS)")
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parent.parent
    env = os.environ.copy()
    env["ASPNETCORE_ENVIRONMENT"] = "Development"
    if args.port:
        env["ASPNETCORE_URLS"] = f"http://localhost:{args.port}"

    cmd = ["dotnet"]
    if args.watch:
        cmd += ["watch", "--project", args.project, "run", "--configuration", args.configuration]
    else:
        cmd += ["run", "--project", args.project, "--configuration", args.configuration]
    if args.no_build:
        cmd.append("--no-build")

    subprocess.run(cmd, cwd=repo_root, env=env, check=True)


if __name__ == "__main__":
    main()

