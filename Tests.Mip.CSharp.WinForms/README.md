# Tests.Mip.CSharp.WinForms

Unit and integration tests for the MIP C# WinForms application and the MIP native bridge.

## Test classes

- **MipApiTests**: `mip_init`, `mip_getinfo` (invalid paths).
- **MipFileApiTests**: `mip_protect`, `mip_unprotect`, `mip_getinfo` with temporary files.
- **MipIntegrationTests**: Full flow (protect → unprotect).

## Running tests

From the solution directory:

```bash
dotnet test Tests.Mip.CSharp.WinForms/Tests.Mip.CSharp.WinForms.csproj
```

Or from Visual Studio: Test Explorer → Run All.

## Notes

- Some tests may be skipped or pass with non-zero bridge return codes when the MIP SDK is not fully configured (e.g. no authentication or templates).
- The bridge requires `Mip.NativeBridge.dll` (and optionally `mip.dll`) in the test output directory; building the solution should copy them when the main app is built.
