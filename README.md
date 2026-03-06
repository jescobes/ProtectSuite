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
  - Automatic certificate detection (GIC/CLC) for AD RMS
  - Identity retrieval using existing certificates without authentication prompts
- **Protection Modes**:
  - **Template Mode**: Select from available RMS templates
  - **Custom Mode**: Define custom user rights with individual permissions
- **Operations**: 
  - **Protect**: Encrypt files using MSIPC API (`IpcfEncryptFile`)
  - **Unprotect**: Decrypt protected files (`IpcfDecryptFile`)
  - **Get Info**: Retrieve protection information from licenses
  - **View Content**: Open protected files in Notepad++ (requires VIEW rights)
- **File Protection Detection**: Automatic detection using MSIPC API (`IpcfIsFileEncrypted`)
- **Backend Support**: Purview (Azure) and AD RMS
- **Identity Management**:
  - Automatic MSIPC user identity retrieval from existing certificates
  - Offline mode support (uses cached GIC/CLC certificates)
  - Online mode fallback when certificates need refresh
  - "From scratch" license method when templates are unavailable

#### MSIPC C++ Win32 Application  
- **Target**: Native Win32 C++ application
- **UI**: Native Windows dialog interface
- **Authentication**: MSIPC C API integration
- **Protection Modes**:
  - **Template Mode**: Real-time template fetching from MSIPC
  - **Custom Mode**: Custom user rights configuration
- **Operations**: Real MSIPC file encryption/decryption using `IpcfEncryptFile`/`IpcfDecryptFile`
- **Build Configurations**: Debug, Release (AnyCPU - auto-detects architecture)

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

### Dependencies and NuGet

**Important:** This project does **NOT** use NuGet during the build process. All dependencies are referenced directly from DLL files.

#### External Libraries (Manual Updates Required)

The following libraries are referenced directly and should be updated manually when needed:

1. **Microsoft.Identity.Client** (v4.61.3)
   - **Purpose**: OAuth2 authentication for Purview/Azure AD
   - **Location**: `Msipc.CSharp.WinForms\bin\Debug\net472\Microsoft.Identity.Client.dll`
   - **NuGet Package**: `Microsoft.Identity.Client`
   - **Update Frequency**: Check periodically for security updates
   - **Update Method**: Download from NuGet, extract DLL, copy to project's bin folder

2. **Newtonsoft.Json** (v13.0.3)
   - **Purpose**: JSON configuration parsing
   - **Location**: `Msipc.CSharp.WinForms\bin\Debug\net472\Newtonsoft.Json.dll`
   - **NuGet Package**: `Newtonsoft.Json`
   - **Update Frequency**: Check periodically for security updates
   - **Update Method**: Download from NuGet, extract DLL, copy to project's bin folder

3. **xUnit** (v2.4.2) - Test Framework
   - **Purpose**: Unit testing framework
   - **Location**: `packages\xunit.*\`
   - **NuGet Packages**: `xunit`, `xunit.abstractions`, `xunit.assert`, `xunit.extensibility.core`, `xunit.extensibility.execution`
   - **Update Frequency**: As needed for new testing features
   - **Update Method**: Use `nuget.exe restore` on `Tests.Msipc.CSharp.WinForms\packages.config`, then copy DLLs

#### Why NuGet is Disabled

- **Build Reliability**: Avoids MSBuild 17 RuntimeIdentifier validation issues with traditional .NET Framework projects
- **Portability**: No dependency on NuGet restore during build
- **Control**: Explicit dependency management
- **Compatibility**: Works with older build tools and environments

#### Updating Dependencies

To update a dependency:

1. **Download the NuGet package**:
   ```powershell
   nuget.exe install <PackageName> -Version <Version> -OutputDirectory packages
   ```

2. **Copy the DLL** to the appropriate location:
   - For application projects: Copy to `bin\Debug\net472\` or `bin\Release\net472\`
   - For test projects: Copy to `packages\<PackageName>.<Version>\lib\<TargetFramework>\`

3. **Update the project file** if the version changed:
   - Update `HintPath` in `.csproj` file
   - Update version in `packages.config` (for test projects)

4. **Test the build** to ensure compatibility

### Configuration Files

#### AD RMS Configuration (`config/adrms.json`)
```json
{
  "serverUrl": "https://protection.sealpath.com/",
  "extranetUrl": "",
  "intranetUrl": "",
  "licensingUrl": "",
  "licensingOnlyClusters": true
}
```

**Configuration Options:**
- `serverUrl`: Base URL for certification (used if extranetUrl/intranetUrl not specified)
- `extranetUrl`: Explicit extranet URL (optional)
- `intranetUrl`: Explicit intranet URL (optional)
- `licensingUrl`: Specific URL for licensing-only clusters (optional)
- `licensingOnlyClusters`: Boolean indicating if server is licensing-only (default: true)

**URL Formats:**
- **Base URL**: `https://protection.sealpath.com/` - SDK performs auto-discovery
- **Full Path**: `https://protection.sealpath.com/_wmcs/certificationexternal` - Explicit endpoint

