$FFPath = Get-Content ffmpeg.json | ConvertFrom-Json
Write-Output $FFPath.bin