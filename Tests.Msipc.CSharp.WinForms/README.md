# MSIPC Unit Tests

This project contains comprehensive unit tests for MSIPC (Microsoft Information Protection and Control) API functions, covering all the API calls that Office makes when working with protected documents.

## Test Coverage

The tests are organized into the following categories, based on the MSIPC API calls seen in Office logs:

### 1. Core API Tests (`MsipcApiTests.cs`)
- `IpcInitializeEnvironment` - Initialize MSIPC environment
- `IpcGetGlobalProperty` / `IpcSetGlobalProperty` - Get/Set global properties
- `IpcGetAPIMode` / `IpcSetAPIMode` - Get/Set API mode (Client/Server)
- `IpcGetTemplateList` - Get list of available templates
- `IpcGetTemplateIssuerList` - Get list of template issuers

### 2. Key API Tests (`MsipcKeyTests.cs`)
- `IpcGetKey` (IpcpGetKey) - Get key from serialized license
- `IpcGetKeyProperty` - Get key properties:
  - Block size
  - License
  - User display name
- `IpcCloseHandle` - Close key handle
- `IpcFreeMemory` - Free memory allocated by MSIPC

### 3. Access Control Tests (`MsipcAccessControlTests.cs`)
- `IpcAccessCheck` - Check user access rights:
  - VIEW
  - EDIT
  - EXPORT
  - PRINT
  - FORWARD
  - OWNER (Full Control)

### 4. License Property Tests (`MsipcLicenseTests.cs`)
- `IpcGetLicenseProperty` - Get license properties:
  - ContentId
  - Owner
  - UserRightsList
  - ValidityTime
  - IntervalTime
  - ConnectionInfo
  - Descriptor (TemplateInfo)
- `IpcSetLicenseProperty` - Set license properties

### 5. File API Tests (`MsipcFileApiTests.cs`)
- `IpcfIsFileEncrypted` - Check if file is encrypted
- `IpcfEncryptFile` - Encrypt file (with template ID or license handle)
- `IpcfDecryptFile` - Decrypt file
- `IpcfGetSerializedLicenseFromFile` - Get license from protected file
- `IpcfEncryptFileStream` / `IpcfDecryptFileStream` - Stream-based operations
- Get file info without Full Control (VIEW rights scenario)

### 6. Template Tests (`MsipcTemplateTests.cs`)
- `IpcCreateLicenseFromTemplateId` - Create license from template ID
- `IpcCreateLicenseFromScratch` - Create license from scratch
- `IpcSerializeLicense` - Serialize license (from template ID or license handle)
- Template property access

### 7. Republishing License Tests (`MsipcRepublishingLicenseTests.cs`)
- `IpcCreateRepublishingLicense` - Create republishing license
  - Note: This API may not be available in all MSIPC SDK versions

### 8. Integration Tests (`MsipcIntegrationTests.cs`)
Complete workflows simulating Office operations:
- **Complete Protection Workflow**: Get templates → Create license → Encrypt file → Verify encryption
- **Complete Opening Workflow**: Check encryption → Get license → Get key → Check access → Get properties
- **View Rights Workflow**: Opening document with VIEW rights (not Full Control)
- **Unprotection Workflow**: Decrypt file → Verify content

## API Coverage Based on Office Log

The tests cover all MSIPC API calls identified in Office logs when opening protected documents:

