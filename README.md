# Budget Tracker

A paper monthly-budget worksheet turned into a web app. You enter your income and monthly expenses, and it shows whether you're hitting a 60/20/20 split between essentials, wants, and savings.

**Stack:** C# / .NET 9 · Blazor | Interactive Server render mode · Azure App Service · Bicep · GitHub Actions

**See it running:** screenshots of each page are in the [gallery](gallery/).

## Page Description

- **Worksheet** (`/`): enter a month's income and expenses, see your 60/20/20 split.
- **Yearly** (`/year`): all twelve months side by side.
- **Buckets** (`/buckets`): savings envelopes and how much each one got.

** **

## Two ways to deploy to Azure (`infra/`)

- **One app** (`infra/one-app/`): every feature in a single web app.
- **A/B slots** (`infra/ab-slots/`): production plus two slots, one showing Yearly and one showing Buckets, with visitors split 50/50.

Each approach can be deployed with Bicep (`infra/scripts/deploy.sh one-app`) or step by step with the numbered PowerShell scripts in `infra/scripts/pwsh/`.

Full details: [infra/README.md](infra/README.md)

**PowerShell steps**

**Same for both** (`One-App-Deploy/` and `A-B-Slots/`):

- **000**: sign in and create the resource group.
- **001**: create the App Service plan (the server).
- **002**: create Log Analytics (log storage).
- **003**: create Application Insights (monitoring).
- **004**: create a managed identity for deploys.
- **005**: let GitHub sign in to Azure with no password.
- **006**: create the web app (HTTPS only, FTP off).
- **007**: add app settings.
- **008**: turn off FTP publishing.
- **009**: give the identity permission to deploy this app only.

**One app** (`One-App-Deploy/`):

- **010**: build and upload the app.
- **011**: connect GitHub for automatic deploys.
- **012**: delete everything (asks to confirm).

**A/B slots** (`A-B-Slots/`):

- **010**: create the `yearly` and `buckets` slots.
- **011**: build once and upload to production and both slots.
- **012**: split visitors 50/50 between the slots.
- **013**: connect GitHub so each branch deploys to its slot.



** **

## Deploys

GitHub Actions runs on every push. It builds the app, runs the tests, and deploys only if every test passes.

Where it goes depends on the branch:

- `main` → production
- `feature/yearly-breakdown` → the `yearly` slot
- `feature/bucket-breakdown` → the `buckets` slot

So a feature can be tested live in its own slot without touching production.