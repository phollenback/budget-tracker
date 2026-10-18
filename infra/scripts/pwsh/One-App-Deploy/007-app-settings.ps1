$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$insights = Get-AzApplicationInsights -ResourceGroupName $RG -Name $AppInsightsName
if (-not $insights) {
    throw "Application Insights $AppInsightsName not found. Run 003-app-insights.ps1 first."
}

$connectionString = $insights.ConnectionString
if (-not $connectionString) {
    $connectionString = $insights.Properties.ConnectionString
}
if (-not $connectionString) {
    throw "Application Insights connection string was empty."
}

$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName
if (-not $app) {
    throw "Web app $AppName not found. Run 006-web-app.ps1 first."
}

$settings = @{}
foreach ($setting in $app.SiteConfig.AppSettings) {
    $settings[$setting.Name] = $setting.Value
}

$settings["APPLICATIONINSIGHTS_CONNECTION_STRING"] = $connectionString
$settings["ApplicationInsightsAgent_EXTENSION_VERSION"] = "~3"
$settings["XDT_MicrosoftApplicationInsights_Mode"] = "Recommended"
$settings["BudgetExperiment__Variant"] = $ExperimentVariant

Set-AzWebApp -ResourceGroupName $RG -Name $AppName -AppSettings $settings | Out-Null
Write-Host "Set Application Insights and BudgetExperiment__Variant=$ExperimentVariant on $AppName"
