// MIP Native Bridge - C++ wrapper for MIP SDK
// This bridge exposes C-style functions that can be called from C# via P/Invoke

#include <windows.h>
#include <string>
#include <memory>
#include <vector>
#include <sstream>
#include <fstream>

// MIP SDK includes
#include <mip/file/file_profile.h>
#include <mip/file/file_engine.h>
#include <mip/file/file_handler.h>
#include <mip/protection/protection_descriptor_builder.h>
#include <mip/protection/protection_descriptor.h>
#include <mip/common_types.h>
#include <mip/error.h>

using namespace mip;
using namespace std;

// Global state
static shared_ptr<FileProfile> g_fileProfile = nullptr;
static shared_ptr<FileEngine> g_fileEngine = nullptr;
static bool g_initialized = false;

// Helper function to convert HRESULT to error code
static int HrToErrorCode(HRESULT hr)
{
    if (SUCCEEDED(hr))
        return 0;
    // Return negative error code
    return -(hr & 0x7FFFFFFF);
}

// Helper function to get error message
static wstring GetErrorMessage(HRESULT hr)
{
    wstringstream ss;
    ss << L"Error 0x" << hex << hr;
    return ss.str();
}

// Initialize MIP SDK
extern "C" __declspec(dllexport) int __cdecl mip_init()
{
    if (g_initialized)
        return 0;

    try
    {
        // Create file profile settings
        FileProfile::Settings profileSettings(
            "ProtectSuite",
            "",  // Client data - can be empty for basic usage
            mip::ApplicationInfo{
                "ProtectSuite",
                "1.0",
                "ProtectSuite MIP Bridge"
            },
            false  // loadSensitivityTypes
        );

        // Create file profile
        // Note: This is a simplified initialization
        // In production, you would need proper authentication and engine settings
        g_initialized = true;
        return 0;
    }
    catch (const mip::Error& e)
    {
        return HrToErrorCode(e.GetCode());
    }
    catch (...)
    {
        return -1;
    }
}

// Protect a file using MIP SDK
extern "C" __declspec(dllexport) int __cdecl mip_protect(
    const wchar_t* inFile,
    const wchar_t* outFile,
    const wchar_t* templateId,
    const wchar_t* labelId)
{
    if (!g_initialized)
    {
        int rc = mip_init();
        if (rc != 0)
            return rc;
    }

    if (!inFile || !outFile)
        return -1;

    try
    {
        // Read input file
        ifstream inputFile(inFile, ios::binary);
        if (!inputFile.is_open())
            return -2; // File not found

        // Get file size
        inputFile.seekg(0, ios::end);
        size_t fileSize = static_cast<size_t>(inputFile.tellg());
        inputFile.seekg(0, ios::beg);

        // Read file content
        vector<uint8_t> fileData(fileSize);
        inputFile.read(reinterpret_cast<char*>(fileData.data()), fileSize);
        inputFile.close();

        // For now, create a simple protected file format
        // In production, you would use MIP SDK FileHandler to protect
        ofstream outputFile(outFile, ios::binary);
        if (!outputFile.is_open())
            return -3; // Cannot create output file

        // Write MIP protection header (simplified)
        const char* header = "MIP_PROTECTED_FILE\0";
        outputFile.write(header, strlen(header) + 1);

        // Write template ID if provided
        if (templateId && wcslen(templateId) > 0)
        {
            size_t templateLen = wcslen(templateId);
            outputFile.write(reinterpret_cast<const char*>(&templateLen), sizeof(templateLen));
            // Convert wide string to UTF-8
            int utf8Len = WideCharToMultiByte(CP_UTF8, 0, templateId, -1, nullptr, 0, nullptr, nullptr);
            vector<char> utf8Template(utf8Len);
            WideCharToMultiByte(CP_UTF8, 0, templateId, -1, utf8Template.data(), utf8Len, nullptr, nullptr);
            outputFile.write(utf8Template.data(), utf8Len - 1); // -1 to exclude null terminator
        }
        else
        {
            size_t zero = 0;
            outputFile.write(reinterpret_cast<const char*>(&zero), sizeof(zero));
        }

        // Write label ID if provided
        if (labelId && wcslen(labelId) > 0)
        {
            size_t labelLen = wcslen(labelId);
            outputFile.write(reinterpret_cast<const char*>(&labelLen), sizeof(labelLen));
            int utf8Len = WideCharToMultiByte(CP_UTF8, 0, labelId, -1, nullptr, 0, nullptr, nullptr);
            vector<char> utf8Label(utf8Len);
            WideCharToMultiByte(CP_UTF8, 0, labelId, -1, utf8Label.data(), utf8Len, nullptr, nullptr);
            outputFile.write(utf8Label.data(), utf8Len - 1);
        }
        else
        {
            size_t zero = 0;
            outputFile.write(reinterpret_cast<const char*>(&zero), sizeof(zero));
        }

        // Write file data (in production, this would be encrypted)
        outputFile.write(reinterpret_cast<const char*>(fileData.data()), fileData.size());
        outputFile.close();

        return 0;
    }
    catch (const mip::Error& e)
    {
        return HrToErrorCode(e.GetCode());
    }
    catch (...)
    {
        return -1;
    }
}

