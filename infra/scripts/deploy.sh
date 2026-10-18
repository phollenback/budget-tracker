#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
INFRA_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
SOLUTION="${1:-}"

usage() {
  echo "Usage: $(basename "$0") <one-app|ab-slots>" >&2
  echo "Deploys Bicep only (no zip). Publish code with the matching pwsh 010/011 script." >&2
}

case "$SOLUTION" in
  one-app)
    RG="${RG:-budget-tracker-oneapp-rg}"
    TEMPLATE="$INFRA_DIR/one-app/main.bicep"
    PARAMS="$INFRA_DIR/one-app/main.bicepparam"
    ;;
  ab-slots)
    RG="${RG:-budget-tracker-prod-rg}"
    TEMPLATE="$INFRA_DIR/ab-slots/main.bicep"
    PARAMS="$INFRA_DIR/ab-slots/main.bicepparam"
    ;;
  *)
    usage
    exit 1
    ;;
esac

LOCATION="${LOCATION:-westus2}"

az group create --name "$RG" --location "$LOCATION"
az deployment group create \
  --resource-group "$RG" \
  --template-file "$TEMPLATE" \
  --parameters "$PARAMS"

echo "Bicep deployed for $SOLUTION in $RG."
if [[ "$SOLUTION" == "one-app" ]]; then
  echo "Publish code with: infra/scripts/pwsh/One-App-Deploy/010-publish-site.ps1"
else
  echo "Publish code with: infra/scripts/pwsh/A-B-Slots/011-publish-site.ps1"
fi
