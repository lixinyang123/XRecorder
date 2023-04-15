$LibFolder = "Recorder/lib"

if (Test-Path $LibFolder) {
    Remove-Item $LibFolder -Recurse -Force
}

if ($False -eq $(Test-Path $LibFolder)) {
    New-Item $LibFolder -ItemType Directory
}

function Download {
    param (
        $Url,
        $OutFile
    )

    Write-Output "`nDownload: $Url`n"
    Invoke-WebRequest $Url -OutFile $OutFile

    Write-Output "`nUnzip: $OutFile`n"
    Expand-Archive $OutFile -DestinationPath $LibFolder

    Remove-Item $OutFile -Force
}

$Dependences = Get-Content dependences.json | ConvertFrom-Json

if($IsWindows) {
    Download -Url $Dependences.ffmpeg.bin.win64 -OutFile $LibFolder/ffmpeg.zip
    Download -Url $Dependences.chromium.bin.win64 -OutFile $LibFolder/chromium.zip
}

if ($IsLinux) {
    Download -Url $Dependences.ffmpeg.bin.linux64 -OutFile $LibFolder/ffmpeg.zip
    Download -Url $Dependences.chromium.bin.linux64 -OutFile $LibFolder/chromium.zip

    chmod +x $LibFolder/ffmpeg
    chmod +x $LibFolder/chrome-linux/chrome
}

if($IsMacOS) {
    Download -Url $Dependences.ffmpeg.bin.osx64 -OutFile $LibFolder/ffmpeg.zip
    Download -Url $Dependences.chromium.bin.osx64 -OutFile $LibFolder/chromium.zip

    chmod +x $LibFolder/ffmpeg
    chmod +x $LibFolder/chrome-mac/Chromium.app/Contents/MacOS/Chromium
}

dotnet restore