**URL Validation:**
- DNS resolution check
- HTTP GET to `URL + /certification.asmx` (for certification URLs)
- HTTP GET to `URL + /licensing.asmx` (for licensing URLs)

## Building and Running

### Quick Start (Recommended)

1. **Setup environment** (auto-detects SDK paths):
   ```powershell
   .\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig
   ```

2. **Build all projects**:
   ```powershell
   # Build Debug configuration (AnyCPU - auto-detects platform)
   .\scripts\build.ps1 -Configuration Debug
   
   # Build Release configuration (AnyCPU - auto-detects platform)
   .\scripts\build.ps1 -Configuration Release
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

**Note on native DLL loading with AnyCPU:**
- The C# application uses MSIPC SDK which includes native C++ DLLs (`msipc.dll`)
- **Debug/Release** configurations use `AnyCPU` (auto-detects platform at runtime)
- **How native DLL loading works:**
  - When compiled as `AnyCPU`, the .NET process architecture is determined at runtime:
    - On 64-bit Windows: Process runs as 64-bit
    - On 32-bit Windows: Process runs as 32-bit
  - The `IpcInitialize()` method:
    1. Reads MSIPC client installation path from Windows Registry: `SOFTWARE\Microsoft\MSIPC\CurrentVersion\InstallLocation`
    2. The registry typically points to the 64-bit client: `C:\Program Files\Cliente 2.1 de Active Directory Rights Management Services\`
    3. Automatically detects the process architecture at runtime using `IntPtr.Size`
    4. Adjusts the path based on process architecture:
       - **64-bit process**: Uses the path from registry (Program Files)
       - **32-bit process**: Redirects to Program Files (x86) version: `C:\Program Files (x86)\Cliente 2.1 de Active Directory Rights Management Services\`
    5. Also checks for architecture-specific subdirectories (`x64` or `x86`) if they exist
    6. Uses `SetDllDirectory()` to configure the correct DLL search path before loading
    7. **Note**: MSIPC client has separate 32-bit and 64-bit installations in different Program Files directories
  - **Result:** One `AnyCPU` build automatically works on both 32-bit and 64-bit Windows systems

**Using build script (recommended):**
```powershell
# Debug (AnyCPU - auto-detects platform)
.\scripts\build.ps1 -Configuration Debug -Project Msipc.CSharp.WinForms

# Release (AnyCPU - auto-detects platform)
.\scripts\build.ps1 -Configuration Release -Project Msipc.CSharp.WinForms
```

**Using MSBuild directly:**
```bash
# Debug configuration (AnyCPU)
msbuild "Msipc.CSharp.WinForms\Msipc.CSharp.WinForms.csproj" /p:Configuration=Debug

# Release configuration (AnyCPU)
msbuild "Msipc.CSharp.WinForms\Msipc.CSharp.WinForms.csproj" /p:Configuration=Release
```

**Run:**
```bash
# Debug (AnyCPU)
Msipc.CSharp.WinForms\bin\Debug\Msipc.CSharp.WinForms.exe

