#include <windows.h>

extern "C" __declspec(dllexport) int __cdecl mip_init()
{
    return 0; // placeholder
}

extern "C" __declspec(dllexport) int __cdecl mip_protect(const wchar_t* inFile, const wchar_t* outFile)
{
    // TODO: call MIP C++ SDK to protect
    (void)inFile; (void)outFile;
    return 0;
}

extern "C" __declspec(dllexport) int __cdecl mip_unprotect(const wchar_t* inFile, const wchar_t* outFile)
{
    // TODO: call MIP C++ SDK to unprotect
    (void)inFile; (void)outFile;
    return 0;
}

extern "C" __declspec(dllexport) int __cdecl mip_getinfo(const wchar_t* inFile, wchar_t* info, int capacity)
{
    // TODO: call MIP C++ SDK to query info
    (void)inFile;
    const wchar_t* msg = L"MIP info placeholder";
    if (!info || capacity <= 0) return -1;
    wcsncpy_s(info, capacity, msg, _TRUNCATE);
    return 0;
}


