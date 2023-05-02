Param(
    [Parameter(Mandatory=$true)]
    [string]
    $exeName,
	[Parameter(Mandatory=$true)]
	[string]
	$ProjectBuild,
	[Parameter(Mandatory=$false)]
    [string]
    $OutputDirectory = "C:\TEMP",
    [Parameter(Mandatory=$false)]
    [string]
    $DistributionTool = "C:\Program Files\Elgato\Stream Deck\DistributionTool.exe",
    [Parameter(Mandatory=$false)]
    [string]
    $StreamDeckExe = "C:\Program Files\Elgato\StreamDeck\StreamDeck.exe",
    [Parameter(Mandatory=$false)]
    [Int32]
    $LoadTimeout = 4
)


Write-Output "Installing the new build to StreamDeck";
Push-Location $PSScriptRoot
Write-Output "CWD: $(Get-Location)";
Write-Output "exeName: '$exeName'";
Write-Output "projectBuild: '$ProjectBuild'";
Write-Output "OutputDirectory: '$OutputDirectory'";
Write-Output "DistrbutionTool: '$DistributionTool'";
Write-Output "StreamDeckExe: '$StreamDeckExe'";
Write-Output "LoadTimeout: $LoadTimeout'";

Write-Output "Stopping 'streamdeck' process";
if (Get-Process -Name "streamdeck" -ErrorAction:SilentlyContinue) {
    Stop-Process -Name "streamdeck" -Force -ErrorAction:Stop;
}
$modifiedExe = $exeName.Replace(".exe", "");
Write-Output "Stopping '$modifiedExe' process";
if (Get-Process -Name $modifiedExe -ErrorAction:SilentlyContinue) {
    Stop-Process -Name $modifiedExe -Force -ErrorAction:Stop;
}
Write-Output "Sleeping for $LoadTimeout seconds";
Start-Sleep -Seconds $LoadTimeout;

Write-Output "Generating packed plugin name from '$ProjectBuild'...";
$partialName = (Get-Item -Path $ProjectBuild).Name;
if ($null -eq $partialName) {
    Write-Error "Expected to get partial name, however it was null.";
    Exit 1;
}
$PackedPluginName = $OutputDirectory + '\' + $partialName.Replace(".sdPlugin", ".streamDeckPlugin");
Write-Output "Packed plugin name: $PackedPluginName...";

Write-Output "Deleting Existing packed plugin in $PackedPluginName";
Remove-Item -Path $PackedPluginName -Force -Recurse -ErrorAction:Ignore;

Write-Output "Building (with '$DistributionTool') packed plugin from '$ProjectBuild' into '$OutputDirectory'";
Start-Process -FilePath $DistributionTool -ArgumentList "-b -i $ProjectBuild -o $OutputDirectory" -Wait -NoNewWindow -ErrorAction:Stop;

$currentPlugin = $env:APPDATA + "\Elgato\StreamDeck\Plugins\" + $partialName;
Write-Output "Removing existing plugin by the same name '$currentPlugin'";
if (Test-Path $currentPlugin) {
    Remove-Item -Path $currentPlugin -Force -Recurse -ErrorAction:Stop;
}

Write-Output "Starting Streamdeck";
Start-Process -FilePath $StreamDeckExe -ErrorAction:Stop;

Write-Output "Sleeping for $LoadTimeout seconds";
Start-Sleep -Seconds $LoadTimeout;

Write-Output "Installing Plugin to streamdeck...";
& $PackedPluginName;
Write-Output "Installed.";