// Unprotect a file using MIP SDK
extern "C" __declspec(dllexport) int __cdecl mip_unprotect(
    const wchar_t* inFile,
    const wchar_t* outFile)
{
    if (!g_initialized)
    {
        int rc = mip_init();
        if (rc != 0)
            return rc;
    }

    if (!inFile || !outFile)
        return -1;

    try
    {
        // Read protected file
        ifstream inputFile(inFile, ios::binary);
        if (!inputFile.is_open())
            return -2; // File not found

        // Read and verify header
        char header[20] = { 0 };
        inputFile.read(header, 19);
        if (strcmp(header, "MIP_PROTECTED_FILE") != 0)
            return -4; // Not a protected file

        // Read template ID length
        size_t templateLen = 0;
        inputFile.read(reinterpret_cast<char*>(&templateLen), sizeof(templateLen));
        if (templateLen > 0)
        {
            // Skip template ID
            vector<char> templateData(templateLen);
            inputFile.read(templateData.data(), templateLen);
        }

        // Read label ID length
        size_t labelLen = 0;
        inputFile.read(reinterpret_cast<char*>(&labelLen), sizeof(labelLen));
        if (labelLen > 0)
        {
            // Skip label ID
            vector<char> labelData(labelLen);
            inputFile.read(labelData.data(), labelLen);
        }

        // Get remaining file size
        inputFile.seekg(0, ios::end);
        size_t totalSize = static_cast<size_t>(inputFile.tellg());
        size_t headerSize = 19 + sizeof(size_t) + templateLen + sizeof(size_t) + labelLen;
        size_t dataSize = totalSize - headerSize;

        // Read file data
        inputFile.seekg(headerSize, ios::beg);
        vector<uint8_t> fileData(dataSize);
        inputFile.read(reinterpret_cast<char*>(fileData.data()), dataSize);
        inputFile.close();

        // Write unprotected file
        ofstream outputFile(outFile, ios::binary);
        if (!outputFile.is_open())
            return -3; // Cannot create output file

        outputFile.write(reinterpret_cast<const char*>(fileData.data()), fileData.size());
        outputFile.close();

        return 0;
    }
    catch (const mip::Error& e)
    {
        return HrToErrorCode(e.GetCode());
    }
    catch (...)
    {
        return -1;
    }
}

// Get protection information from a file
extern "C" __declspec(dllexport) int __cdecl mip_getinfo(
    const wchar_t* inFile,
    wchar_t* info,
    int capacity)
{
    if (!inFile || !info || capacity <= 0)
        return -1;

    try
    {
        ifstream inputFile(inFile, ios::binary);
        if (!inputFile.is_open())
        {
            wcscpy_s(info, capacity, L"File not found");
            return -2;
        }

        // Read and verify header
        char header[20] = { 0 };
        inputFile.read(header, 19);
        
        wstringstream infoStream;
        
        if (strcmp(header, "MIP_PROTECTED_FILE") == 0)
        {
            infoStream << L"Status: Protected\r\n";
            
            // Read template ID
            size_t templateLen = 0;
            inputFile.read(reinterpret_cast<char*>(&templateLen), sizeof(templateLen));
            if (templateLen > 0)
            {
                vector<char> templateData(templateLen);
                inputFile.read(templateData.data(), templateLen);
                templateData.push_back('\0');
                
                int wideLen = MultiByteToWideChar(CP_UTF8, 0, templateData.data(), -1, nullptr, 0);
                vector<wchar_t> wideTemplate(wideLen);
                MultiByteToWideChar(CP_UTF8, 0, templateData.data(), -1, wideTemplate.data(), wideLen);
                infoStream << L"Template ID: " << wideTemplate.data() << L"\r\n";
            }
            
            // Read label ID
            size_t labelLen = 0;
            inputFile.read(reinterpret_cast<char*>(&labelLen), sizeof(labelLen));
            if (labelLen > 0)
            {
                vector<char> labelData(labelLen);
                inputFile.read(labelData.data(), labelLen);
                labelData.push_back('\0');
                
                int wideLen = MultiByteToWideChar(CP_UTF8, 0, labelData.data(), -1, nullptr, 0);
                vector<wchar_t> wideLabel(wideLen);
                MultiByteToWideChar(CP_UTF8, 0, labelData.data(), -1, wideLabel.data(), wideLen);
                infoStream << L"Label ID: " << wideLabel.data() << L"\r\n";
            }
        }
        else
        {
            infoStream << L"Status: Unprotected\r\n";
        }
        
        inputFile.close();
        
        wstring infoStr = infoStream.str();
        if (infoStr.length() >= static_cast<size_t>(capacity))
            infoStr = infoStr.substr(0, capacity - 1);
        
        wcscpy_s(info, capacity, infoStr.c_str());
        return 0;
    }
    catch (...)
    {
        wcscpy_s(info, capacity, L"Error reading file information");
        return -1;
    }
}

// Cleanup MIP SDK resources
extern "C" __declspec(dllexport) void __cdecl mip_cleanup()
{
    g_fileEngine = nullptr;
    g_fileProfile = nullptr;
    g_initialized = false;
}