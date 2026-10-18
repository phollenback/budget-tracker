$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$repo = "$GitHubOrg/$GitHubRepo"

$app = Get-AzWebApp -ResourceGroupName $RG -Name $AppName -ErrorAction SilentlyContinue
if (-not $app) {
    throw "Web app $AppName not found. Run 006-web-app.ps1 first."
}

function Get-SitePublishingProfileXml {
    $file = Join-Path ([System.IO.Path]::GetTempPath()) ("publish-" + [guid]::NewGuid().ToString("N") + ".xml")

    Get-AzWebAppPublishingProfile `
        -ResourceGroupName $RG `
        -Name $AppName `
        -Format WebDeploy `
        -OutputFile $file | Out-Null

    if (-not (Test-Path $file) -or (Get-Item $file).Length -eq 0) {
        throw "Publishing profile was empty for $AppName."
    }

    $file
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is required. Install it, then run: gh auth login"
}

gh auth status | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run: gh auth login"
}

$prodProfile = Get-SitePublishingProfileXml
try {
    gh api --method PUT "repos/$repo/environments/$GitHubEnvironment" --silent
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub environment $GitHubEnvironment."
    }
    Write-Host "Ensured GitHub environment $GitHubEnvironment"

    Get-Content -Raw $prodProfile | gh secret set $GitHubPublishProfileSecret --repo $repo
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set GitHub secret $GitHubPublishProfileSecret. Is gh logged in? (gh auth login)"
    }
    Write-Host "Set GitHub secret $GitHubPublishProfileSecret"
}
finally {
    Remove-Item $prodProfile -ErrorAction SilentlyContinue
}

Write-Host "Continuous deployment is .github/workflows/azure-one-app.yml"
Write-Host "  $GitHubBranch -> production $AppName (no slots)"
Write-Host "This secret is separate from A-B-Slots so that deploy is not overwritten."
