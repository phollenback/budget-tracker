$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName -ErrorAction SilentlyContinue
if (-not $app) {
    throw "Web app $AppName not found. Run 006-web-app.ps1 first."
}

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

$staging = Join-Path ([System.IO.Path]::GetTempPath()) ("budget-tracker-oneapp-publish-" + [guid]::NewGuid().ToString("N"))
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

Write-Host "Deploying package to production ($AppName)"
Publish-AzWebApp -ResourceGroupName $RG -Name $AppName -ArchivePath $zip -Type zip -Timeout 300000 -Force | Out-Null
$site = Get-AzWebApp -ResourceGroupName $RG -Name $AppName
Write-Host "  https://$($site.DefaultHostName)"
Write-Host "Yearly and Buckets are both on this site (BudgetExperiment__Variant=$ExperimentVariant)"
