#!/usr/bin/env bash
set -euo pipefail

# Simple local DNS + local deploy for DemoSkills using dnsmasq and docker compose.
# Run this from ~/repos/DemoSkills on the Linux box (wookie).

IP_ADDRESS="${1:-192.168.0.94}"
CONF_PATH="/etc/dnsmasq.d/demoskills.conf"
REPO_DIR="${REPO_DIR:-$PWD}"

cd "$REPO_DIR"

echo "Repo: $REPO_DIR"
current_branch="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")"
echo "Current git branch: $current_branch"
read -r -p "Continue with this branch? (y/N): " confirm_branch
confirm_branch="${confirm_branch:-n}"
if [[ ! "$confirm_branch" =~ ^[Yy]$ ]]; then
  echo "Canceled. To switch branches, run: git fetch && git checkout <branch>"
  exit 0
fi

echo "Updating repo..."
git pull

read -r -p "Refresh dnsmasq mappings? (y/N): " refresh_dns
refresh_dns="${refresh_dns:-n}"
if [[ "$refresh_dns" =~ ^[Yy]$ ]]; then
  if ! command -v dnsmasq >/dev/null 2>&1; then
    echo "dnsmasq not found. Installing..."
    if command -v apt-get >/dev/null 2>&1; then
      sudo apt-get update
      sudo apt-get install -y dnsmasq
    else
      echo "Unsupported package manager. Install dnsmasq manually and re-run."
      exit 1
    fi
  fi

  echo "Writing ${CONF_PATH} (IP ${IP_ADDRESS})"
  sudo tee "${CONF_PATH}" >/dev/null <<EOF
address=/longranch.wookie/${IP_ADDRESS}
address=/employee.longranch.wookie/${IP_ADDRESS}
address=/company.longranch.wookie/${IP_ADDRESS}
address=/pay.longranch.wookie/${IP_ADDRESS}
address=/tax.longranch.wookie/${IP_ADDRESS}
address=/report.longranch.wookie/${IP_ADDRESS}
EOF

  echo "Restarting dnsmasq..."
  sudo systemctl restart dnsmasq
fi

echo "Starting docker compose..."
read -r -p "Force recreate containers? (y/N): " force_recreate
force_recreate="${force_recreate:-n}"
if [[ "$force_recreate" =~ ^[Yy]$ ]]; then
  docker compose up --build -d --force-recreate
else
  docker compose up --build -d
fi

ensure_alembic() {
  if ! command -v alembic >/dev/null 2>&1; then
    echo "alembic not found. Installing CompanyAPI requirements..."
    if command -v pip >/dev/null 2>&1; then
      pip install -r apis/CompanyAPI/requirements.txt
    else
      echo "pip not found. Install Python/pip and re-run."
      exit 1
    fi
  fi
}

if [ -f "apis/CompanyAPI/alembic.ini" ]; then
  ensure_alembic
  if [ -z "${DemoSkills_DATABASE_URL:-}" ]; then
    echo "DemoSkills_DATABASE_URL not set. Set it before running migrations."
    exit 1
  fi
  echo "Running CompanyAPI migrations..."
  (cd apis/CompanyAPI && DATABASE_URL="${DemoSkills_DATABASE_URL}" alembic -c alembic.ini upgrade head)
fi

if [ -f "apis/ReportAPI/alembic.ini" ]; then
  ensure_alembic
  if [ -z "${DemoSkills_DATABASE_URL:-}" ]; then
    echo "DemoSkills_DATABASE_URL not set. Set it before running migrations."
    exit 1
  fi
  echo "Running ReportAPI migrations..."
  (cd apis/ReportAPI && DATABASE_URL="${DemoSkills_DATABASE_URL}" alembic -c alembic.ini upgrade head)
fi

echo "Done."
echo "If DNS changes don't show up on clients, flush their DNS cache."
echo "Windows: run 'ipconfig /flushdns' in an elevated Command Prompt."
