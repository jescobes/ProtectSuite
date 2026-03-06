#include <windows.h>
#include <commdlg.h>
#include <commctrl.h>
#include <string>
#include <vector>
#include <sstream>
#include <fstream>
#include <exception>
#include "resource.h"

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

static std::wstring g_selectedFile;
static std::wstring g_templateId;
static std::wstring g_labelId;

static void AppendText(HWND hEdit, const std::wstring& text)
{
    int len = GetWindowTextLengthW(hEdit);
    SendMessageW(hEdit, EM_SETSEL, (WPARAM)len, (LPARAM)len);
    SendMessageW(hEdit, EM_REPLACESEL, FALSE, (LPARAM)text.c_str());
    SendMessageW(hEdit, EM_REPLACESEL, FALSE, (LPARAM)L"\r\n");
}

static void OnBrowse(HWND hDlg)
{
    wchar_t fileName[MAX_PATH] = {0};
    OPENFILENAMEW ofn = {0};
    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = hDlg;
    ofn.lpstrFilter = L"All Files (*.*)\0*.*\0";
    ofn.nFilterIndex = 1;
    ofn.lpstrFile = fileName;
    ofn.nMaxFile = MAX_PATH;
    ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;
    if (GetOpenFileNameW(&ofn))
    {
        g_selectedFile = fileName;
        SetDlgItemTextW(hDlg, IDC_EDIT_FILE, g_selectedFile.c_str());
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Selected file: " + g_selectedFile);
    }
}

static void OnProtect(HWND hDlg)
{
    AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Protecting file...");
    
    if (g_selectedFile.empty())
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: No file selected");
        return;
    }

    // Get template ID and label ID from UI
    wchar_t templateIdBuf[512] = {0};
    wchar_t labelIdBuf[512] = {0};
    GetDlgItemTextW(hDlg, IDC_EDIT_TEMPLATE_ID, templateIdBuf, 512);
    GetDlgItemTextW(hDlg, IDC_EDIT_LABEL_ID, labelIdBuf, 512);
    
    g_templateId = templateIdBuf;
    g_labelId = labelIdBuf;

    try
    {
        // Read input file
        ifstream inputFile(g_selectedFile, ios::binary);
        if (!inputFile.is_open())
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: Cannot open input file");
            return;
        }

        // Get file size
        inputFile.seekg(0, ios::end);
        size_t fileSize = static_cast<size_t>(inputFile.tellg());
        inputFile.seekg(0, ios::beg);

        // Read file content
        vector<uint8_t> fileData(fileSize);
        inputFile.read(reinterpret_cast<char*>(fileData.data()), fileSize);
        inputFile.close();

        // Create output file path
        std::wstring outFile = g_selectedFile;
        size_t dotPos = outFile.find_last_of(L".");
        if (dotPos != std::wstring::npos)
        {
            outFile = outFile.substr(0, dotPos) + L".mip.pfile";
        }
        else
        {
            outFile += L".mip.pfile";
        }

        // Write protected file (simplified format - in production use MIP SDK FileHandler)
        ofstream outputFile(outFile, ios::binary);
        if (!outputFile.is_open())
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: Cannot create output file");
            return;
        }

        // Write MIP protection header
        const char* header = "MIP_PROTECTED_FILE\0";
        outputFile.write(header, strlen(header) + 1);

        // Write template ID if provided
        if (!g_templateId.empty())
        {
            size_t templateLen = g_templateId.length();
            outputFile.write(reinterpret_cast<const char*>(&templateLen), sizeof(templateLen));
            int utf8Len = WideCharToMultiByte(CP_UTF8, 0, g_templateId.c_str(), -1, nullptr, 0, nullptr, nullptr);
            vector<char> utf8Template(utf8Len);
            WideCharToMultiByte(CP_UTF8, 0, g_templateId.c_str(), -1, utf8Template.data(), utf8Len, nullptr, nullptr);
            outputFile.write(utf8Template.data(), utf8Len - 1);
        }
        else
        {
            size_t zero = 0;
            outputFile.write(reinterpret_cast<const char*>(&zero), sizeof(zero));
        }

        // Write label ID if provided
        if (!g_labelId.empty())
        {
            size_t labelLen = g_labelId.length();
            outputFile.write(reinterpret_cast<const char*>(&labelLen), sizeof(labelLen));
            int utf8Len = WideCharToMultiByte(CP_UTF8, 0, g_labelId.c_str(), -1, nullptr, 0, nullptr, nullptr);
            vector<char> utf8Label(utf8Len);
            WideCharToMultiByte(CP_UTF8, 0, g_labelId.c_str(), -1, utf8Label.data(), utf8Len, nullptr, nullptr);
            outputFile.write(utf8Label.data(), utf8Len - 1);
        }
        else
        {
            size_t zero = 0;
            outputFile.write(reinterpret_cast<const char*>(&zero), sizeof(zero));
        }

        // Write file data
        outputFile.write(reinterpret_cast<const char*>(fileData.data()), fileData.size());
        outputFile.close();

        std::wstringstream msg;
        msg << L"File protected successfully: " << outFile;
        if (!g_templateId.empty())
            msg << L"\r\n  Template ID: " << g_templateId;
        if (!g_labelId.empty())
            msg << L"\r\n  Label ID: " << g_labelId;
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), msg.str());
    }
    catch (const exception& e)
    {
        std::wstringstream msg;
        msg << L"Error protecting file: ";
        int len = MultiByteToWideChar(CP_ACP, 0, e.what(), -1, nullptr, 0);
        vector<wchar_t> wideMsg(len);
        MultiByteToWideChar(CP_ACP, 0, e.what(), -1, wideMsg.data(), len);
        msg << wideMsg.data();
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), msg.str());
    }
    catch (...)
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error protecting file: Unknown exception");
    }
}

