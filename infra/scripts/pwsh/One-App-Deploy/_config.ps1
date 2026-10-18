# Shared values for the numbered one-app deploy steps. Edit this file; the steps read it.
# Separate from A-B-Slots so that solution can stay up while you test this one.

$Sub = "<your-subscription-id>"   # do not commit your Azure subscription id
$RG  = "budget-tracker-oneapp-rg"
$Loc = "westus2"

$Tags = @{
    env    = "prod"
    app    = "budgettracker-oneapp"
    deploy = "one-app"
}

$PlanName        = "budget-tracker-oneapp-plan"
$PlanTier        = "PremiumV3"
$PlanWorkerSize  = "Small"   # P1v3
$PlanWorkerCount = 1

$AppName         = "budgettracker-oneapp"
$LinuxFxVersion  = "DOTNETCORE|9.0"

$LawName             = "budgettracker-oneapp-law"
$LogRetentionInDays  = 30

$AppInsightsName = "budgettracker-oneapp"

$IdentityName = "budgettracker-oneapp-id"

$GitHubOrg         = "phollenback"
$GitHubRepo        = "budget-tracker"
$GitHubBranch      = "main"
$GitHubEnvironment = "oneapp"
$GitHubPublishProfileSecret = "AZURE_WEBAPP_PUBLISH_PROFILE_ONEAPP"

# Production-only host: both Yearly and Buckets nav/pages are on.
$ExperimentVariant = "All"
