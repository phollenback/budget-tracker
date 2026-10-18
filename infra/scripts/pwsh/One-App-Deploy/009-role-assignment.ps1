$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$identity = Get-AzUserAssignedIdentity -ResourceGroupName $RG -Name $IdentityName
$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName

$existing = Get-AzRoleAssignment `
    -ObjectId $identity.PrincipalId `
    -Scope $app.Id `
    -ErrorAction SilentlyContinue |
    Where-Object { $_.RoleDefinitionName -eq "Website Contributor" }

if ($existing) {
    Write-Host "Website Contributor already assigned to $IdentityName on $AppName"
    $existing
    return
}

$assignment = New-AzRoleAssignment `
    -ObjectId $identity.PrincipalId `
    -RoleDefinitionName "Website Contributor" `
    -Scope $app.Id

Write-Host "Assigned Website Contributor to $IdentityName on $AppName"
$assignment
