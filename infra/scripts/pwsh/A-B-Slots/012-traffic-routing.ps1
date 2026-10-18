$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$total = $YearlyTrafficPercent + $BucketsTrafficPercent
if ($total -lt 0 -or $total -gt 100) {
    throw "YearlyTrafficPercent + BucketsTrafficPercent must be between 0 and 100. Got $total."
}

$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName
if (-not $app) {
    throw "Web app $AppName not found. Run 006-web-app.ps1 first."
}

if (-not $app.ClientAffinityEnabled) {
    Set-AzWebApp -ResourceGroupName $RG -Name $AppName -ClientAffinityEnabled $true | Out-Null
    Write-Host "Enabled client affinity so visitors stay on one variant"
}

function Get-RequiredSlot {
    param(
        [Parameter(Mandatory)] [string] $Slot
    )

    $slotApp = Get-AzWebAppSlot -ResourceGroupName $RG -Name $AppName -Slot $Slot -ErrorAction SilentlyContinue
    if (-not $slotApp) {
        throw "Slot $Slot not found. Run 010-deployment-slots.ps1 first."
    }

    $slotApp
}

function New-RampUpRule {
    param(
        [Parameter(Mandatory)] [string] $Slot,
        [Parameter(Mandatory)] [int] $Percent
    )

    if ($Percent -le 0) {
        return $null
    }

    $slotApp = Get-RequiredSlot -Slot $Slot
    Write-Host "Routing $Percent% -> slot $Slot ($($slotApp.DefaultHostName))"

    @{
        name              = $Slot
        actionHostName    = $slotApp.DefaultHostName
        reroutePercentage = $Percent
    }
}

# Add-AzWebAppTrafficRouting rejects integer percents. Set both rules in one PATCH.
$rules = @(
    (New-RampUpRule -Slot $YearlySlot -Percent $YearlyTrafficPercent)
    (New-RampUpRule -Slot $BucketsSlot -Percent $BucketsTrafficPercent)
) | Where-Object { $null -ne $_ }

$path = "/subscriptions/$Sub/resourceGroups/$RG/providers/Microsoft.Web/sites/${AppName}/config/web?api-version=2024-11-01"
$payload = @{
    properties = @{
        experiments = @{
            rampUpRules = @($rules)
        }
    }
} | ConvertTo-Json -Depth 6

$result = Invoke-AzRestMethod -Path $path -Method PATCH -Payload $payload
if ($result.StatusCode -lt 200 -or $result.StatusCode -ge 300) {
    throw "Traffic routing update failed ($($result.StatusCode)): $($result.Content)"
}

$productionPercent = 100 - $total
Write-Host "Production slot keeps $productionPercent% of $($app.DefaultHostName) traffic"
Write-Host "Pinned visitors use the x-ms-routing-name cookie (yearly | buckets | self)"
