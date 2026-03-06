# MIP SDK Setup Script
# This script helps set up the MIP SDK either via standard installation or manual file copying

param(
    [string]$SdkSourcePath = "",
    [string]$TargetPath = "D:\Program Files\SDKs\MIP",
    [switch]$CreateStructure = $false,
    [switch]$VerifyInstallation = $false,
    [switch]$Help = $false
)

function Show-Help {
    Write-Host "MIP SDK Setup Script" -ForegroundColor Green
    Write-Host "====================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\scripts\setup-mip-sdk.ps1 -CreateStructure" -ForegroundColor Cyan
    Write-Host "    Creates the directory structure for MIP SDK"
    Write-Host ""
    Write-Host "  .\scripts\setup-mip-sdk.ps1 -SdkSourcePath 'C:\Path\To\MIP\SDK' -TargetPath 'D:\Program Files\SDKs\MIP'" -ForegroundColor Cyan
    Write-Host "    Copies MIP SDK files from source to target location"
    Write-Host ""
    Write-Host "  .\scripts\setup-mip-sdk.ps1 -VerifyInstallation" -ForegroundColor Cyan
    Write-Host "    Verifies that MIP SDK is properly installed"
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -SdkSourcePath    Path to source MIP SDK files (if copying manually)"
    Write-Host "  -TargetPath       Target installation path (default: D:\Program Files\SDKs\MIP)"
    Write-Host "  -CreateStructure  Create directory structure only"
    Write-Host "  -VerifyInstallation  Verify SDK installation"
    Write-Host "  -Help             Show this help message"
}

function Create-DirectoryStructure {
    param([string]$BasePath)
    
    Write-Host "Creating MIP SDK directory structure..." -ForegroundColor Yellow
    
    $directories = @(
        "$BasePath\include",
        "$BasePath\include\mip",
        "$BasePath\include\mip\file",
        "$BasePath\include\mip\protection",
        "$BasePath\include\mip\policy",
        "$BasePath\lib",
        "$BasePath\lib\x64",
        "$BasePath\lib\x86",
        "$BasePath\bin",
        "$BasePath\bin\x64",
        "$BasePath\bin\x86",
        "$BasePath\redist",
        "$BasePath\redist\x64",
        "$BasePath\redist\x86"
    )
    
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "  Created: $dir" -ForegroundColor Green
        } else {
            Write-Host "  Exists: $dir" -ForegroundColor Gray
        }
    }
    
    Write-Host "Directory structure created successfully!" -ForegroundColor Green
}

function Copy-SdkFiles {
    param(
        [string]$SourcePath,
        [string]$TargetPath
    )
    
    if (-not (Test-Path $SourcePath)) {
        Write-Host "Error: Source path does not exist: $SourcePath" -ForegroundColor Red
        return $false
    }
    
    Write-Host "Copying MIP SDK files from $SourcePath to $TargetPath..." -ForegroundColor Yellow
    
    # Create target structure first
    Create-DirectoryStructure -BasePath $TargetPath
    
    # Copy include files
    if (Test-Path "$SourcePath\include") {
        Write-Host "Copying header files..." -ForegroundColor Yellow
        Copy-Item -Path "$SourcePath\include\*" -Destination "$TargetPath\include\" -Recurse -Force
        Write-Host "  Headers copied" -ForegroundColor Green
    } elseif (Test-Path "$SourcePath\inc") {
        Write-Host "Copying header files (from 'inc' directory)..." -ForegroundColor Yellow
        Copy-Item -Path "$SourcePath\inc\*" -Destination "$TargetPath\include\" -Recurse -Force
        Write-Host "  Headers copied" -ForegroundColor Green
    } else {
        Write-Host "Warning: No include directory found in source" -ForegroundColor Yellow
    }
    
    # Copy library files
    if (Test-Path "$SourcePath\lib") {
        Write-Host "Copying library files..." -ForegroundColor Yellow
        Copy-Item -Path "$SourcePath\lib\*" -Destination "$TargetPath\lib\" -Recurse -Force
        Write-Host "  Libraries copied" -ForegroundColor Green
    } else {
        Write-Host "Warning: No lib directory found in source" -ForegroundColor Yellow
    }
    
    # Copy binary/DLL files
    if (Test-Path "$SourcePath\bin") {
        Write-Host "Copying binary files..." -ForegroundColor Yellow
        Copy-Item -Path "$SourcePath\bin\*" -Destination "$TargetPath\bin\" -Recurse -Force
        Write-Host "  Binaries copied" -ForegroundColor Green
    } elseif (Test-Path "$SourcePath\redist") {
        Write-Host "Copying redistributable files..." -ForegroundColor Yellow
        Copy-Item -Path "$SourcePath\redist\*" -Destination "$TargetPath\redist\" -Recurse -Force
        Write-Host "  Redistributables copied" -ForegroundColor Green
    } else {
        Write-Host "Warning: No bin or redist directory found in source" -ForegroundColor Yellow
    }
    
    Write-Host "File copy completed!" -ForegroundColor Green
    return $true
}

