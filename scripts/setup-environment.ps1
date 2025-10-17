# ProtectSuite Environment Setup Script
# This script helps configure the development environment for ProtectSuite

param(
    [string]$MsipcSdkPath = "",
    [string]$MipSdkPath = "",
    [string]$BuildToolsPath = "",
    [string]$WindowsSdkPath = "",
    [switch]$AutoDetect = $false,
    [switch]$UpdateConfig = $false
)

Write-Host "ProtectSuite Environment Setup" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green

# Function to find MSIPC SDK
function Find-MsipcSdk {
    $commonPaths = @(
        "C:\Program Files\SDKs\MSIPC",
        "D:\Program Files\SDKs\MSIPC",
        "C:\Program Files (x86)\SDKs\MSIPC",
        "D:\Program Files (x86)\SDKs\MSIPC"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path "$path\inc\msipc.h") {
            return $path
        }
    }
    
    # Try to find via registry
    $regPath = "HKLM:\SOFTWARE\Microsoft\MSIPC"
    if (Test-Path $regPath) {
        $sdkPath = Get-ItemProperty -Path $regPath -Name "InstallPath" -ErrorAction SilentlyContinue
        if ($sdkPath -and (Test-Path "$($sdkPath.InstallPath)\inc\msipc.h")) {
            return $sdkPath.InstallPath
        }
    }
    
    return $null
}

# Function to find MIP SDK
function Find-MipSdk {
    $commonPaths = @(
        "C:\Program Files\SDKs\MIP",
        "D:\Program Files\SDKs\MIP",
        "C:\Program Files (x86)\SDKs\MIP",
        "D:\Program Files (x86)\SDKs\MIP"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path "$path\include") {
            return $path
        }
    }
    
    return $null
}

# Function to find Visual Studio Build Tools
function Find-BuildTools {
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vsWhere) {
        $vsInstallation = & $vsWhere -latest -products * -requires Microsoft.VisualStudio.Component.MSBuild -property installationPath
        if ($vsInstallation) {
            return "$vsInstallation\MSBuild\Current\Bin\MSBuild.exe"
        }
    }
    
    # Fallback to common paths
    $commonPaths = @(
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "D:\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    return $null
}

# Function to find Windows SDK
function Find-WindowsSdk {
    $commonPaths = @(
        "C:\Program Files (x86)\Windows Kits\10",
        "C:\Program Files\Windows Kits\10"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    return $null
}

# Auto-detect paths if requested
if ($AutoDetect) {
    Write-Host "Auto-detecting SDK paths..." -ForegroundColor Yellow
    
    if (-not $MsipcSdkPath) {
        $MsipcSdkPath = Find-MsipcSdk
        if ($MsipcSdkPath) {
            Write-Host "Found MSIPC SDK: $MsipcSdkPath" -ForegroundColor Green
        } else {
            Write-Host "MSIPC SDK not found. Please install it manually." -ForegroundColor Red
        }
    }
    
    if (-not $MipSdkPath) {
        $MipSdkPath = Find-MipSdk
        if ($MipSdkPath) {
            Write-Host "Found MIP SDK: $MipSdkPath" -ForegroundColor Green
        } else {
            Write-Host "MIP SDK not found. Please install it manually." -ForegroundColor Yellow
        }
    }
    
    if (-not $BuildToolsPath) {
        $BuildToolsPath = Find-BuildTools
        if ($BuildToolsPath) {
            Write-Host "Found Build Tools: $BuildToolsPath" -ForegroundColor Green
        } else {
            Write-Host "Build Tools not found. Please install Visual Studio Build Tools." -ForegroundColor Red
        }
    }
    
    if (-not $WindowsSdkPath) {
        $WindowsSdkPath = Find-WindowsSdk
        if ($WindowsSdkPath) {
            Write-Host "Found Windows SDK: $WindowsSdkPath" -ForegroundColor Green
        } else {
            Write-Host "Windows SDK not found. Please install Windows 10 SDK." -ForegroundColor Red
        }
    }
}

# Update configuration file
if ($UpdateConfig) {
    $configPath = "config\sdk-paths.json"
    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json
        
        if ($MsipcSdkPath) {
            $config.msipc.sdkPath = $MsipcSdkPath
            $config.msipc.includePath = "$MsipcSdkPath\inc"
            $config.msipc.libPath = "$MsipcSdkPath\lib"
            $config.msipc.binPath = "$MsipcSdkPath\bin"
            $config.msipc.redistPath = "$MsipcSdkPath\redist"
        }
        
        if ($MipSdkPath) {
            $config.mip.sdkPath = $MipSdkPath
            $config.mip.includePath = "$MipSdkPath\include"
            $config.mip.libPath = "$MipSdkPath\lib"
            $config.mip.binPath = "$MipSdkPath\bin"
        }
        
        if ($BuildToolsPath) {
            $config.buildTools.msbuildPath = $BuildToolsPath
        }
        
        if ($WindowsSdkPath) {
            $config.buildTools.windowsSdkPath = $WindowsSdkPath
        }
        
        $config | ConvertTo-Json -Depth 3 | Set-Content $configPath
        Write-Host "Updated configuration file: $configPath" -ForegroundColor Green
    }
}

# Set environment variables
Write-Host "Setting environment variables..." -ForegroundColor Yellow

if ($MsipcSdkPath) {
    $env:MSIPC_SDK_PATH = $MsipcSdkPath
    $env:MSIPC_INCLUDE_PATH = "$MsipcSdkPath\inc"
    $env:MSIPC_LIB_PATH = "$MsipcSdkPath\lib"
    Write-Host "Set MSIPC_SDK_PATH = $MsipcSdkPath" -ForegroundColor Green
}

if ($MipSdkPath) {
    $env:MIP_SDK_PATH = $MipSdkPath
    $env:MIP_INCLUDE_PATH = "$MipSdkPath\include"
    $env:MIP_LIB_PATH = "$MipSdkPath\lib"
    Write-Host "Set MIP_SDK_PATH = $MipSdkPath" -ForegroundColor Green
}

if ($BuildToolsPath) {
    $env:MSBUILD_PATH = $BuildToolsPath
    Write-Host "Set MSBUILD_PATH = $BuildToolsPath" -ForegroundColor Green
}

Write-Host "`nEnvironment setup complete!" -ForegroundColor Green
Write-Host "You can now build the project using the detected paths." -ForegroundColor Green
