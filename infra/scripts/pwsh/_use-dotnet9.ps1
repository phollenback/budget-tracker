# Prefer a .NET 9 SDK. Interactive pwsh often resolves /usr/local/share/dotnet (8.x)
# ahead of the user-local 9.x install.

$sdkRoots = @(
    (Join-Path $HOME ".dotnet"),
    "/usr/local/share/dotnet",
    "/opt/homebrew/opt/dotnet/libexec"
)

$selectedRoot = $null
$selectedVersion = $null
foreach ($root in $sdkRoots) {
    $sdkDir = Join-Path $root "sdk"
    if (-not (Test-Path $sdkDir)) {
        continue
    }

    $sdk9 = Get-ChildItem -Path $sdkDir -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "9.*" } |
        Sort-Object { [version]$_.Name } -Descending |
        Select-Object -First 1

    if ($sdk9) {
        $selectedRoot = $root
        $selectedVersion = $sdk9.Name
        break
    }
}

if (-not $selectedRoot) {
    throw "A .NET 9 SDK is required to publish this app. Install it from https://aka.ms/dotnet/download"
}

$env:DOTNET_ROOT = $selectedRoot
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
if ($env:MSBuildSDKsPath -and $env:MSBuildSDKsPath -notmatch '[\\/]9\.') {
    Remove-Item Env:MSBuildSDKsPath
}
$env:PATH = "$selectedRoot$([IO.Path]::PathSeparator)$env:PATH"

Write-Host "Using .NET SDK $selectedVersion ($selectedRoot)"
