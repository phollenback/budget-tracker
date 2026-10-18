$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

if ($RG -match '^(NetworkWatcherRG|DefaultResourceGroup-)') {
    throw "Refusing to delete protected group $RG."
}

$group = Get-AzResourceGroup -Name $RG -ErrorAction SilentlyContinue
if (-not $group) {
    Write-Host "Resource group $RG does not exist. Nothing to clean up."
    return
}

Write-Host "Subscription: $((Get-AzContext).Subscription.Name)"
Write-Host "Resource group: $RG"
Get-AzResource -ResourceGroupName $RG |
    Select-Object Name, ResourceType |
    Format-Table -AutoSize

$typed = Read-Host "Delete '$RG' and all one-app resources? Type the resource group name to confirm"
if ($typed -ne $RG) {
    Write-Host "Name did not match. Aborted."
    return
}

Write-Host "Deleting $RG..."
Remove-AzResourceGroup -Name $RG -Force
Write-Host "Resource group $RG has been deleted. The app hostname can stay reserved for a few minutes."
