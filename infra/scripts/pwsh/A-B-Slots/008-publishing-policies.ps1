$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

function Set-PublishingPolicy {
    param(
        [Parameter(Mandatory)] [string] $PolicyName,
        [Parameter(Mandatory)] [bool] $Allow
    )

    $path = "/subscriptions/$Sub/resourceGroups/$RG/providers/Microsoft.Web/sites/$AppName/basicPublishingCredentialsPolicies/${PolicyName}?api-version=2024-11-01"
    $payload = @{ properties = @{ allow = $Allow } } | ConvertTo-Json -Compress
    Invoke-AzRestMethod -Path $path -Method PUT -Payload $payload | Out-Null
    Write-Host "Set $PolicyName publishing credentials allow=$Allow"
}

Set-PublishingPolicy -PolicyName "ftp" -Allow $false
Set-PublishingPolicy -PolicyName "scm" -Allow $true
