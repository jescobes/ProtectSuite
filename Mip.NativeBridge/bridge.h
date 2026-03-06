// MIP Native Bridge - C API Header
// This header defines the C-style functions exported by Mip.NativeBridge.dll
// These functions can be called from C# via P/Invoke

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

// Initialize MIP SDK
// Returns: 0 on success, negative error code on failure
int __cdecl mip_init(void);

// Protect a file using MIP SDK
// Parameters:
//   inFile: Path to the input file to protect
//   outFile: Path to the output protected file
//   templateId: Optional template ID (can be NULL)
//   labelId: Optional label ID (can be NULL)
// Returns: 0 on success, negative error code on failure
int __cdecl mip_protect(
    const wchar_t* inFile,
    const wchar_t* outFile,
    const wchar_t* templateId,
    const wchar_t* labelId);

// Unprotect a file using MIP SDK
// Parameters:
//   inFile: Path to the protected input file
//   outFile: Path to the output unprotected file
// Returns: 0 on success, negative error code on failure
int __cdecl mip_unprotect(
    const wchar_t* inFile,
    const wchar_t* outFile);

// Get protection information from a file
// Parameters:
//   inFile: Path to the file to query
//   info: Buffer to receive protection information (wide string)
//   capacity: Size of the info buffer in characters
// Returns: 0 on success, negative error code on failure
int __cdecl mip_getinfo(
    const wchar_t* inFile,
    wchar_t* info,
    int capacity);

// Cleanup MIP SDK resources
// Call this when done using the MIP bridge
void __cdecl mip_cleanup(void);

#ifdef __cplusplus
}
#endif
