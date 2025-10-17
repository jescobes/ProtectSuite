# ProtectSuite GitHub Setup Script
# This script helps set up Git and prepare the project for GitHub upload

param(
    [string]$GitHubUsername = "",
    [string]$GitHubEmail = "",
    [string]$RepositoryName = "ProtectSuite",
    [switch]$SetupGit = $false,
    [switch]$CreateRemote = $false
)

Write-Host "ProtectSuite GitHub Setup" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

# Function to setup Git configuration
function Setup-GitConfig {
    param(
        [string]$Username,
        [string]$Email
    )
    
    if ([string]::IsNullOrEmpty($Username)) {
        $Username = Read-Host "Enter your GitHub username"
    }
    
    if ([string]::IsNullOrEmpty($Email)) {
        $Email = Read-Host "Enter your GitHub email"
    }
    
    Write-Host "Setting up Git configuration..." -ForegroundColor Yellow
    
    # Set global Git configuration
    git config --global user.name $Username
    git config --global user.email $Email
    
    # Set local repository configuration
    git config user.name $Username
    git config user.email $Email
    
    Write-Host "Git configuration set:" -ForegroundColor Green
    Write-Host "  Name: $Username" -ForegroundColor Green
    Write-Host "  Email: $Email" -ForegroundColor Green
}

# Function to create initial commit
function Create-InitialCommit {
    Write-Host "Creating initial commit..." -ForegroundColor Yellow
    
    # Add all files
    git add .
    
    # Create commit
    $commitMessage = @"
Initial commit: ProtectSuite - Microsoft Information Protection SDK Demo

- MSIPC C# WinForms application with OAuth2 and AD RMS support
- MSIPC C++ Win32 native application with real file encryption
- Template picker and custom rights configuration dialogs
- Portable configuration system for easy migration
- Comprehensive documentation and migration guides
- VS Code integration with debug configurations
- Build scripts with auto-detection of SDK paths

Features:
- Real MSIPC file encryption/decryption using IpcfEncryptFile/IpcfDecryptFile
- OAuth2 integration with suppressUI=true for Purview
- AD RMS server configuration via JSON
- Template selection and custom user rights dialogs
- Multi-layered portable path resolution
- Auto-detection of SDK installations
- Multiple build configurations (Debug, Release, Debug64, Release64)
- Comprehensive documentation and migration guides
"@
    
    git commit -m $commitMessage
    
    Write-Host "Initial commit created successfully!" -ForegroundColor Green
}

# Function to create GitHub repository
function Create-GitHubRepository {
    param(
        [string]$RepoName,
        [string]$Username
    )
    
    Write-Host "Creating GitHub repository..." -ForegroundColor Yellow
    Write-Host "Repository name: $RepoName" -ForegroundColor Cyan
    Write-Host "GitHub username: $Username" -ForegroundColor Cyan
    
    Write-Host "`nTo create the repository on GitHub:" -ForegroundColor Yellow
    Write-Host "1. Go to https://github.com/new" -ForegroundColor White
    Write-Host "2. Set Repository name: $RepoName" -ForegroundColor White
    Write-Host "3. Set Description: Microsoft Information Protection SDK Demo Suite" -ForegroundColor White
    Write-Host "4. Make it Public or Private as desired" -ForegroundColor White
    Write-Host "5. DO NOT initialize with README, .gitignore, or license (we already have them)" -ForegroundColor White
    Write-Host "6. Click 'Create repository'" -ForegroundColor White
    
    Write-Host "`nAfter creating the repository, run these commands:" -ForegroundColor Yellow
    Write-Host "git remote add origin https://github.com/$Username/$RepoName.git" -ForegroundColor Cyan
    Write-Host "git branch -M main" -ForegroundColor Cyan
    Write-Host "git push -u origin main" -ForegroundColor Cyan
}

# Function to show current Git status
function Show-GitStatus {
    Write-Host "`nCurrent Git Status:" -ForegroundColor Yellow
    git status --short
    
    Write-Host "`nGit Configuration:" -ForegroundColor Yellow
    Write-Host "  Name: $(git config user.name)" -ForegroundColor White
    Write-Host "  Email: $(git config user.email)" -ForegroundColor White
    
    Write-Host "`nRepository Information:" -ForegroundColor Yellow
    Write-Host "  Repository: $(git remote get-url origin 2>$null || 'No remote configured')" -ForegroundColor White
    Write-Host "  Branch: $(git branch --show-current)" -ForegroundColor White
    Write-Host "  Commits: $(git rev-list --count HEAD 2>$null || '0')" -ForegroundColor White
}

# Main execution
if ($SetupGit) {
    Setup-GitConfig -Username $GitHubUsername -Email $GitHubEmail
    Create-InitialCommit
    Show-GitStatus
}

if ($CreateRemote) {
    Create-GitHubRepository -RepoName $RepositoryName -Username $GitHubUsername
}

if (-not $SetupGit -and -not $CreateRemote) {
    Write-Host "Usage examples:" -ForegroundColor Yellow
    Write-Host "  .\scripts\setup-github.ps1 -SetupGit -GitHubUsername 'yourusername' -GitHubEmail 'your@email.com'" -ForegroundColor Cyan
    Write-Host "  .\scripts\setup-github.ps1 -CreateRemote -GitHubUsername 'yourusername' -RepositoryName 'ProtectSuite'" -ForegroundColor Cyan
    Write-Host "  .\scripts\setup-github.ps1 -SetupGit -CreateRemote -GitHubUsername 'yourusername' -GitHubEmail 'your@email.com'" -ForegroundColor Cyan
    
    Write-Host "`nCurrent status:" -ForegroundColor Yellow
    Show-GitStatus
}
