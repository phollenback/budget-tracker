$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$repoRoot = $PSScriptRoot
while ($repoRoot -and -not (Test-Path (Join-Path $repoRoot "BudgetTracker.sln"))) {
    $parent = Split-Path $repoRoot -Parent
    if ([string]::IsNullOrWhiteSpace($parent) -or $parent -eq $repoRoot) {
        break
    }
    $repoRoot = $parent
}

$project = Join-Path $repoRoot "src/BudgetTracker.Web/BudgetTracker.Web.csproj"
if (-not (Test-Path $project)) {
    throw "Project not found: $project"
}

$staging = Join-Path ([System.IO.Path]::GetTempPath()) ("budget-tracker-publish-" + [guid]::NewGuid().ToString("N"))
$publishDir = Join-Path $staging "webapp"
New-Item -ItemType Directory -Path $publishDir | Out-Null

Write-Host "Publishing $project"
. (Join-Path $PSScriptRoot "../_use-dotnet9.ps1")
dotnet publish $project --configuration Release --output $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

$zip = Join-Path $staging "webapp.zip"
Push-Location $publishDir
try {
    Compress-Archive -Path * -DestinationPath $zip -Force
}
finally {
    Pop-Location
}

function Publish-SiteTarget {
    param([string] $Slot)

    if ([string]::IsNullOrWhiteSpace($Slot)) {
        Write-Host "Deploying package to production ($AppName)"
        Publish-AzWebApp -ResourceGroupName $RG -Name $AppName -ArchivePath $zip -Type zip -Timeout 300000 -Force | Out-Null
        $site = Get-AzWebApp -ResourceGroupName $RG -Name $AppName
        Write-Host "  https://$($site.DefaultHostName)"
        return
    }

    $slotApp = Get-AzWebAppSlot -ResourceGroupName $RG -Name $AppName -Slot $Slot -ErrorAction SilentlyContinue
    if (-not $slotApp) {
        throw "Slot $Slot not found. Run 010-deployment-slots.ps1 first."
    }

    Write-Host "Deploying package to slot $Slot"
    Publish-AzWebApp -ResourceGroupName $RG -Name $AppName -Slot $Slot -ArchivePath $zip -Type zip -Timeout 300000 -Force | Out-Null
    Write-Host "  https://$($slotApp.DefaultHostName)"
}

Publish-SiteTarget
Publish-SiteTarget -Slot $YearlySlot
Publish-SiteTarget -Slot $BucketsSlot
