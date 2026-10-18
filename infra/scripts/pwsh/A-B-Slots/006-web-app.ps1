$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$plan = Get-AzAppServicePlan -ResourceGroupName $RG -Name $PlanName
if (-not $plan) {
    throw "App Service plan $PlanName not found. Run 001-publish-app-plan.ps1 first."
}

$identity = Get-AzUserAssignedIdentity -ResourceGroupName $RG -Name $IdentityName
if (-not $identity) {
    throw "Managed identity $IdentityName not found. Run 004-managed-identity.ps1 first."
}

$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName -ErrorAction SilentlyContinue
if (-not $app) {
    $app = New-AzWebApp `
        -ResourceGroupName $RG `
        -Name $AppName `
        -Location $Loc `
        -AppServicePlan $PlanName
    Write-Host "Created web app $AppName"
}

$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName
$app.HttpsOnly = $true
$app.SiteConfig.LinuxFxVersion = $LinuxFxVersion
$app.SiteConfig.AlwaysOn = $true
$app.SiteConfig.MinTlsVersion = "1.2"
$app.SiteConfig.FtpsState = "Disabled"
$app.SiteConfig.Http20Enabled = $true
Set-AzWebApp -WebApp $app | Out-Null

$sitePath = "/subscriptions/$Sub/resourceGroups/$RG/providers/Microsoft.Web/sites/${AppName}?api-version=2024-11-01"
$identityPayload = @{
    identity = @{
        type = "UserAssigned"
        userAssignedIdentities = @{
            $identity.Id = @{}
        }
    }
} | ConvertTo-Json -Depth 6

Invoke-AzRestMethod -Path $sitePath -Method PATCH -Payload $identityPayload | Out-Null

Write-Host "Configured web app $AppName ($LinuxFxVersion, HTTPS, Always On, user-assigned identity)"
Get-AzWebApp -ResourceGroupName $RG -Name $AppName