static void OnUnprotect(HWND hDlg)
{
    AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Unprotecting file...");
    
    if (g_selectedFile.empty())
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: No file selected");
        return;
    }

    try
    {
        // Read protected file
        ifstream inputFile(g_selectedFile, ios::binary);
        if (!inputFile.is_open())
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: Cannot open input file");
            return;
        }

        // Read and verify header
        char header[20] = {0};
        inputFile.read(header, 19);
        if (strcmp(header, "MIP_PROTECTED_FILE") != 0)
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: Not a protected file");
            inputFile.close();
            return;
        }

        // Read template ID length
        size_t templateLen = 0;
        inputFile.read(reinterpret_cast<char*>(&templateLen), sizeof(templateLen));
        if (templateLen > 0)
        {
            vector<char> templateData(templateLen);
            inputFile.read(templateData.data(), templateLen);
        }

        // Read label ID length
        size_t labelLen = 0;
        inputFile.read(reinterpret_cast<char*>(&labelLen), sizeof(labelLen));
        if (labelLen > 0)
        {
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

        // Create output file path
        std::wstring outFile = g_selectedFile;
        size_t dotPos = outFile.find_last_of(L".");
        if (dotPos != std::wstring::npos)
        {
            outFile = outFile.substr(0, dotPos) + L".mip.unprot";
        }
        else
        {
            outFile += L".mip.unprot";
        }

        // Write unprotected file
        ofstream outputFile(outFile, ios::binary);
        if (!outputFile.is_open())
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: Cannot create output file");
            return;
        }

        outputFile.write(reinterpret_cast<const char*>(fileData.data()), fileData.size());
        outputFile.close();

        std::wstringstream msg;
        msg << L"File unprotected successfully: " << outFile;
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), msg.str());
    }
    catch (const exception& e)
    {
        std::wstringstream msg;
        msg << L"Error unprotected file: ";
        int len = MultiByteToWideChar(CP_ACP, 0, e.what(), -1, nullptr, 0);
        vector<wchar_t> wideMsg(len);
        MultiByteToWideChar(CP_ACP, 0, e.what(), -1, wideMsg.data(), len);
        msg << wideMsg.data();
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), msg.str());
    }
    catch (...)
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error unprotected file: Unknown exception");
    }
}

static void OnGetInfo(HWND hDlg)
{
    AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Getting protection information...");

    if (g_selectedFile.empty())
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: No file selected");
        return;
    }

    try
    {
        ifstream inputFile(g_selectedFile, ios::binary);
        if (!inputFile.is_open())
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: Cannot open file");
            return;
        }

        // Read and verify header
        char header[20] = {0};
        inputFile.read(header, 19);
        
        std::wstringstream info;
        info << L"=== Protection Information ===" << L"\r\n";
        
        if (strcmp(header, "MIP_PROTECTED_FILE") == 0)
        {
            info << L"Status: Protected\r\n";
            
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
                info << L"Template ID: " << wideTemplate.data() << L"\r\n";
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
                info << L"Label ID: " << wideLabel.data() << L"\r\n";
            }
        }
        else
        {
            info << L"Status: Unprotected\r\n";
        }
        
        inputFile.close();
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), info.str());
    }
    catch (const exception& e)
    {
        std::wstringstream msg;
        msg << L"Error getting file information: ";
        int len = MultiByteToWideChar(CP_ACP, 0, e.what(), -1, nullptr, 0);
        vector<wchar_t> wideMsg(len);
        MultiByteToWideChar(CP_ACP, 0, e.what(), -1, wideMsg.data(), len);
        msg << wideMsg.data();
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), msg.str());
    }
    catch (...)
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error getting file information: Unknown exception");
    }
}

static INT_PTR CALLBACK DlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_INITDIALOG:
        return TRUE;
    
    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDC_BTN_BROWSE:
            OnBrowse(hDlg);
            return TRUE;
        
        case IDC_BTN_PROTECT:
            OnProtect(hDlg);
            return TRUE;
        
        case IDC_BTN_UNPROTECT:
            OnUnprotect(hDlg);
            return TRUE;
        
        case IDC_BTN_GETINFO:
            OnGetInfo(hDlg);
            return TRUE;
        
        case IDOK:
        case IDCANCEL:
            EndDialog(hDlg, LOWORD(wParam));
            return TRUE;
        }
        break;
    
    case WM_CLOSE:
        EndDialog(hDlg, 0);
        return TRUE;
    }
    return FALSE;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
    return (int)DialogBoxW(hInstance, MAKEINTRESOURCEW(IDD_MAIN_DIALOG), NULL, DlgProc);
}
