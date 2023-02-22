# Recorder

Screen capture build with ffmpeg.

### Prerequisites

Windows

```bash
Visual Studio 2022.
```

Ubuntu (20.04+)

```bash
sudo apt-get dotnet-sdk-7
```

### Restore dependences

```bash
pwsh initialize.ps1
```

### Compile

```bash
dotnet build
```

### Debug & Run

```bash
dotnet run
```

### Publish

```bash
dotnet publish -r <RID> -c Release --self-contained

# Build for Windows example
dotnet publish -r win-x64 -c Release --self-contained
```

### Publish & Builder installer

Publish script include the publish command.

```bash
pwsh ./publish.ps1
```
