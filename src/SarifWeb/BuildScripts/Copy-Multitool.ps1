<#
.SYNOPSIS
    Copy-Multitool

.DESCRIPTION
    This description copies the Sarif.Multitool binaries from the NuGet package directory to
    the Multitool subdirectory of this project so they can be published to the Web server,
    from where they can be accessed from the Web application.

.PARAMETER Version
    The version of the Sarif.Multitool NuGet package to copy.
#>

[CmdletBinding()]
param(
    [string]
    $Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ToolName = "Sarif.Multitool"

$destinationPath = Join-Path (Split-Path -Parent $PSScriptRoot) Multitool
$packagesRoot = Join-Path -Resolve $PSScriptRoot ..\..\packages

if (-not (Test-Path $packagesRoot)) {
    Write-Error "Directory $packagesRoot does not exist. Please perform a NuGet restore."
}

if (Test-Path $destinationPath) {
    Remove-Item -Recurse -Force $destinationPath
}

New-Item -Type Directory $destinationPath | Out-Null

[array]$multitoolPackageNames = Get-ChildItem -Path $packagesRoot -Directory `
    | Where-Object { $_.Name -like "${ToolName}.*" } `
    | Foreach-Object { $_.Name }

if ($multitoolPackageNames.Length -eq 0) {
    Write-Error "There are no $ToolName packages in ${packagesRoot}. Please perform a NuGet restore."
}

if ($Version) {
    $packageName = "${ToolName}.$Version"
} else {
    if ($multitoolPackageNames.Length -gt 1) {
        Write-Error "There is more than one $ToolName package in ${packagesRoot}. Either remove the packages you don't need, or specify the -Version parameter to this script."
    }

    $packageName = $multitoolPackageNames[0]
}

$packagePath = Join-Path $packagesRoot $packageName

if (-not (Test-Path $packagePath)) {
    Write-Error "The directory $packagePath does not exist. Please perform a NuGet restore or specify a different version of ${ToolName}."
}

$sourceFilePath = "$packagePath\tools\net461\*"

Write-Information "Copying $sourceFilePath to ${destinationPath}..."
Copy-Item -Recurse -Path $sourceFilePath -Destination $destinationPath
Write-Information "Done."
