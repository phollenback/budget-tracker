# Shared values for the numbered deploy steps. Edit this file; the steps read it.

$Sub = "<your-subscription-id>"   # do not commit your Azure subscription id
$RG  = "budget-tracker-prod-rg"
$Loc = "westus2"

$Tags = @{
    env = "prod"
    app = "budgettracker-ab"
}

$PlanName        = "budget-tracker-prod-plan"
$PlanTier        = "PremiumV3"
$PlanWorkerSize  = "Small"   # P1v3
$PlanWorkerCount = 1

$AppName         = "budgettracker-ab"
$LinuxFxVersion  = "DOTNETCORE|10.0"

$LawName             = "budgettracker-law"
$LogRetentionInDays  = 30

$AppInsightsName = "budgettracker"

$IdentityName = "budgettracker-id"

$GitHubOrg         = "phollenback"
$GitHubRepo        = "budget-tracker"
$GitHubBranch      = "main"
$GitHubEnvironment = "production"

$YearlySlot  = "yearly"
$BucketsSlot = "buckets"

# Share of production-URL traffic sent to each experiment slot.
# Remainder stays on the production slot (worksheet-only when Variant is Off).
# 50 + 50 = 100 means no production-slot traffic.
$YearlyTrafficPercent  = 50
$BucketsTrafficPercent = 50
