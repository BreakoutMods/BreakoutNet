param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue

if (-not $dotnet) {
    $fallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $fallback) {
        $dotnet = $fallback
    }
}

if (-not $dotnet) {
    throw "dotnet SDK was not found on PATH or at C:\Program Files\dotnet\dotnet.exe"
}

& $dotnet build (Join-Path $root "BreakoutNet.sln") -c $Configuration

if ($Deploy) {
    $pluginDir = Join-Path $root "..\..\BepInEx\plugins\BreakoutNet"
    New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
    Copy-Item -Force (Join-Path $root "src\BreakoutNet\bin\$Configuration\net462\BreakoutNet.dll") $pluginDir
    Write-Host "Deployed BreakoutNet.dll to $pluginDir"
}
