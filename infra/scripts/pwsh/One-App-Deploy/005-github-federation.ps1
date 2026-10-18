$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

function Set-GitHubFederatedCredential {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [string] $Subject
    )

    $existing = Get-AzFederatedIdentityCredential `
        -ResourceGroupName $RG `
        -IdentityName $IdentityName `
        -Name $Name `
        -ErrorAction SilentlyContinue

    if ($existing) {
        Write-Host "Federated credential $Name already exists"
        return $existing
    }

    $cred = New-AzFederatedIdentityCredential `
        -ResourceGroupName $RG `
        -IdentityName $IdentityName `
        -Name $Name `
        -Issuer "https://token.actions.githubusercontent.com" `
        -Subject $Subject `
        -Audience "api://AzureADTokenExchange"

    Write-Host "Created federated credential $Name"
    $cred
}

# Create these one at a time. Azure rejects concurrent FIC writes on one identity.
Set-GitHubFederatedCredential `
    -Name "github-$GitHubBranch" `
    -Subject "repo:${GitHubOrg}/${GitHubRepo}:ref:refs/heads/${GitHubBranch}"

Set-GitHubFederatedCredential `
    -Name "github-env-$GitHubEnvironment" `
    -Subject "repo:${GitHubOrg}/${GitHubRepo}:environment:${GitHubEnvironment}"
