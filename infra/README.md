# Infrastructure

Everything this app needs in Azure, written as code. Pick a setup, run one command, and it's built. Run another, and it's gone.

## What's in here
- **`one-app/`**: one web app, every feature on.
- **`ab-slots/`**: one web app plus a `yearly` slot and a `buckets` slot. Visitors are split 50/50.
- **`modules/linux-web-app.bicep`**: the actual Azure resources, defined once and shared by both setups.
- **`scripts/`**: `deploy.sh`, `cleanup.sh`, and the step-by-step PowerShell scripts.
** **

## What gets created
- **Resource group**: the container that holds everything below.
- **App Service plan**: the Linux server (Premium v3, which supports slots).
- **Web app**: runs the app on .NET 9. HTTPS only, TLS 1.2+, FTP off, Always On.
- **Application Insights + Log Analytics**: monitoring and logs (kept 30 days).
- **Managed identity**: lets GitHub deploy with no stored password, and can only touch this one app.
- **Slots** *(A/B only)*: `yearly` and `buckets`, each with its own feature setting.
** **

## Deploy it
Sign in first with `az login`.

```bash
scripts/deploy.sh one-app      # or: ab-slots
```

This creates the infrastructure only. To push the code, run the publish step from the PowerShell scripts below, or push to GitHub.

<details>
<summary><b>Or build it step by step (PowerShell)</b></summary>

Scripts live in `scripts/pwsh/One-App-Deploy/` and `scripts/pwsh/A-B-Slots/`. Run them in number order. Each one skips anything that already exists, so re-running is safe.

**Same for both:**
- **000**: sign in and create the resource group.
- **001**: create the App Service plan.
- **002**: create Log Analytics.
- **003**: create Application Insights.
- **004**: create the managed identity.
- **005**: let GitHub sign in as that identity.
- **006**: create the web app.
- **007**: add app settings.
- **008**: turn off FTP publishing.
- **009**: give the identity permission to deploy.

**One app:**
- **010**: build and upload the app.
- **011**: connect GitHub for automatic deploys.
- **012**: delete everything.

**A/B slots:**
- **010**: create the two slots.
- **011**: build once, upload to production and both slots.
- **012**: split visitors 50/50.
- **013**: connect GitHub so each branch deploys to its slot.

</details>

## Change a setting
- **Bicep:** edit `main.bicepparam` in `one-app/` or `ab-slots/`.
- **PowerShell:** edit `_config.ps1` in that setup's script folder.

Names, region (`westus2`), server size and the traffic split all live there.

## Tear it down
```bash
scripts/cleanup.sh                                   # A/B slots
RG=budget-tracker-oneapp-rg scripts/cleanup.sh       # one app
```

It lists everything first and asks before deleting. Premium v3 isn't free, so run this when you're done.
