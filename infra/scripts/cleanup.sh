#!/usr/bin/env bash
set -euo pipefail

RG="${RG:-budget-tracker-prod-rg}"
EXPECTED_RG="budget-tracker-prod-rg"
YES=0
WAIT=1

usage() {
  cat <<EOF
Usage: $(basename "$0") [--yes] [--no-wait]

Deletes resource group '$EXPECTED_RG' and every resource in it
(App Service, plan, Application Insights, Log Analytics, identity).

  --yes       Skip the confirmation prompt
  --no-wait   Return as soon as Azure accepts the delete

Override the group with RG=name if needed. The script will not
delete NetworkWatcherRG or DefaultResourceGroup-* groups.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --yes|-y) YES=1 ;;
    --no-wait) WAIT=0 ;;
    -h|--help) usage; exit 0 ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
  shift
done

protected_group() {
  case "$1" in
    NetworkWatcherRG|DefaultResourceGroup-*) return 0 ;;
    *) return 1 ;;
  esac
}

if [[ -z "$RG" ]]; then
  echo "RG is empty. Aborting." >&2
  exit 1
fi

if protected_group "$RG"; then
  echo "Refusing to delete protected group '$RG'." >&2
  exit 1
fi

if ! az account show >/dev/null 2>&1; then
  echo "Not logged in to Azure. Run: az login" >&2
  exit 1
fi

if [[ "$(az group exists --name "$RG")" != "true" ]]; then
  echo "Resource group '$RG' does not exist. Nothing to clean up."
  exit 0
fi

SUBSCRIPTION="$(az account show --query name -o tsv)"
echo "Subscription: $SUBSCRIPTION"
echo "Resource group: $RG"
echo "Resources:"
az resource list --resource-group "$RG" --query "[].{name:name, type:type}" -o table
echo

if [[ "$RG" != "$EXPECTED_RG" && "$YES" -ne 1 ]]; then
  echo "This is not '$EXPECTED_RG'."
  read -r -p "Type the resource group name to confirm: " typed
  if [[ "$typed" != "$RG" ]]; then
    echo "Name did not match. Aborting." >&2
    exit 1
  fi
elif [[ "$YES" -ne 1 ]]; then
  read -r -p "Delete '$RG' and all resources in it? [y/N] " reply
  if [[ ! "$reply" =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 0
  fi
fi

echo "Deleting '$RG'..."
az group delete --name "$RG" --yes --no-wait

if [[ "$WAIT" -eq 1 ]]; then
  echo "Waiting until the group is gone (the app name can stay reserved for a few minutes after this)..."
  az group wait --deleted --resource-group "$RG"
  echo "Resource group '$RG' has been deleted."
else
  echo "Delete started. Check with: az group wait --deleted --resource-group $RG"
fi
