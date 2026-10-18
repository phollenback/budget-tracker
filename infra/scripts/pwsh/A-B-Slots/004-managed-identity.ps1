$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$identity = Get-AzUserAssignedIdentity -ResourceGroupName $RG -Name $IdentityName -ErrorAction SilentlyContinue
if ($identity) {
    Write-Host "Managed identity $IdentityName already exists"
    $identity
    return
}

$identity = New-AzUserAssignedIdentity `
    -ResourceGroupName $RG `
    -Name $IdentityName `
    -Location $Loc `
    -Tag $Tags

Write-Host "Created user-assigned identity $IdentityName"
$identity
