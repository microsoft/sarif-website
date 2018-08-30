<#
.SYNOPSIS
    Copy-Multitool

.DESCRIPTION
    This description copies the Sarif.Multitool binaries from the NuGet package directory to
    the Multitool subdirectory of this project so they can be published to the Web server,
    from where they can be accessed from the Web application.
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ToolName = "Sarif.Multitool"

$destinationPath = Join-Path (Split-Path -Parent $PSScriptRoot) $ToolName
$packagesRoot = Join-Path -Resolve $PSScriptRoot ..\..\packages

# Consult packages.config to determine which version of the package binaries we should
# copy. This is necessary because the packages directory might contain more than one
# version of the package.
function Get-MultitoolPackageVersion {
    $xPath = "/packages/package[@id='$ToolName']"
    $packagesConfigFilePath = Join-Path -Resolve $PSScriptRoot ..\packages.config
    $xml = Select-Xml -Path $packagesConfigFilePath -XPath $xPath
    if ($xml) {
        $xml.Node.version
    } else {
        $null
    }
}

$version = Get-MultitoolPackageVersion
if ($version -eq $null) {
    Write-Error "packages.config does not mention the $ToolName package!"
}

$toolPackageDirectoryName = "${ToolName}.$version"
$toolPackagePath = "$packagesRoot\$toolPackageDirectoryName"

if (-not (Test-Path $packagesRoot)) {
    Write-Error "Directory $toolPackagePath does not exist. Please perform a NuGet restore."
}

if (Test-Path $destinationPath) {
    Remove-Item -Recurse -Force $destinationPath
}

New-Item -Type Directory $destinationPath | Out-Null

$sourceFilePath = "$toolPackagePath\tools\net461\*"

Write-Information "Copying $sourceFilePath to ${destinationPath}..."
Copy-Item -Recurse -Path $sourceFilePath -Destination $destinationPath
Write-Information "Done."