function Verify-Installation {
    param([string]$SdkPath)
    
    Write-Host "Verifying MIP SDK installation at: $SdkPath" -ForegroundColor Yellow
    Write-Host ""
    
    $checks = @{
        "Include directory" = "$SdkPath\include"
        "Library directory" = "$SdkPath\lib"
        "Binary directory" = "$SdkPath\bin"
    }
    
    $allGood = $true
    
    foreach ($check in $checks.GetEnumerator()) {
        if (Test-Path $check.Value) {
            Write-Host "  [OK] $($check.Key): $($check.Value)" -ForegroundColor Green
        } else {
            Write-Host "  [MISSING] $($check.Key): $($check.Value)" -ForegroundColor Red
            $allGood = $false
        }
    }
    
    # Check for key header files
    Write-Host ""
    Write-Host "Checking for key header files..." -ForegroundColor Yellow
    $headers = @(
        "$SdkPath\include\mip\file\file_profile.h",
        "$SdkPath\include\mip\file\file_engine.h",
        "$SdkPath\include\mip\file\file_handler.h"
    )
    
    foreach ($header in $headers) {
        if (Test-Path $header) {
            Write-Host "  [OK] $(Split-Path $header -Leaf)" -ForegroundColor Green
        } else {
            Write-Host "  [MISSING] $(Split-Path $header -Leaf)" -ForegroundColor Yellow
        }
    }
    
    # Check for library files
    Write-Host ""
    Write-Host "Checking for library files..." -ForegroundColor Yellow
    $libs = @(
        "$SdkPath\lib\x64\mip.lib",
        "$SdkPath\lib\mip.lib"
    )
    
    $libFound = $false
    foreach ($lib in $libs) {
        if (Test-Path $lib) {
            Write-Host "  [OK] $(Split-Path $lib -Leaf)" -ForegroundColor Green
            $libFound = $true
        }
    }
    
    if (-not $libFound) {
        Write-Host "  [MISSING] mip.lib" -ForegroundColor Yellow
    }
    
    # Check for DLL files
    Write-Host ""
    Write-Host "Checking for runtime DLLs..." -ForegroundColor Yellow
    $dlls = @(
        "$SdkPath\bin\x64\mip.dll",
        "$SdkPath\bin\mip.dll",
        "$SdkPath\redist\x64\mip.dll"
    )
    
    $dllFound = $false
    foreach ($dll in $dlls) {
        if (Test-Path $dll) {
            Write-Host "  [OK] $(Split-Path $dll -Leaf)" -ForegroundColor Green
            $dllFound = $true
        }
    }
    
    if (-not $dllFound) {
        Write-Host "  [MISSING] mip.dll" -ForegroundColor Yellow
    }
    
    Write-Host ""
    if ($allGood) {
        Write-Host "MIP SDK installation verified successfully!" -ForegroundColor Green
    } else {
        Write-Host "MIP SDK installation incomplete. Some components are missing." -ForegroundColor Yellow
    }
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

if ($CreateStructure) {
    Create-DirectoryStructure -BasePath $TargetPath
    exit 0
}

if ($VerifyInstallation) {
    Verify-Installation -SdkPath $TargetPath
    exit 0
}

if ($SdkSourcePath) {
    Copy-SdkFiles -SourcePath $SdkSourcePath -TargetPath $TargetPath
    Verify-Installation -SdkPath $TargetPath
} else {
    Write-Host "MIP SDK Setup Options:" -ForegroundColor Green
    Write-Host "=====================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Option 1: Standard Installation (Recommended)" -ForegroundColor Yellow
    Write-Host "  1. Download MIP SDK from Microsoft" -ForegroundColor White
    Write-Host "  2. Run the installer as Administrator" -ForegroundColor White
    Write-Host "  3. Install to: $TargetPath" -ForegroundColor White
    Write-Host "  4. Run: .\scripts\setup-mip-sdk.ps1 -VerifyInstallation" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Option 2: Manual File Copy" -ForegroundColor Yellow
    Write-Host "  1. Obtain MIP SDK files (from another installation or download)" -ForegroundColor White
    Write-Host "  2. Run: .\scripts\setup-mip-sdk.ps1 -SdkSourcePath 'C:\Path\To\Source' -TargetPath '$TargetPath'" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Option 3: Create Structure Only" -ForegroundColor Yellow
    Write-Host "  Run: .\scripts\setup-mip-sdk.ps1 -CreateStructure" -ForegroundColor Cyan
    Write-Host "  Then manually copy files to the created directories" -ForegroundColor White
    Write-Host ""
    Write-Host "For help, run: .\scripts\setup-mip-sdk.ps1 -Help" -ForegroundColor Cyan
}

