$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$workspace = Get-AzOperationalInsightsWorkspace -ResourceGroupName $RG -Name $LawName -ErrorAction SilentlyContinue
if ($workspace) {
    Write-Host "Log Analytics workspace $LawName already exists"
    $workspace
    return
}

$workspace = New-AzOperationalInsightsWorkspace `
    -ResourceGroupName $RG `
    -Name $LawName `
    -Location $Loc `
    -Sku PerGB2018 `
    -RetentionInDays $LogRetentionInDays `
    -Tag $Tags

Write-Host "Created Log Analytics workspace $LawName"
$workspace
