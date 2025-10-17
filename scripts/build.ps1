# ProtectSuite Build Script
# This script builds the ProtectSuite solution with proper SDK path resolution

param(
    [string]$Configuration = "Debug",
    [string]$Platform = "x64",
    [switch]$Clean = $false,
    [switch]$Restore = $false,
    [string]$Project = "",
    [switch]$Verbose = $false
)

Write-Host "ProtectSuite Build Script" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

# Load SDK configuration
$configPath = "config\sdk-paths.json"
if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    Write-Host "Loaded SDK configuration from $configPath" -ForegroundColor Green
} else {
    Write-Host "Configuration file not found: $configPath" -ForegroundColor Red
    Write-Host "Please run setup-environment.ps1 first" -ForegroundColor Yellow
    exit 1
}

# Resolve MSBuild path
$msbuildPath = $config.buildTools.msbuildPath
if (-not (Test-Path $msbuildPath)) {
    Write-Host "MSBuild not found at: $msbuildPath" -ForegroundColor Red
    Write-Host "Please run setup-environment.ps1 to detect correct paths" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using MSBuild: $msbuildPath" -ForegroundColor Green

# Set environment variables for SDK paths
if ($config.msipc.sdkPath) {
    $env:MSIPC_SDK_PATH = $config.msipc.sdkPath
    $env:MSIPC_INCLUDE_PATH = $config.msipc.includePath
    $env:MSIPC_LIB_PATH = $config.msipc.libPath
    Write-Host "MSIPC SDK Path: $($config.msipc.sdkPath)" -ForegroundColor Green
}

if ($config.mip.sdkPath) {
    $env:MIP_SDK_PATH = $config.mip.sdkPath
    $env:MIP_INCLUDE_PATH = $config.mip.includePath
    $env:MIP_LIB_PATH = $config.mip.libPath
    Write-Host "MIP SDK Path: $($config.mip.sdkPath)" -ForegroundColor Green
}

# Build arguments
$buildArgs = @()

if ($Clean) {
    $buildArgs += "/t:Clean"
    Write-Host "Clean build requested" -ForegroundColor Yellow
}

if ($Restore) {
    $buildArgs += "/t:Restore"
    Write-Host "Restore packages requested" -ForegroundColor Yellow
}

$buildArgs += "/t:Build"
$buildArgs += "/p:Configuration=$Configuration"
$buildArgs += "/p:Platform=$Platform"

if ($Verbose) {
    $buildArgs += "/v:detailed"
}

# Determine target
if ($Project) {
    # For individual projects, build the project file directly
    $projectFile = ""
    switch ($Project) {
        "Msipc.CSharp.WinForms" { $projectFile = "Msipc.CSharp.WinForms\Msipc.CSharp.WinForms.csproj" }
        "Msipc.Cpp.Win32" { $projectFile = "Msipc.Cpp.Win32\Msipc.Cpp.Win32.vcxproj" }
        "Mip.CSharp.WinForms" { $projectFile = "Mip.CSharp.WinForms\Mip.CSharp.WinForms.csproj" }
        "Mip.CppBridge.WinForms" { $projectFile = "Mip.CppBridge.WinForms\Mip.CppBridge.WinForms.csproj" }
        default { 
            Write-Host "Unknown project: $Project" -ForegroundColor Red
            exit 1
        }
    }
    
    if (-not (Test-Path $projectFile)) {
        Write-Host "Project file not found: $projectFile" -ForegroundColor Red
        exit 1
    }
    
    $target = $projectFile
    Write-Host "Building project: $Project ($projectFile)" -ForegroundColor Green
} else {
    $target = "ProtectSuite.sln"
    Write-Host "Building solution: $target" -ForegroundColor Green
}

Write-Host "Configuration: $Configuration" -ForegroundColor Green
Write-Host "Platform: $Platform" -ForegroundColor Green

# Execute build
$buildCommand = "& `"$msbuildPath`" `"$target`" $($buildArgs -join ' ')"
Write-Host "Executing: $buildCommand" -ForegroundColor Cyan

try {
    Invoke-Expression $buildCommand
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "`nBuild failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "`nBuild failed with error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
