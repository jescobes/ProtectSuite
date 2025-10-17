# ProtectSuite

A comprehensive suite of applications demonstrating Microsoft Information Protection (MIP) and Microsoft Information Protection and Control (MSIPC) SDKs for file protection, unprotection, and information retrieval.

## Overview

ProtectSuite provides four different implementations of file protection using Microsoft's information protection technologies:

1. **MSIPC C# WinForms** - Uses MSIPC SDK C# with managed API wrapper
2. **MSIPC C++ Win32** - Uses MSIPC SDK C++ with native Win32 dialog interface  
3. **MIP C# WinForms** - Uses MIP SDK C# (planned)
4. **MIP C++ Bridge WinForms** - Uses MIP SDK C++ via C++/CLI bridge (planned)

## Features

### MSIPC Applications (Completed)

#### MSIPC C# WinForms Application
- **Target Framework**: .NET Framework 4.7.2
- **UI**: Windows Forms with modern interface
- **Authentication**: 
  - OAuth2 flow with `suppressUI=true` for Purview backend
  - AD RMS server configuration via `config/adrms.json`
- **Protection Modes**:
  - **Template Mode**: Select from available RMS templates
  - **Custom Mode**: Define custom user rights with individual permissions
- **Operations**: Protect, Unprotect, Get Protection Information
- **Backend Support**: Purview (Azure) and AD RMS

#### MSIPC C++ Win32 Application  
- **Target**: Native Win32 C++ application
- **UI**: Native Windows dialog interface
- **Authentication**: MSIPC C API integration
- **Protection Modes**:
  - **Template Mode**: Real-time template fetching from MSIPC
  - **Custom Mode**: Custom user rights configuration
- **Operations**: Real MSIPC file encryption/decryption using `IpcfEncryptFile`/`IpcfDecryptFile`
- **Build Configurations**: Debug, Release, Debug64, Release64

### MIP Applications (Planned)

#### MIP C# WinForms Application
- **Target Framework**: .NET 8
- **UI**: Windows Forms
- **Authentication**: OAuth2 with MSAL
- **Operations**: Protect, Unprotect, Get Protection Information

#### MIP C++ Bridge WinForms Application
- **Target Framework**: .NET 8
- **UI**: Windows Forms
- **Bridge**: C++/CLI wrapper for MIP C++ SDK
- **Operations**: Protect, Unprotect, Get Protection Information

## Project Structure

```
ProtectSuite/
├── Msipc.CSharp.WinForms/          # MSIPC C# WinForms app
│   ├── MainForm.cs                  # Main UI and logic
│   ├── TemplatePickerDialog.cs     # Template selection dialog
│   ├── UserRightsEditorDialog.cs   # Custom rights configuration
│   ├── SafeNativeMethods.cs        # MSIPC managed API wrapper
│   └── IpcManagedAPI.cs            # MSIPC managed API implementation
├── Msipc.Cpp.Win32/                # MSIPC C++ Win32 app
│   ├── main.cpp                    # Main application logic
│   ├── resource.rc                 # Dialog resources
│   └── resource.h                  # Resource definitions
├── Mip.CSharp.WinForms/            # MIP C# app (planned)
├── Mip.CppBridge.WinForms/         # MIP C++ bridge app (planned)
├── config/
│   └── adrms.json                  # AD RMS server configuration
└── .vscode/                        # VS Code configuration
    ├── launch.json                 # Debug configurations
    └── tasks.json                  # Build tasks
```

## Prerequisites

