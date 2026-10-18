#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
RG="${RG:-budget-tracker-prod-rg}"
LOCATION="${LOCATION:-westus2}"

az group create --name "$RG" --location "$LOCATION"
az deployment group create \
  --resource-group "$RG" \
  --template-file "$SCRIPT_DIR/main.bicep" \
  --parameters "$SCRIPT_DIR/main.bicepparam"
