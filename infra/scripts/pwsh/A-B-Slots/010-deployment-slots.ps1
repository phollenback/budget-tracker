$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

function Set-ExperimentSlot {
    param(
        [Parameter(Mandatory)] [string] $Slot,
        [Parameter(Mandatory)] [string] $Variant
    )

    $existing = Get-AzWebAppSlot -ResourceGroupName $RG -Name $AppName -Slot $Slot -ErrorAction SilentlyContinue
    if (-not $existing) {
        New-AzWebAppSlot -ResourceGroupName $RG -Name $AppName -Slot $Slot | Out-Null
        Write-Host "Created slot $Slot"
    }
    else {
        Write-Host "Slot $Slot already exists"
    }

    $slotApp = Get-AzWebAppSlot -ResourceGroupName $RG -Name $AppName -Slot $Slot
    $settings = @{}
    foreach ($setting in $slotApp.SiteConfig.AppSettings) {
        $settings[$setting.Name] = $setting.Value
    }

    $settings["BudgetExperiment__Variant"] = $Variant
    Set-AzWebAppSlot -ResourceGroupName $RG -Name $AppName -Slot $Slot -AppSettings $settings | Out-Null
    Write-Host "Set BudgetExperiment__Variant=$Variant on slot $Slot"
}

Set-ExperimentSlot -Slot $YearlySlot -Variant "Yearly"
Set-ExperimentSlot -Slot $BucketsSlot -Variant "Buckets"
