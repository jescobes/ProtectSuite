# SDK Installation Guide

## Prerequisites Status
- ✅ .NET 8 SDK: Installed
- ✅ Visual Studio Build Tools: Installed at D:\BuildTools
- ⚠️ C:\BuildTools: Partially removed (some locked files remain)
- 📁 SDK Directories: Created at D:\Program Files\SDKs\

## Required SDKs

### 1. MSIPC (RMS) SDK
**Purpose**: For MSIPC.CSharp.WinForms and MSIPC.CppBridge.WinForms projects

**Download**:
- Visit: https://www.microsoft.com/en-us/download/details.aspx?id=53018
- Download: "Microsoft Rights Management Services SDK 2.1"
- File: RMSSDK_2.1.exe

**Installation**:
1. Run RMSSDK_2.1.exe as Administrator
2. Extract to: D:\Program Files\SDKs\MSIPC\
3. Expected structure:
   ```
   D:\Program Files\SDKs\MSIPC\
   ├── Redist\MSIPC\
   │   ├── x64\
   │   └── x86\
   ├── Include\
   └── Lib\
   ```

### 2. Microsoft Information Protection (MIP) SDK
**Purpose**: For Mip.CSharp.WinForms and Mip.CppBridge.WinForms projects

**Download**:
- Visit: https://www.microsoft.com/en-us/download/details.aspx?id=100429
- Download: "Microsoft Information Protection SDK"
- File: Microsoft.InformationProtection.SDK.x.x.x.exe

**Installation**:
1. Run the installer as Administrator
2. Install to: D:\Program Files\SDKs\MIP\
3. Expected structure:
   ```
   D:\Program Files\SDKs\MIP\
   ├── Redist\
   │   ├── x64\
   │   └── x86\
   ├── Include\
   └── Lib\
   ```

## Project Configuration

After SDK installation, update project references:

### MSIPC Projects
- Add MSIPC SDK references to .csproj files
- Point to D:\Program Files\SDKs\MSIPC\Lib\
- Include headers from D:\Program Files\SDKs\MSIPC\Include\

### MIP Projects
- Add MIP SDK NuGet packages for .NET projects
- For C++ bridge: reference D:\Program Files\SDKs\MIP\Lib\

## Next Steps
1. Install both SDKs following the guide above
2. Run `dotnet restore` to update project references
3. Build and test all four projects
4. Implement actual SDK calls in the stub methods

## Troubleshooting
- If C:\BuildTools removal issues persist, restart the system
- Ensure all SDK installers are run as Administrator
- Verify SDK paths in project files after installation