1. ✅ `IpcInitializeEnvironment` - Environment initialization
2. ✅ `IpcGetTemplateList` - Template discovery
3. ✅ `IpcGetKey` (IpcpGetKey) - Primary API for getting keys from licenses
4. ✅ `IpcGetKeyProperty` - Multiple calls for block size, license, user display name
5. ✅ `IpcAccessCheck` - Multiple calls for various rights (VIEW, EDIT, PRINT, etc.)
6. ✅ `IpcGetLicenseProperty` - Multiple calls for ContentId, Owner, UserRightsList, etc.
7. ✅ `IpcCreateRepublishingLicense` - Creating republishing licenses
8. ✅ `IpcFreeMemory` - Memory management (multiple calls)
9. ✅ `IpcCloseHandle` - Handle cleanup
10. ✅ `IpcfEncryptFile` - File encryption
11. ✅ `IpcfDecryptFile` - File decryption
12. ✅ `IpcfIsFileEncrypted` - File encryption status check
13. ✅ `IpcfGetSerializedLicenseFromFile` - License extraction from files

## Running the Tests

### Prerequisites
- MSIPC SDK installed and configured
- AD RMS server or Purview backend available (for tests that require server connectivity)
- xUnit test framework

### Using Visual Studio
1. Open `ProtectSuite.sln`
2. Build the solution
3. Open Test Explorer (Test → Test Explorer)
4. Run all tests or select specific test categories

### Using Command Line
```powershell
# Restore NuGet packages
nuget restore ProtectSuite.sln

# Build the test project
msbuild Tests.Msipc.CSharp.WinForms\Tests.Msipc.CSharp.WinForms.csproj /p:Configuration=Debug64

# Run tests using xUnit console runner
xunit.console.exe Tests.Msipc.CSharp.WinForms\bin\Debug64\Tests.Msipc.CSharp.WinForms.dll
```

### Using dotnet test (if available)
```powershell
dotnet test Tests.Msipc.CSharp.WinForms\Tests.Msipc.CSharp.WinForms.csproj
```

## Test Scenarios

### Typical Scenarios Covered

1. **Validation**: Check if file is encrypted, get license information
2. **Protection**: Encrypt file with template or custom rights
3. **Unprotection**: Decrypt protected file
4. **Get Info**: Extract protection information without Full Control (VIEW rights)
5. **Access Checks**: Verify user permissions for various rights
6. **License Properties**: Get and set license metadata

### Office-Like Workflows

The integration tests simulate complete Office workflows:

- **Opening Protected Document**:
  1. Check file encryption status
  2. Get license from file
  3. Get key from license (`IpcGetKey`)
  4. Get key properties (block size, user display name)
  5. Check access rights (`IpcAccessCheck` for multiple rights)
  6. Get license properties (owner, content ID, user rights)

- **Protecting New Document**:
  1. Get available templates (`IpcGetTemplateList`)
  2. Create license from template (`IpcCreateLicenseFromTemplateId`)
  3. Encrypt file (`IpcfEncryptFile`)
  4. Verify encryption status

- **Unprotecting Document**:
  1. Check encryption status
  2. Decrypt file (`IpcfDecryptFile`)
  3. Verify decrypted content matches original

## Notes

- Some tests may be skipped if MSIPC SDK is not properly configured or server is unavailable
- Tests that require server connectivity will gracefully handle connection failures
- Tests are designed to work with both AD RMS and Purview backends
- Memory management is handled automatically by the SafeNativeMethods wrapper classes

## Test Structure

```
Tests.Msipc.CSharp.WinForms/
├── MsipcApiTests.cs              # Core API tests
├── MsipcKeyTests.cs              # Key API tests
├── MsipcAccessControlTests.cs   # Access control tests
├── MsipcLicenseTests.cs          # License property tests
├── MsipcFileApiTests.cs         # File API tests
├── MsipcTemplateTests.cs        # Template tests
├── MsipcRepublishingLicenseTests.cs  # Republishing license tests
├── MsipcIntegrationTests.cs     # Integration tests
├── TestHelpers.cs               # Test helper utilities
└── Properties/
    └── AssemblyInfo.cs          # Assembly information
```

## Contributing

When adding new tests:
1. Follow the existing test structure
2. Use `TestHelpers` for common operations
3. Handle `InformationProtectionException` gracefully
4. Clean up resources in `Dispose()` methods
5. Add appropriate test documentation comments
