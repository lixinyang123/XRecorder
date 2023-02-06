Write-Output "===================================="
Write-Output "|         Recorder Builder         |"
Write-Output "===================================="

$Platform = "UnSupport"

if($IsWindows) {
    $Platform = "win-x64"
}

if ($IsLinux) {
    $Platform = "linux-x64"
}

if($IsMacOS) {
    $Platform = "osx-x64"
}

if ("UnSupport" -eq $Platform) {
    Throw "UnSupport Platform"
}

$LibFolder = "Recorder/lib"
$PublishFolder = "Recorder/bin/Release/net7.0/$Platform/publish"

if (Test-Path $PublishFolder) {
    Remove-Item $PublishFolder -Recurse
}

Copy-Item -Path $LibFolder -Destination $PublishFolder -Recurse

dotnet publish -r $Platform -c Release --self-contained

# ====================== Build Setup App ======================

if($IsWindows) {
    makensis installer.nsi
}

if ($IsLinux) {
    # build appimage
}

if($IsMacOS) {
    # build dmg
}