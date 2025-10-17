# GitHub Upload Guide for ProtectSuite

This guide will help you upload the ProtectSuite project to GitHub.

## Prerequisites

- Git installed on your system
- GitHub account
- PowerShell execution policy allows running scripts

## Quick Setup (Recommended)

### 1. Run the Setup Script
```powershell
# Navigate to project directory
cd "D:\Juanjo\Proyectos\ProtectSuite"

# Run the GitHub setup script
.\scripts\setup-github.ps1 -SetupGit -GitHubUsername "yourusername" -GitHubEmail "your@email.com"
```

### 2. Create GitHub Repository
```powershell
# Run the remote creation helper
.\scripts\setup-github.ps1 -CreateRemote -GitHubUsername "yourusername" -RepositoryName "ProtectSuite"
```

### 3. Follow the GitHub Instructions
The script will provide step-by-step instructions to create the repository on GitHub.

### 4. Push to GitHub
After creating the repository, run these commands:
```bash
git remote add origin https://github.com/yourusername/ProtectSuite.git
git branch -M main
git push -u origin main
```

## Manual Setup

### 1. Configure Git (if not already done)
```bash
git config --global user.name "Your Name"
git config --global user.email "your@email.com"
```

### 2. Create Initial Commit
```bash
# Add all files
git add .

# Create commit
git commit -m "Initial commit: ProtectSuite - Microsoft Information Protection SDK Demo

- MSIPC C# WinForms application with OAuth2 and AD RMS support
- MSIPC C++ Win32 native application with real file encryption
- Template picker and custom rights configuration dialogs
- Portable configuration system for easy migration
- Comprehensive documentation and migration guides
- VS Code integration with debug configurations
- Build scripts with auto-detection of SDK paths"
```

### 3. Create GitHub Repository
1. Go to https://github.com/new
2. Set Repository name: `ProtectSuite`
3. Set Description: `Microsoft Information Protection SDK Demo Suite`
4. Make it Public or Private as desired
5. **DO NOT** initialize with README, .gitignore, or license (we already have them)
6. Click "Create repository"

### 4. Connect Local Repository to GitHub
```bash
# Add remote origin
git remote add origin https://github.com/yourusername/ProtectSuite.git

# Rename default branch to main
git branch -M main

# Push to GitHub
git push -u origin main
```

## Repository Structure

The uploaded repository will include:

```
ProtectSuite/
├── .gitignore                  # Git ignore rules
├── .vscode/                    # VS Code configuration
├── config/                     # Configuration files
├── scripts/                    # Build and setup scripts
├── Msipc.CSharp.WinForms/      # MSIPC C# application
├── Msipc.Cpp.Win32/           # MSIPC C++ application
├── IpcManagedAPI/             # MSIPC managed API wrapper
├── README.md                  # Main documentation
├── LICENSE                    # MIT License
├── CONTRIBUTING.md            # Contribution guidelines
├── MIGRATION_GUIDE.md         # Migration instructions
├── SDK_INSTALLATION_GUIDE.md  # SDK installation guide
├── GITHUB_UPLOAD_GUIDE.md     # This file
├── Directory.Build.props      # MSBuild configuration
└── ProtectSuite.sln          # Visual Studio solution
```

## What's Included

### ✅ Ready for GitHub
- **Complete source code** for both MSIPC applications
- **Comprehensive documentation** with setup and migration guides
- **Portable configuration** system for easy migration
- **Build scripts** with auto-detection
- **VS Code integration** with debug configurations
- **Proper .gitignore** to exclude build outputs and temporary files
- **MIT License** for open source distribution
- **Contributing guidelines** for community contributions

### 🚫 Excluded from Repository
- Build outputs (`bin/`, `obj/`, `Debug/`, `Release/`)
- Visual Studio user files (`.user`, `.suo`)
- Temporary files and logs
- SDK files (users will install these separately)
- Environment-specific configurations

## Post-Upload Steps

### 1. Verify Upload
- Check that all files are present on GitHub
- Verify the README displays correctly
- Test cloning the repository on a different machine

### 2. Set Repository Settings
- Add topics/tags: `msipc`, `mip`, `microsoft-information-protection`, `csharp`, `cpp`, `winforms`, `win32`
- Set up branch protection rules if desired
- Configure issue templates if needed

### 3. Test Migration
- Clone the repository on a different machine
- Follow the migration guide to set up the environment
- Verify the build process works

### 4. Community Setup
- Enable GitHub Discussions for community questions
- Set up GitHub Actions for CI/CD (optional)
- Create release tags for stable versions

## Troubleshooting

### Common Issues

1. **"Permission denied" when pushing**
   - Use GitHub Personal Access Token instead of password
   - Or set up SSH keys for authentication

2. **"Repository not found"**
   - Check the repository URL is correct
   - Verify you have push access to the repository

3. **"Large file" errors**
   - Check .gitignore is working correctly
   - Remove any large files that shouldn't be in the repository

4. **"LF will be replaced by CRLF" warnings**
   - These are normal on Windows
   - Git is handling line ending conversion automatically

### Verification Commands

```bash
# Check repository status
git status

# Check remote configuration
git remote -v

# Check branch information
git branch -a

# Check commit history
git log --oneline
```

## Next Steps After Upload

1. **Test the migration process** on a clean machine
2. **Create a release** with the initial version
3. **Share the repository** with your team or community
4. **Monitor issues and discussions** for user feedback
5. **Plan future enhancements** based on community needs

## Repository Description

Use this description for your GitHub repository:

```
Microsoft Information Protection SDK Demo Suite

A comprehensive demonstration of Microsoft Information Protection (MIP) and Microsoft Information Protection and Control (MSIPC) SDKs for file protection, unprotection, and information retrieval.

Features:
- MSIPC C# WinForms application with OAuth2 and AD RMS support
- MSIPC C++ Win32 native application with real file encryption
- Template picker and custom rights configuration dialogs
- Portable configuration system for easy migration
- Comprehensive documentation and migration guides
- VS Code integration with debug configurations

Perfect for learning Microsoft Information Protection SDKs and as a reference implementation.
```

Your ProtectSuite project is now ready for GitHub! 🎉
