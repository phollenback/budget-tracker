$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$workspace = Get-AzOperationalInsightsWorkspace -ResourceGroupName $RG -Name $LawName
if (-not $workspace) {
    throw "Log Analytics workspace $LawName not found. Run 002-log-analytics.ps1 first."
}

$insights = Get-AzApplicationInsights -ResourceGroupName $RG -Name $AppInsightsName -ErrorAction SilentlyContinue
if ($insights) {
    Write-Host "Application Insights $AppInsightsName already exists"
    $insights
    return
}

$insights = New-AzApplicationInsights `
    -ResourceGroupName $RG `
    -Name $AppInsightsName `
    -Location $Loc `
    -Kind web `
    -ApplicationType web `
    -WorkspaceResourceId $workspace.ResourceId

Write-Host "Created Application Insights $AppInsightsName"
$insights
