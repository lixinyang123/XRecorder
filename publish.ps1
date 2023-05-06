Write-Output "===================================="
Write-Output "|         Recorder Builder         |"
Write-Output "===================================="

# ====================== Check Platform =====================

$Platform = "UnSupport"

if($IsWindows) {
    $Platform = "win-x64"
}

if ($IsLinux) {
    $Platform = "linux-x64"
    Throw "UnSupport Linux Installer"
}

if($IsMacOS) {
    $Platform = "osx-x64"
    Throw "UnSupport MacOS Installer"
}

if ("UnSupport" -eq $Platform) {
    Throw "UnSupport Platform"
}

# ======================== Build App ========================

$LibFolder = "Recorder/lib"
$PublishFolder = "Recorder/bin/Release/net7.0/$Platform/publish"

if (Test-Path $PublishFolder) {
    Remove-Item $PublishFolder -Recurse -Force
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