# Release (AnyCPU)
Msipc.CSharp.WinForms\bin\Release\Msipc.CSharp.WinForms.exe
```

### VS Code Integration

The project includes VS Code configuration for debugging and building:

- **Debug Configurations**: Available for both C# and C++ applications
- **Build Tasks**: Pre-configured for all build configurations
- **Launch Configurations**: Ready-to-use debug setups

## Usage

### MSIPC Applications

1. **Select File**: Click "Browse" to select a file for protection
   - The application automatically detects if the file is protected
   - UI buttons are enabled/disabled based on file protection status
2. **Choose Backend**: Select between Purview (Azure) or AD RMS
   - **For AD RMS**: MSIPC user identity is retrieved automatically if certificates exist
     - **How it works**: The application uses a multi-step process to retrieve user identity without authentication:
       1. **Certificate Detection**: Checks for existing GIC (Group Identity Certificate) and CLC (Client Licensor Certificate) certificates using `IpcGetTemplateList` in offline mode
       2. **Template Method**: If templates are available, creates a temporary license from the first template using `IpcCreateLicenseFromTemplateId`, then serializes it with `IpcSerializeLicense` in offline mode to extract the user identity from the resulting key handle
       3. **From Scratch Method**: If no templates are available, uses `IpcGetTemplateIssuerList` to find an issuer that allows "from scratch" licenses, then creates a temporary license using `IpcCreateLicenseFromScratch` and extracts identity similarly
       4. **Online Fallback**: If offline mode fails (e.g., certificates don't match server URL or need refresh), automatically retries in online mode
       5. **Identity Extraction**: Uses `IpcGetKeyUserDisplayName` on the key handle to get the user's email address, and `IpcGetSerializedLicenseConnectionInfo` to get server information
     - **Certificate Location**: Certificates are stored by MSIPC SDK in `%LOCALAPPDATA%\Microsoft\MSIPC\` (typically `C:\Users\<User>\AppData\Local\Microsoft\MSIPC\`)
     - **No Authentication Required**: If certificates exist and match the configured server URL, identity is retrieved without any user prompts
   - **For Purview**: Identity is retrieved during first protect/unprotect operation (requires OAuth2 authentication)
3. **Select Protection Mode**:
   - **Template Mode**: Click "Select Template" to choose from available RMS templates
   - **Custom Mode**: Click "Edit User Rights" to configure custom permissions
4. **Protect**: Click "Protect" to encrypt the file using MSIPC API
   - Disabled for already-protected files
5. **Unprotect**: Click "Unprotect" to decrypt a protected file
   - Only enabled for protected files
6. **Get Info**: Click "Get Info" to retrieve protection information
   - Shows MSIPC user identity, owner, content ID, and app-specific data
7. **View Content**: Click "View Content" to open file in Notepad++
   - Automatically decrypts protected files if user has VIEW rights
   - Opens unprotected files directly

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
- Real file encryption via `IpcfEncryptFile`/`IpcfDecryptFile`
- File protection detection via `IpcfIsFileEncrypted`
- License-based protection approach
- OAuth2 integration for Purview
- AD RMS configuration management
- Certificate-based identity retrieval (GIC/CLC)
- Offline/online mode support for identity retrieval
- "From scratch" license creation when templates unavailable
- **Native DLL Loading:**
  - Uses P/Invoke (`DllImport`) to call MSIPC native C++ functions
  - `IpcInitialize()` reads MSIPC client path from Windows Registry (`SOFTWARE\Microsoft\MSIPC\CurrentVersion\InstallLocation`)
  - The registry typically points to the 64-bit client installation in `Program Files`
  - Detects process architecture at runtime (`IntPtr.Size`)
  - For 32-bit processes, redirects to `Program Files (x86)` version automatically
  - Example paths:
    - 64-bit process: `C:\Program Files\Cliente 2.1 de Active Directory Rights Management Services\msipc.dll`
    - 32-bit process: `C:\Program Files (x86)\Cliente 2.1 de Active Directory Rights Management Services\msipc.dll`
  - Also checks for architecture subdirectories (`x64`/`x86`) if they exist
  - Uses `SetDllDirectory()` to configure DLL search path before loading
  - Works seamlessly with `AnyCPU` builds - architecture detection and path redirection are automatic

### Build Configurations

**MSIPC C++ Win32 Application:**
- **Debug64**: 64-bit debug build
- **Release64**: 64-bit release build

**MSIPC C# WinForms Application:**
- **Debug**: AnyCPU debug build (auto-detects platform at runtime)
- **Release**: AnyCPU optimized release build (auto-detects platform at runtime)
- Architecture-specific DLLs are loaded automatically based on runtime process architecture

### Authentication

#### Purview (Azure)
- OAuth2 flow with MSAL
- `suppressUI=true` for automated token acquisition
- Application registration required

#### AD RMS
- Server configuration via JSON file (`config/adrms.json`)
- Supports base URLs with auto-discovery of standard endpoints
- Supports explicit extranet/intranet URLs
- Supports licensing-only clusters configuration
- URL validation (DNS resolution and HTTP endpoint checks)
- Windows authentication
- Certificate-based identity (GIC/CLC)
  - Automatic detection of existing certificates
  - Offline mode support using cached certificates
  - Online mode fallback when certificates need refresh
  - Identity retrieval without authentication if certificates exist

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
   - Check if GIC/CLC certificates exist for AD RMS server
   - Verify server URL matches certificate server URL
   - Try online mode if offline mode fails with `0x8004020D`

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
- MSIPC user identity information
- Certificate detection status (GIC/CLC)
- Connection info and server URLs
- Template availability and issuer information

**Common Error Codes:**
- `0x8004020D` (IPCERROR_NEEDS_ONLINE): Operation requires network access but offline mode was requested
  - **Cause**: No certificates cached for this server, or certificates don't match server URL
  - **Solution**: Try online mode or perform protect/unprotect operation to cache certificates
- `0x80070005` (E_ACCESSDENIED): Authentication required but `suppressUI=true` prevents prompts
  - **Solution**: Set `suppressUI=false` or perform protect/unprotect operation to authenticate
- `0x80070002` (E_FILE_NOT_FOUND): Certificates not found for this server
  - **Solution**: Perform protect/unprotect operation to download and cache certificates

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