### Required Software
- **Visual Studio Build Tools 2022** (installed to `D:\BuildTools`)
- **Windows 10 SDK** (installed with Build Tools)
- **.NET Framework 4.7.2** (for MSIPC C# app)
- **.NET 8 SDK** (for MIP apps)

### Required SDKs
- **MSIPC SDK** (installed to `D:\Program Files\SDKs\MSIPC`)
  - Headers: `D:\Program Files\SDKs\MSIPC\inc\`
  - Libraries: `D:\Program Files\SDKs\MSIPC\lib\`
  - Runtime: `D:\Program Files\SDKs\MSIPC\bin\`

### Configuration Files

#### AD RMS Configuration (`config/adrms.json`)
```json
{
  "servers": [
    {
      "name": "AD RMS Server 1",
      "intranetUrl": "https://rms-server.company.com",
      "extranetUrl": "https://rms-server.company.com"
    }
  ]
}
```

## Building and Running

### Quick Start (Recommended)

1. **Setup environment** (auto-detects SDK paths):
   ```powershell
   .\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig
   ```

2. **Build all projects**:
   ```powershell
   .\scripts\build.ps1 -Configuration Debug64
   ```

### Manual Build Commands

#### MSIPC C++ Win32 Application

**Using build script (recommended):**
```powershell
.\scripts\build.ps1 -Configuration Debug64 -Project Msipc.Cpp.Win32
.\scripts\build.ps1 -Configuration Release64 -Project Msipc.Cpp.Win32
```

**Using MSBuild directly:**
```bash
# Debug64 configuration
msbuild "Msipc.Cpp.Win32\Msipc.Cpp.Win32.vcxproj" /p:Configuration=Debug64 /p:Platform=x64

# Release64 configuration  
msbuild "Msipc.Cpp.Win32\Msipc.Cpp.Win32.vcxproj" /p:Configuration=Release64 /p:Platform=x64
```

**Run:**
```bash
# Debug64
Msipc.Cpp.Win32\bin\Debug64\x64\Msipc.Cpp.Win32.exe

# Release64
Msipc.Cpp.Win32\bin\Release64\x64\Msipc.Cpp.Win32.exe
```

#### MSIPC C# WinForms Application

**Using build script (recommended):**
```powershell
.\scripts\build.ps1 -Configuration Debug64 -Project Msipc.CSharp.WinForms
.\scripts\build.ps1 -Configuration Release64 -Project Msipc.CSharp.WinForms
```

**Using MSBuild directly:**
```bash
# Debug configuration
msbuild "Msipc.CSharp.WinForms\Msipc.CSharp.WinForms.csproj" /p:Configuration=Debug

# Debug64 configuration
msbuild "Msipc.CSharp.WinForms\Msipc.CSharp.WinForms.csproj" /p:Configuration=Debug64
```

**Run:**
```bash
# Debug
Msipc.CSharp.WinForms\bin\Debug\Msipc.CSharp.WinForms.exe

# Debug64
Msipc.CSharp.WinForms\bin\Debug64\Msipc.CSharp.WinForms.exe
```

### VS Code Integration

The project includes VS Code configuration for debugging and building:

- **Debug Configurations**: Available for both C# and C++ applications
- **Build Tasks**: Pre-configured for all build configurations
- **Launch Configurations**: Ready-to-use debug setups

## Usage

### MSIPC Applications

1. **Select File**: Click "Browse" to select a file for protection
2. **Choose Backend**: Select between Purview (Azure) or AD RMS
3. **Select Protection Mode**:
   - **Template Mode**: Click "Select Template" to choose from available RMS templates
   - **Custom Mode**: Click "Edit User Rights" to configure custom permissions
4. **Protect**: Click "Protect" to encrypt the file
5. **Unprotect**: Click "Unprotect" to decrypt a protected file
6. **Get Info**: Click "Get Info" to retrieve protection information

### Template Selection

The template picker displays:
- Template name and description
- Template ID
- Issuer information
- Real-time template fetching from MSIPC

### Custom Rights Configuration

Configure individual user permissions:
- **User Identity**: Email address or "ANYONE" for all users
- **Rights**: VIEW, EDIT, PRINT, EXPORT, EXTRACT, COMMENT, FORWARD
- **Multiple Users**: Add multiple users with different permission sets

## Technical Details

### MSIPC Integration

#### C++ Application
- Uses MSIPC C API directly
- Real file encryption via `IpcfEncryptFile`/`IpcfDecryptFile`
- Template fetching via `IpcGetTemplateList`
- Custom rights via `IpcCreateLicenseFromScratch` + `IpcSetLicenseProperty`

#### C# Application  
- Uses managed MSIPC API wrapper
- License-based protection approach
- OAuth2 integration for Purview
- AD RMS configuration management

### Build Configurations

All projects support multiple build configurations:
- **Debug**: Standard debug build
- **Release**: Optimized release build  
- **Debug64**: 64-bit debug build
- **Release64**: 64-bit release build

### Authentication

#### Purview (Azure)
- OAuth2 flow with MSAL
- `suppressUI=true` for automated token acquisition
- Application registration required

#### AD RMS
- Server configuration via JSON file
- No auto-discovery (explicit server URLs)
- Windows authentication

## Migration to New Machine

The project is designed to be easily portable with a comprehensive multi-layered configuration system.

### 🎯 Portable Configuration System

The project uses multiple mechanisms for portable path resolution:

#### 1. **Environment Variables** (Primary)
```powershell
$env:MSIPC_SDK_PATH = "C:\YourPath\To\MSIPC"
$env:MIP_SDK_PATH = "C:\YourPath\To\MIP"
$env:MSBUILD_PATH = "C:\YourPath\To\MSBuild.exe"
$env:WINDOWS_SDK_PATH = "C:\YourPath\To\WindowsKits"
```

#### 2. **Configuration File** (Secondary)
`config\sdk-paths.json`:
```json
{
  "msipc": {
    "sdkPath": "C:\\YourPath\\To\\MSIPC",
    "includePath": "C:\\YourPath\\To\\MSIPC\\inc",
    "libPath": "C:\\YourPath\\To\\MSIPC\\lib"
  },
  "mip": {
    "sdkPath": "C:\\YourPath\\To\\MIP"
  },
  "buildTools": {
    "msbuildPath": "C:\\YourPath\\To\\MSBuild.exe"
  }
}
```

#### 3. **Directory.Build.props** (MSBuild)
Central MSBuild configuration that:
- Resolves SDK paths from environment variables
- Provides fallback to hardcoded paths
- Validates SDK existence before building
- Sets derived paths automatically

#### 4. **Auto-Detection Scripts**
- `scripts\setup-environment.ps1` - Automatically detects SDK installations
- `scripts\build.ps1` - Portable build script with path resolution

### 🚀 Quick Migration Process

#### **For New Machine Setup:**
```powershell
# 1. Copy project files
# 2. Install required SDKs
# 3. Auto-detect and configure
.\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig

# 4. Build project
.\scripts\build.ps1 -Configuration Debug64
```

#### **For Manual Configuration:**
```powershell
# Set environment variables
$env:MSIPC_SDK_PATH = "C:\YourPath\To\MSIPC"

# Or update config file
# Edit config\sdk-paths.json

# Build
.\scripts\build.ps1 -Configuration Debug64
```

### 🔍 SDK Detection Logic

The system automatically searches for SDKs in:

#### **MSIPC SDK Detection:**
1. Environment variable `MSIPC_SDK_PATH`
2. Common paths: `C:\Program Files\SDKs\MSIPC`, `D:\Program Files\SDKs\MSIPC`
3. Registry: `HKLM:\SOFTWARE\Microsoft\MSIPC`
4. Fallback to hardcoded path

#### **MIP SDK Detection:**
1. Environment variable `MIP_SDK_PATH`
2. Common paths: `C:\Program Files\SDKs\MIP`, `D:\Program Files\SDKs\MIP`
3. Fallback to hardcoded path

#### **Build Tools Detection:**
1. Environment variable `MSBUILD_PATH`
2. `vswhere.exe` if available
3. Common paths: VS 2019/2022 Build Tools
4. Fallback to hardcoded path

### ✅ Benefits

1. **Zero Configuration Migration** - Copy project files to new machine, run setup script, build immediately
2. **Multiple Configuration Methods** - Environment variables (for CI/CD), configuration files (for team sharing), auto-detection (for convenience)
3. **Fallback Support** - Graceful degradation if auto-detection fails, hardcoded paths as last resort, clear error messages for missing SDKs
4. **IDE Integration** - VS Code tasks use portable build script, debug configurations work across machines, no hardcoded paths in IDE configs
5. **Team Collaboration** - Share `config\sdk-paths.json` for team consistency, individual developers can override with environment variables, clear documentation for setup process

### 📋 Migration Checklist

When migrating to a new machine:

- [ ] Copy entire project directory
- [ ] Install Visual Studio Build Tools 2022
- [ ] Install Windows 10 SDK
- [ ] Install MSIPC SDK
- [ ] Install MIP SDK (optional)
- [ ] Run `.\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig`
- [ ] Verify paths with `.\scripts\build.ps1 -ShowSdkPaths`
- [ ] Test build with `.\scripts\build.ps1 -Configuration Debug64`
- [ ] Update team configuration if needed

### 🔧 Migration Troubleshooting

#### **Common Issues:**

1. **"MSIPC SDK not found"**
   - Run auto-detection script
   - Check SDK installation
   - Set environment variable manually

2. **"MSBuild not found"**
   - Install Visual Studio Build Tools
   - Set `MSBUILD_PATH` environment variable
   - Update configuration file

3. **"Project won't build"**
   - Verify all SDKs are installed
   - Check environment variables
   - Run setup script with verbose output

#### **Debug Commands:**

```powershell
# Show detected paths
.\scripts\build.ps1 -ShowSdkPaths

# Verbose auto-detection
.\scripts\setup-environment.ps1 -AutoDetect -Verbose

# Verbose build
.\scripts\build.ps1 -Configuration Debug64 -Verbose
```

### 📁 Portable File Structure

```
ProtectSuite/
├── config/
│   ├── sdk-paths.json          # SDK path configuration
│   └── adrms.json              # AD RMS server configuration
├── scripts/
│   ├── setup-environment.ps1   # Auto-detection and setup
│   └── build.ps1               # Portable build script
├── Directory.Build.props       # Central MSBuild configuration
├── .vscode/
│   ├── tasks.json              # Updated VS Code tasks
│   └── launch.json             # Debug configurations
└── MIGRATION_GUIDE.md          # Detailed migration instructions
```

**Result**: The ProtectSuite project is now **100% portable** and can be easily migrated between machines with different SDK installation paths. The multi-layered configuration system ensures maximum flexibility while maintaining ease of use.

## Troubleshooting

### Common Issues

1. **MSIPC SDK Not Found**
   - Run `.\scripts\setup-environment.ps1 -AutoDetect` to detect SDK paths
   - Set `$env:MSIPC_SDK_PATH` environment variable
   - Update `config\sdk-paths.json` with correct paths

2. **Build Tools Issues**
   - Ensure Visual Studio Build Tools 2022 is installed
   - Verify Windows 10 SDK is available
   - Check .NET Framework 4.7.2 targeting pack
   - Set `$env:MSBUILD_PATH` if MSBuild is in non-standard location

3. **Authentication Failures**
   - Verify Purview application registration
   - Check AD RMS server configuration
   - Ensure network connectivity

4. **Path Resolution Issues**
   - Use `.\scripts\build.ps1 -ShowSdkPaths` to verify detected paths
   - Check environment variables are set correctly
   - Verify `config\sdk-paths.json` has correct paths

### Debug Information

Both applications provide comprehensive logging:
- Operation status messages
- Error details and troubleshooting hints
- Authentication flow information
- File operation results

### Migration Support

For migration issues:
- Check [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) for detailed instructions
- Use `.\scripts\setup-environment.ps1 -AutoDetect -Verbose` for detailed detection
- Verify all required SDKs are installed and accessible

## Contributing

This project demonstrates Microsoft Information Protection SDKs and serves as a reference implementation. Contributions are welcome for:

- Additional protection scenarios
- Enhanced UI/UX features
- Additional authentication methods
- Performance optimizations
- Documentation improvements

## License

This project is provided as a demonstration of Microsoft Information Protection SDKs. Please refer to Microsoft's licensing terms for the underlying SDKs and technologies used.

## Support

For issues related to:
- **MSIPC SDK**: Microsoft Information Protection and Control documentation
- **MIP SDK**: Microsoft Information Protection documentation  
- **This Project**: Create an issue in the project repository