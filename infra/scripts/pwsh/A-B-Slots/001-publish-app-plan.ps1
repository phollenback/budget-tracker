$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$group = Get-AzResourceGroup -Name $RG -ErrorAction SilentlyContinue
if (-not $group) {
    New-AzResourceGroup -Name $RG -Location $Loc -Tag $Tags | Out-Null
    Write-Host "Created resource group $RG"
}

$plan = Get-AzAppServicePlan -ResourceGroupName $RG -Name $PlanName -ErrorAction SilentlyContinue
if ($plan) {
    Write-Host "App Service plan $PlanName already exists"
    $plan
    return
}

$plan = New-AzAppServicePlan `
    -Location $Loc `
    -Name $PlanName `
    -ResourceGroupName $RG `
    -Tier $PlanTier `
    -WorkerSize $PlanWorkerSize `
    -NumberofWorkers $PlanWorkerCount `
    -Linux `
    -Tag $Tags

Write-Host "Created App Service plan $PlanName ($PlanTier $PlanWorkerSize Linux)"
$plan
