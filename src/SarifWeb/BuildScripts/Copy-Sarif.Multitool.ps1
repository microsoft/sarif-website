<#
.SYNOPSIS
    Copy-Sarif.Multitool

.DESCRIPTION
    This script copies the Sarif.Multitool binaries from the NuGet package directory to
    the Multitool subdirectory of this project so they can be published to the Web server,
    from where they can be accessed from the Web application. It also copies the SARIF schema,
    which will be specified on the Sarif.Multitool command line.
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ToolPackageName = "Sarif.Multitool"
$SdkPackageName = "Sarif.Sdk"

$PackagesRoot = Join-Path -Resolve $PSScriptRoot ..\..\packages
if (-not (Test-Path $PackagesRoot)) {
    Write-Error "The NuGet packages directory $PackagesRoot does not exist. Please perform a NuGet restore."
}

$PackagesConfigFilePath = Join-Path -Resolve $PSScriptRoot ..\packages.config
if (-not (Test-Path $PackagesConfigFilePath)) {
    Write-Error "The NuGet package configuration file $PackagesConfigFilePath does not exist."
}

$DestinationPath = Join-Path (Split-Path -Parent $PSScriptRoot) $ToolPackageName
if (Test-Path $DestinationPath) {
    Remove-Item -Recurse -Force $DestinationPath
}

New-Item -Type Directory $DestinationPath | Out-Null

# Consult packages.config to determine which version of the specified package we should
# copy. This is necessary because the packages directory might contain more than one
# version of the package.
function Get-PackageVersion($packageName) {
    $xPath = "/packages/package[@id='$packageName']"
    $xml = Select-Xml -Path $PackagesConfigFilePath -XPath $xPath
    if ($xml) {
        $xml.Node.version
    } else {
        $null
    }
}

function Get-PackagePath($packageName) {
    $version = Get-PackageVersion $packageName
    if ($version -eq $null) {
        Write-Error "packages.config does not mention the $packageName package!"
    }

    "$PackagesRoot\${packageName}.$version"
}

$toolPackagePath = Get-PackagePath $ToolPackageName
$toolBinariesSourcePath = "$toolPackagePath\tools\net461\*"

Write-Verbose "Copying $ToolPackageName binaries from $toolBinariesSourcePath to ${DestinationPath}..."
Copy-Item -Recurse -Path $toolBinariesSourcePath -Destination $DestinationPath

$sdkPackagePath = Get-PackagePath $SdkPackageName
$sarifSchemaPath = "$sdkPackagePath\Schemata\sarif-schema.json"

Write-Verbose "Copying SARIF schema file from $sarifSchemaPath to ${DestinationPath}..."
Copy-Item -Path $sarifSchemaPath -Destination $DestinationPath

Write-Verbose "Done."
