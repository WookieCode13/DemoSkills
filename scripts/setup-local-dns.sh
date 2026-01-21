#!/usr/bin/env bash
set -euo pipefail

# Simple local DNS for DemoSkills using dnsmasq.
# Run this on the Linux box (wookie) and then point your router's DNS to this host.

IP_ADDRESS="${1:-192.168.0.94}"
CONF_PATH="/etc/dnsmasq.d/demoskills.conf"

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

echo "Done."
echo "Next: set your router's primary DNS to ${IP_ADDRESS}, then flush DNS on clients."
