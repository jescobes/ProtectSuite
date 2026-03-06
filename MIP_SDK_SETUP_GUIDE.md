# MIP SDK Setup Guide

This guide helps you install or set up the Microsoft Information Protection (MIP) SDK for the ProtectSuite project.

## Installation Options

You have **three options** for setting up the MIP SDK:

### Option 1: Standard Installation (Recommended)

**Best for**: First-time setup, full SDK features

1. **Download the MIP SDK**:
   - Visit: https://aka.ms/mipsdk
   - Or search for "Microsoft Information Protection SDK download"
   - Download the Windows C++ SDK installer

2. **Run the Installer**:
   ```powershell
   # Run as Administrator
   # Install to: D:\Program Files\SDKs\MIP
   ```

3. **Verify Installation**:
   ```powershell
   .\scripts\setup-mip-sdk.ps1 -VerifyInstallation
   ```

### Option 2: Manual File Copy (Quick Setup)

**Best for**: If you have MIP SDK files from another installation or need a minimal setup

1. **Create Directory Structure**:
   ```powershell
   .\scripts\setup-mip-sdk.ps1 -CreateStructure
   ```

2. **Copy SDK Files**:
   If you have MIP SDK files from another location:
   ```powershell
   .\scripts\setup-mip-sdk.ps1 -SdkSourcePath "C:\Path\To\Source\MIP" -TargetPath "D:\Program Files\SDKs\MIP"
   ```

   Or manually copy files to:
   ```
   D:\Program Files\SDKs\MIP\
   ├── include\          # Header files (.h)
   │   └── mip\
   │       ├── file\
   │       ├── protection\
   │       └── policy\
   ├── lib\              # Library files (.lib)
   │   ├── x64\
   │   └── x86\
   ├── bin\              # Runtime DLLs (.dll)
   │   ├── x64\
   │   └── x86\
   └── redist\           # Redistributable files
       ├── x64\
       └── x86\
   ```

### Option 3: Minimal Development Setup

**Best for**: Development/testing with minimal files

You only need these files for basic compilation:

#### Required Files:

1. **Header Files** (minimum):
   ```
   include\mip\file\file_profile.h
   include\mip\file\file_engine.h
   include\mip\file\file_handler.h
   include\mip\protection\protection_descriptor.h
   include\mip\common_types.h
   include\mip\error.h
   ```

2. **Library Files** (for linking):
   ```
   lib\x64\mip.lib        # For x64 builds
   lib\x86\mip.lib        # For x86 builds (optional)
   ```

3. **Runtime DLLs** (for running):
   ```
   bin\x64\mip.dll        # For x64 applications
   ```

## Required File Structure

The MIP SDK should have this structure:

```
D:\Program Files\SDKs\MIP\
├── include\
│   └── mip\
│       ├── file\
│       │   ├── file_profile.h
│       │   ├── file_engine.h
│       │   ├── file_handler.h
│       │   └── ...
│       ├── protection\
│       │   ├── protection_descriptor.h
│       │   ├── protection_descriptor_builder.h
│       │   └── ...
│       ├── policy\
│       │   └── ...
│       ├── common_types.h
│       └── error.h
├── lib\
│   ├── x64\
│   │   └── mip.lib
│   └── x86\
│       └── mip.lib
├── bin\
│   ├── x64\
│   │   └── mip.dll
│   └── x86\
│       └── mip.dll
└── redist\
    ├── x64\
    │   └── mip.dll
    └── x86\
        └── mip.dll
```

## Setup Script Usage

### Create Directory Structure
```powershell
.\scripts\setup-mip-sdk.ps1 -CreateStructure
```

### Copy Files from Source
```powershell
.\scripts\setup-mip-sdk.ps1 -SdkSourcePath "C:\Path\To\Source" -TargetPath "D:\Program Files\SDKs\MIP"
```

### Verify Installation
```powershell
.\scripts\setup-mip-sdk.ps1 -VerifyInstallation
```

### Show Help
```powershell
.\scripts\setup-mip-sdk.ps1 -Help
```

## Where to Get MIP SDK Files

### Official Sources:

1. **Microsoft Download Center**:
   - Search for "Microsoft Information Protection SDK"
   - Download the Windows C++ SDK package

2. **NuGet Packages** (for .NET):
   - `Microsoft.InformationProtection.File`
   - `Microsoft.InformationProtection.Protection`
   - `Microsoft.InformationProtection.Policy`

3. **Visual Studio Package Manager**:
   ```powershell
   Install-Package Microsoft.InformationProtection.File
   ```

### Alternative Sources:

- If you have MIP SDK installed elsewhere, copy from:
  - `C:\Program Files\Microsoft Information Protection SDK\`
  - Or wherever the standard installer placed it

- If you have access to another development machine with MIP SDK installed

## Minimal Setup for Development

If you just want to compile the projects without full SDK functionality:

1. **Create the structure**:
   ```powershell
   .\scripts\setup-mip-sdk.ps1 -CreateStructure
   ```

2. **Create stub header files** (for compilation only):
   ```powershell
   # Create minimal headers that define the types we use
   # This allows compilation but runtime will need real DLLs
   ```

3. **Use simplified implementation**:
   - The current bridge implementation uses a simplified file format
   - This works for development/testing without full MIP SDK

## Verification

After setup, verify the installation:

```powershell
# Check directory structure
Get-ChildItem "D:\Program Files\SDKs\MIP" -Recurse

# Verify with script
.\scripts\setup-mip-sdk.ps1 -VerifyInstallation

# Test build
.\scripts\build.ps1 -Configuration Debug64 -Project Mip.NativeBridge
```

## Troubleshooting

### "MIP SDK not found"
- Run `.\scripts\setup-mip-sdk.ps1 -VerifyInstallation` to check what's missing
- Ensure files are in the correct directory structure
- Check environment variable: `$env:MIP_SDK_PATH`

### "Cannot find mip.lib"
- Verify `lib\x64\mip.lib` exists
- Check project file includes correct library path
- Ensure `MIP_SDK_PATH` environment variable is set

### "Cannot find mip.dll"
- Verify `bin\x64\mip.dll` exists
- Check DLL is copied to output directory during build
- Ensure DLL is in the same directory as your executable

### "Header files not found"
- Verify `include\mip\file\file_profile.h` exists
- Check project file includes correct include path
- Ensure header files are in the expected structure

## Next Steps

After MIP SDK is installed:

1. **Update configuration**:
   ```powershell
   .\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig
   ```

2. **Build projects**:
   ```powershell
   .\scripts\build.ps1 -Configuration Debug64 -Project Mip.NativeBridge
   .\scripts\build.ps1 -Configuration Debug64 -Project Mip.Cpp.Win32
   ```

3. **Test applications**:
   - Run the MIP C++ Win32 application
   - Test file protection/unprotection
   - Verify MIP SDK integration

## Notes

- **For C++ projects**: You need the full SDK with headers, libraries, and DLLs
- **For C# projects**: You can use NuGet packages instead
- **For development**: Minimal setup works for compilation, but runtime requires real DLLs
- **For production**: Full SDK installation is recommended

The current implementation uses a simplified file format that works without the full MIP SDK, but for production use, you'll want to integrate with the real MIP SDK APIs.

