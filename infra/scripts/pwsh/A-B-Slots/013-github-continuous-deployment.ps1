$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_config.ps1")

Set-AzContext -Subscription $Sub | Out-Null

$repo = "$GitHubOrg/$GitHubRepo"

function Get-SitePublishingProfileXml {
    param([string] $Slot)

    $file = Join-Path ([System.IO.Path]::GetTempPath()) ("publish-" + [guid]::NewGuid().ToString("N") + ".xml")

    if ([string]::IsNullOrWhiteSpace($Slot)) {
        Get-AzWebAppPublishingProfile `
            -ResourceGroupName $RG `
            -Name $AppName `
            -Format WebDeploy `
            -OutputFile $file | Out-Null
    }
    else {
        Get-AzWebAppSlotPublishingProfile `
            -ResourceGroupName $RG `
            -Name $AppName `
            -Slot $Slot `
            -Format WebDeploy `
            -OutputFile $file | Out-Null
    }

    if (-not (Test-Path $file) -or (Get-Item $file).Length -eq 0) {
        throw "Publishing profile was empty for slot '$Slot'."
    }

    $file
}

function Set-GitHubPublishProfileSecret {
    param(
        [Parameter(Mandatory)] [string] $SecretName,
        [Parameter(Mandatory)] [string] $ProfilePath
    )

    Get-Content -Raw $ProfilePath | gh secret set $SecretName --repo $repo
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set GitHub secret $SecretName. Is gh logged in? (gh auth login)"
    }

    Write-Host "Set GitHub secret $SecretName"
}

function Set-GitHubEnvironment {
    param([Parameter(Mandatory)] [string] $Name)

    gh api --method PUT "repos/$repo/environments/$Name" --silent
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub environment $Name."
    }

    Write-Host "Ensured GitHub environment $Name"
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is required. Install it, then run: gh auth login"
}

gh auth status | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run: gh auth login"
}

$prodProfile = Get-SitePublishingProfileXml
$yearlyProfile = Get-SitePublishingProfileXml -Slot $YearlySlot
$bucketsProfile = Get-SitePublishingProfileXml -Slot $BucketsSlot

try {
    Set-GitHubEnvironment -Name "production"
    Set-GitHubEnvironment -Name $YearlySlot
    Set-GitHubEnvironment -Name $BucketsSlot

    Set-GitHubPublishProfileSecret -SecretName "AZURE_WEBAPP_PUBLISH_PROFILE" -ProfilePath $prodProfile
    Set-GitHubPublishProfileSecret -SecretName "AZURE_WEBAPP_PUBLISH_PROFILE_YEARLY" -ProfilePath $yearlyProfile
    Set-GitHubPublishProfileSecret -SecretName "AZURE_WEBAPP_PUBLISH_PROFILE_BUCKETS" -ProfilePath $bucketsProfile
}
finally {
    Remove-Item $prodProfile, $yearlyProfile, $bucketsProfile -ErrorAction SilentlyContinue
}

Write-Host "Continuous deployment is the existing GitHub Action:"
Write-Host "  $GitHubBranch -> production"
Write-Host "  feature/yearly-breakdown -> slot $YearlySlot"
Write-Host "  feature/bucket-breakdown -> slot $BucketsSlot"
Write-Host "Push those branches after this step to confirm Actions can deploy."
