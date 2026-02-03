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
  echo "Skipping git update. To switch branches, run: git fetch && git checkout <branch>"
else
  echo "Updating repo..."
  git pull
fi

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

# NOTE: Alembic migrations were removed from this script.
# TODO: Revisit Alembic for local runs (possibly via docker compose) if needed.

echo "Done."
echo "If DNS changes don't show up on clients, flush their DNS cache."
echo "Windows: run 'ipconfig /flushdns' in an elevated Command Prompt."
