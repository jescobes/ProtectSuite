# ProtectSuite Migration Guide

This guide helps you migrate the ProtectSuite project to a new machine with different SDK installation paths.

## Quick Start

1. **Run the setup script** to auto-detect SDK paths:
   ```powershell
   .\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig
   ```

2. **Build the project**:
   ```powershell
   .\scripts\build.ps1 -Configuration Debug64
   ```

## Manual Configuration

If auto-detection doesn't work, you can manually configure the SDK paths:

### Method 1: Environment Variables (Recommended)

Set these environment variables before building:

```powershell
# MSIPC SDK
$env:MSIPC_SDK_PATH = "C:\YourPath\To\MSIPC"
$env:MSIPC_INCLUDE_PATH = "C:\YourPath\To\MSIPC\inc"
$env:MSIPC_LIB_PATH = "C:\YourPath\To\MSIPC\lib"

# MIP SDK (if installed)
$env:MIP_SDK_PATH = "C:\YourPath\To\MIP"
$env:MIP_INCLUDE_PATH = "C:\YourPath\To\MIP\include"
$env:MIP_LIB_PATH = "C:\YourPath\To\MIP\lib"

# Build Tools
$env:MSBUILD_PATH = "C:\YourPath\To\MSBuild.exe"
```

### Method 2: Configuration File

Edit `config\sdk-paths.json`:

```json
{
  "msipc": {
    "sdkPath": "C:\\YourPath\\To\\MSIPC",
    "includePath": "C:\\YourPath\\To\\MSIPC\\inc",
    "libPath": "C:\\YourPath\\To\\MSIPC\\lib",
    "binPath": "C:\\YourPath\\To\\MSIPC\\bin",
    "redistPath": "C:\\YourPath\\To\\MSIPC\\redist"
  },
  "mip": {
    "sdkPath": "C:\\YourPath\\To\\MIP",
    "includePath": "C:\\YourPath\\To\\MIP\\include",
    "libPath": "C:\\YourPath\\To\\MIP\\lib",
    "binPath": "C:\\YourPath\\To\\MIP\\bin"
  },
  "buildTools": {
    "msbuildPath": "C:\\YourPath\\To\\MSBuild.exe",
    "vcPath": "C:\\YourPath\\To\\VC",
    "windowsSdkPath": "C:\\YourPath\\To\\WindowsKits"
  }
}
```

## SDK Detection

The setup script automatically searches for SDKs in these locations:

### MSIPC SDK
- `C:\Program Files\SDKs\MSIPC`
- `D:\Program Files\SDKs\MSIPC`
- `C:\Program Files (x86)\SDKs\MSIPC`
- `D:\Program Files (x86)\SDKs\MSIPC`
- Windows Registry: `HKLM:\SOFTWARE\Microsoft\MSIPC`

### MIP SDK
- `C:\Program Files\SDKs\MIP`
- `D:\Program Files\SDKs\MIP`
- `C:\Program Files (x86)\SDKs\MIP`
- `D:\Program Files (x86)\SDKs\MIP`

### Visual Studio Build Tools
- `C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe`
- `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe`
- `D:\BuildTools\MSBuild\Current\Bin\MSBuild.exe`
- Via `vswhere.exe` if available

### Windows SDK
- `C:\Program Files (x86)\Windows Kits\10`
- `C:\Program Files\Windows Kits\10`

## Project Structure

The project uses several mechanisms for portable path resolution:

### 1. Directory.Build.props
- Central configuration file for all projects
- Resolves SDK paths from environment variables
- Provides fallback to hardcoded paths
- Validates SDK existence before building

### 2. Environment Variables
- `MSIPC_SDK_PATH`: MSIPC SDK root directory
- `MIP_SDK_PATH`: MIP SDK root directory
- `MSBUILD_PATH`: MSBuild executable path
- `WINDOWS_SDK_PATH`: Windows SDK root directory

### 3. Configuration File
- `config\sdk-paths.json`: JSON configuration with all paths
- Used by build scripts for path resolution
- Can be updated via setup script

### 4. Build Scripts
- `scripts\setup-environment.ps1`: Auto-detects and configures paths
- `scripts\build.ps1`: Builds project with proper path resolution

## Troubleshooting

### Common Issues

1. **"MSIPC SDK not found"**
   - Run `.\scripts\setup-environment.ps1 -AutoDetect`
   - Manually set `$env:MSIPC_SDK_PATH`
   - Update `config\sdk-paths.json`

2. **"MSBuild not found"**
   - Install Visual Studio Build Tools
   - Set `$env:MSBUILD_PATH` to MSBuild location
   - Update build tools path in config

3. **"Windows SDK not found"**
   - Install Windows 10 SDK
   - Set `$env:WINDOWS_SDK_PATH`
   - Update Windows SDK path in config

4. **"MIP SDK not found"**
   - MIP SDK is optional for MSIPC projects
   - Install MIP SDK if you plan to use MIP projects
   - Set `$env:MIP_SDK_PATH` if installed

### Validation

To verify your configuration:

```powershell
# Show current SDK paths
.\scripts\build.ps1 -ShowSdkPaths

# Validate paths without building
.\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig
```

### Debug Information

Enable verbose output to see path resolution:

```powershell
.\scripts\build.ps1 -Configuration Debug64 -Verbose
```

## Migration Checklist

- [ ] Copy project files to new machine
- [ ] Install required SDKs (MSIPC, MIP, Build Tools, Windows SDK)
- [ ] Run `.\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig`
- [ ] Verify paths with `.\scripts\build.ps1 -ShowSdkPaths`
- [ ] Test build with `.\scripts\build.ps1 -Configuration Debug64`
- [ ] Update any custom paths in `config\sdk-paths.json` if needed

## Advanced Configuration

### Custom SDK Locations

If your SDKs are in non-standard locations:

1. **Set environment variables**:
   ```powershell
   $env:MSIPC_SDK_PATH = "C:\Custom\Path\To\MSIPC"
   ```

2. **Update configuration file**:
   ```json
   {
     "msipc": {
       "sdkPath": "C:\\Custom\\Path\\To\\MSIPC"
     }
   }
   ```

3. **Use build script parameters**:
   ```powershell
   .\scripts\setup-environment.ps1 -MsipcSdkPath "C:\Custom\Path\To\MSIPC" -UpdateConfig
   ```

### Multiple SDK Versions

To use specific SDK versions:

1. **Set version-specific paths**:
   ```powershell
   $env:MSIPC_SDK_PATH = "C:\SDKs\MSIPC\v2.0"
   ```

2. **Update project files** if needed for version-specific requirements

### Network/Shared SDKs

For shared SDK installations:

1. **Use UNC paths**:
   ```powershell
   $env:MSIPC_SDK_PATH = "\\server\share\SDKs\MSIPC"
   ```

2. **Map network drives**:
   ```powershell
   net use Z: \\server\share\SDKs
   $env:MSIPC_SDK_PATH = "Z:\MSIPC"
   ```

## Support

If you encounter issues during migration:

1. Check the troubleshooting section above
2. Verify SDK installations and paths
3. Run the setup script with verbose output
4. Check the build logs for specific error messages
5. Ensure all required dependencies are installed
