# Contributing to ProtectSuite

Thank you for your interest in contributing to ProtectSuite! This project demonstrates Microsoft Information Protection SDKs and serves as a reference implementation.

## How to Contribute

### 1. Fork the Repository
- Fork the repository on GitHub
- Clone your fork locally
- Set up the development environment

### 2. Development Environment Setup
```powershell
# Clone your fork
git clone https://github.com/yourusername/ProtectSuite.git
cd ProtectSuite

# Setup environment (auto-detects SDK paths)
.\scripts\setup-environment.ps1 -AutoDetect -UpdateConfig

# Build the project
.\scripts\build.ps1 -Configuration Debug64
```

### 3. Making Changes
- Create a feature branch: `git checkout -b feature/your-feature-name`
- Make your changes
- Test thoroughly
- Update documentation if needed

### 4. Testing
- Ensure all projects build successfully
- Test on different configurations (Debug, Release, Debug64, Release64)
- Verify portable configuration works
- Test migration to a clean environment

### 5. Submitting Changes
- Commit your changes: `git commit -m "Add your feature"`
- Push to your fork: `git push origin feature/your-feature-name`
- Create a Pull Request on GitHub

## Areas for Contribution

### Code Contributions
- **Additional Protection Scenarios**: New protection patterns or use cases
- **Enhanced UI/UX**: Improved user interface and experience
- **Additional Authentication Methods**: Support for more auth providers
- **Performance Optimizations**: Faster builds, better memory usage
- **Error Handling**: Better error messages and recovery

### Documentation
- **Code Comments**: Improve inline documentation
- **README Updates**: Keep documentation current
- **Migration Guides**: Help with different migration scenarios
- **Tutorials**: Step-by-step guides for specific use cases

### Testing
- **Unit Tests**: Add comprehensive test coverage
- **Integration Tests**: Test SDK integrations
- **Migration Tests**: Verify portability across environments
- **Performance Tests**: Benchmark different scenarios

### Infrastructure
- **CI/CD**: GitHub Actions for automated builds
- **Docker**: Containerized development environment
- **Package Management**: NuGet packages for reusable components

## Coding Standards

### C# Code
- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Handle exceptions appropriately

### C++ Code
- Follow C++ naming conventions
- Use RAII principles
- Handle Windows API errors properly
- Add comments for complex logic

### General
- Keep functions small and focused
- Use consistent indentation (4 spaces)
- Add comments for non-obvious code
- Update documentation when changing APIs

## Pull Request Guidelines

### Before Submitting
- [ ] Code builds without errors
- [ ] All tests pass
- [ ] Documentation is updated
- [ ] Changes are tested on multiple configurations
- [ ] Migration compatibility is verified

### PR Description
- Clearly describe what the PR does
- Reference any related issues
- Include screenshots for UI changes
- Document any breaking changes

### Review Process
- All PRs require review
- Address feedback promptly
- Keep PRs focused and small
- Update PR if requested

## Development Workflow

### Branch Naming
- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring

### Commit Messages
- Use clear, descriptive messages
- Start with a verb (Add, Fix, Update, Remove)
- Reference issues when applicable
- Keep first line under 50 characters

### Testing Strategy
1. **Local Testing**: Test on your development machine
2. **Clean Environment**: Test migration to fresh environment
3. **Multiple Configurations**: Test Debug, Release, Debug64, Release64
4. **Different SDK Versions**: Test with different SDK versions

## Getting Help

### Documentation
- Check the README.md for setup instructions
- Review MIGRATION_GUIDE.md for portability issues
- Look at existing code for examples

### Issues
- Search existing issues before creating new ones
- Provide detailed information about your environment
- Include error messages and logs
- Describe steps to reproduce issues

### Discussions
- Use GitHub Discussions for questions
- Share ideas and suggestions
- Help other contributors

## License

By contributing to ProtectSuite, you agree that your contributions will be licensed under the MIT License. Please note that this project demonstrates Microsoft Information Protection SDKs - refer to Microsoft's licensing terms for the underlying SDKs.

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Respect different viewpoints and experiences

Thank you for contributing to ProtectSuite! 🎉
