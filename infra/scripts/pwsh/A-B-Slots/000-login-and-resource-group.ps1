$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

if (-not (Get-AzContext)) {
    Connect-AzAccount | Out-Null
}

Set-AzContext -Subscription $Sub | Out-Null

$group = Get-AzResourceGroup -Name $RG -ErrorAction SilentlyContinue
if (-not $group) {
    $group = New-AzResourceGroup -Name $RG -Location $Loc -Tag $Tags
    Write-Host "Created resource group $RG"
}
else {
    Write-Host "Resource group $RG already exists"
}

$group
