#include <windows.h>
#include <commdlg.h>
#include <commctrl.h>
#include <string>
#include <vector>
#include <sstream>
#include <fstream>
#include "resource.h"

// MSIPC C API
#include <msipc.h>

static std::wstring g_selectedFile;
static bool g_useTemplate = true;
static std::wstring g_rightsSummary = L"No users configured";
struct UserRightsEntry { std::wstring user; std::vector<std::wstring> rights; };
static std::vector<UserRightsEntry> g_userRights;
static std::vector<std::wstring> g_templates;
static PCIPC_TIL g_templateList = nullptr;

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
    
    if (IsDlgButtonChecked(hDlg, IDC_RDO_TEMPLATE) == BST_CHECKED)
    {
        // Template picker dialog
        INT_PTR res = DialogBoxParamW(GetModuleHandleW(NULL), MAKEINTRESOURCEW(IDD_TEMPLATE_PICKER), hDlg, [](HWND dlg, UINT msg, WPARAM w, LPARAM l)->INT_PTR {
            switch (msg)
            {
            case WM_INITDIALOG:
            {
                // Populate templates using MSIPC API
                g_templates.clear();
                if (g_templateList) {
                    IpcFreeMemory((LPVOID)g_templateList);
                    g_templateList = nullptr;
                }
                
                HRESULT hr = IpcGetTemplateList(nullptr, IPC_GTL_FLAG_DEFAULT, 
                    GetUserDefaultLCID(), nullptr, nullptr, &g_templateList);
                
                if (SUCCEEDED(hr) && g_templateList) {
                    for (DWORD i = 0; i < g_templateList->cTi; i++) {
                        std::wstring templateName = g_templateList->aTi[i].wszName;
                        g_templates.push_back(templateName);
                        SendMessageW(GetDlgItem(dlg, IDC_LIST_TEMPLATES), LB_ADDSTRING, 0, (LPARAM)templateName.c_str());
                    }
                } else {
                    // Fallback to demo data if MSIPC fails
                    g_templates.push_back(L"Confidential Template (Demo)");
                    g_templates.push_back(L"Internal Template (Demo)");
                    g_templates.push_back(L"Public Template (Demo)");
                    
                    for (const auto& tpl : g_templates) {
                        SendMessageW(GetDlgItem(dlg, IDC_LIST_TEMPLATES), LB_ADDSTRING, 0, (LPARAM)tpl.c_str());
                    }
                }
                return TRUE;
            }
            case WM_COMMAND:
                if (LOWORD(w) == IDC_LIST_TEMPLATES && HIWORD(w) == LBN_SELCHANGE)
                {
                    int sel = (int)SendMessageW(GetDlgItem(dlg, IDC_LIST_TEMPLATES), LB_GETCURSEL, 0, 0);
                    if (sel >= 0 && sel < (int)g_templates.size())
                    {
                        std::wstring info;
                        if (g_templateList && sel < (int)g_templateList->cTi) {
                            const auto& tpl = g_templateList->aTi[sel];
                            info = L"Name: " + std::wstring(tpl.wszName) + L"\r\n";
                            info += L"Description: " + std::wstring(tpl.wszDescription) + L"\r\n";
                            info += L"Template ID: " + std::wstring(tpl.wszID) + L"\r\n";
                            info += L"Issuer: " + std::wstring(tpl.wszIssuerDisplayName);
                        } else {
                            const auto& tpl = g_templates[sel];
                            info = L"Name: " + tpl + L"\r\n";
                            info += L"Description: Demo template for " + tpl + L"\r\n";
                            info += L"Template ID: 0000-0000-0000-0000\r\n";
                            info += L"Issuer: Demo Issuer";
                        }
                        SetDlgItemTextW(dlg, IDC_EDIT_TPL_INFO, info.c_str());
                    }
                }
                else if (LOWORD(w) == IDOK) { EndDialog(dlg, IDOK); return TRUE; }
                else if (LOWORD(w) == IDCANCEL) { EndDialog(dlg, IDCANCEL); return TRUE; }
                break;
            }
            return FALSE;
        }, 0);
        if (res != IDOK)
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"No template selected. Protection cancelled.");
            return;
        }
        
        // Get selected template
        int sel = (int)SendMessageW(GetDlgItem(hDlg, IDC_LIST_TEMPLATES), LB_GETCURSEL, 0, 0);
        if (sel >= 0 && sel < (int)g_templates.size())
        {
            std::wstring templateName = g_templates[sel];
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Using template: " + templateName);

            // Resolve template ID if available from MSIPC
            LPCWSTR wszTemplateId = nullptr;
            if (g_templateList && sel < (int)g_templateList->cTi) {
                wszTemplateId = g_templateList->aTi[sel].wszID;
            }

            if (!wszTemplateId) {
                AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: No template ID available");
                return;
            }

            // Encrypt using file API with template ID
            LPCWSTR outPath = nullptr;
            HRESULT ehr = IpcfEncryptFile(
                g_selectedFile.c_str(),                           // input file
                (LPCVOID)wszTemplateId,                            // pvLicenseInfo -> template ID string
                IPCF_EF_TEMPLATE_ID,                               // dwType
                IPCF_EF_FLAG_DEFAULT,                              // flags
                nullptr,                                           // prompt ctx (suppress UI handled elsewhere)
                nullptr,                                           // output directory (same as input)
                &outPath                                           // result output path
            );

            if (SUCCEEDED(ehr)) {
                std::wstring outMsg = L"Protected file saved: ";
                if (outPath) { outMsg += outPath; IpcFreeMemory((LPVOID)outPath); }
                else { outMsg += (g_selectedFile + L".pfile"); }
                AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), outMsg);
            } else {
                AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Protection failed (IpcfEncryptFile)");
            }
        }
    }
    else
    {
        if (g_userRights.empty())
        {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: No custom user rights configured");
            return;
        }
        
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Mode: Custom rights -> " + g_rightsSummary);
        
        // Build from-scratch license via template issuer
        IPC_TEMPLATE_ISSUER issuer = { 0 };
        issuer.connectionInfo.wszExtranetUrl = nullptr;
        issuer.connectionInfo.wszIntranetUrl = nullptr;
        issuer.wszDisplayName = L"Custom";
        issuer.fAllowFromScratch = TRUE;

        IPC_LICENSE_HANDLE hLicense = nullptr;
        HRESULT lhr = IpcCreateLicenseFromScratch(&issuer, 0, nullptr, &hLicense);
        if (FAILED(lhr) || !hLicense) {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: IpcCreateLicenseFromScratch failed");
            return;
        }

        // Build user rights list
        const size_t userCount = g_userRights.size();
        std::vector<std::vector<LPCWSTR>> perUserRights;
        perUserRights.reserve(userCount);
        std::vector<IPC_USER_RIGHTS> rightsEntries;
        rightsEntries.reserve(userCount);

        for (const auto& ur : g_userRights) {
            IPC_USER_RIGHTS entry = {};
            if (_wcsicmp(ur.user.c_str(), L"ANYONE") == 0) {
                entry.User.dwType = IPC_USER_TYPE_IPC;
                entry.User.wszID = L"ANYONE";
            } else {
                entry.User.dwType = IPC_USER_TYPE_EMAIL;
                entry.User.wszID = ur.user.c_str();
            }

            perUserRights.emplace_back();
            auto &arr = perUserRights.back();
            for (const auto& r : ur.rights) {
                if (_wcsicmp(r.c_str(), L"VIEW") == 0) arr.push_back(IPC_GENERIC_READ);
                else if (_wcsicmp(r.c_str(), L"EDIT") == 0) arr.push_back(IPC_GENERIC_WRITE);
                else if (_wcsicmp(r.c_str(), L"PRINT") == 0) arr.push_back(IPC_GENERIC_PRINT);
                else if (_wcsicmp(r.c_str(), L"EXPORT") == 0) arr.push_back(IPC_GENERIC_EXPORT);
                else if (_wcsicmp(r.c_str(), L"EXTRACT") == 0) arr.push_back(IPC_GENERIC_EXTRACT);
                else if (_wcsicmp(r.c_str(), L"COMMENT") == 0) arr.push_back(IPC_GENERIC_COMMENT);
                else if (_wcsicmp(r.c_str(), L"FORWARD") == 0) arr.push_back(IPC_EMAIL_FORWARD);
            }
            entry.cRights = (DWORD)arr.size();
            entry.rgwszRights = arr.empty() ? nullptr : arr.data();
            rightsEntries.push_back(entry);
        }

        const size_t listBytes = sizeof(IPC_USER_RIGHTS_LIST) + (rightsEntries.size() ? (rightsEntries.size() - 1) * sizeof(IPC_USER_RIGHTS) : 0);
        std::vector<BYTE> listBuffer(listBytes);
        PIPC_USER_RIGHTS_LIST listPtr = reinterpret_cast<PIPC_USER_RIGHTS_LIST>(listBuffer.data());
        listPtr->cbSize = (DWORD)sizeof(IPC_USER_RIGHTS_LIST);
        listPtr->cUserRights = (DWORD)rightsEntries.size();
        for (size_t i = 0; i < rightsEntries.size(); ++i) {
            listPtr->rgUserRights[i] = rightsEntries[i];
        }

        HRESULT setHr = IpcSetLicenseProperty(hLicense, FALSE, IPC_LI_USER_RIGHTS_LIST, listPtr);
        if (FAILED(setHr)) {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: IpcSetLicenseProperty(IPC_LI_USER_RIGHTS_LIST) failed");
            return;
        }

        // Encrypt via file API using license handle
        LPCWSTR outPath = nullptr;
        HRESULT ehr = IpcfEncryptFile(
            g_selectedFile.c_str(),                   // input file
            (LPCVOID)hLicense,                        // pvLicenseInfo -> license handle
            IPCF_EF_LICENSE_HANDLE,                   // dwType
            IPCF_EF_FLAG_DEFAULT,                    // flags
            nullptr,                                  // prompt ctx
            nullptr,                                  // output dir
            &outPath                                  // result path
        );

        if (SUCCEEDED(ehr)) {
            std::wstring outMsg = L"Protected file saved: ";
            if (outPath) { outMsg += outPath; IpcFreeMemory((LPVOID)outPath); }
            else { outMsg += (g_selectedFile + L".pfile"); }
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), outMsg);
        } else {
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Protection failed (IpcfEncryptFile/custom)");
        }
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
    
    // Decrypt using file API
    LPCWSTR outPath = nullptr;
    HRESULT dhr = IpcfDecryptFile(
        g_selectedFile.c_str(),
        IPCF_DF_FLAG_DEFAULT,
        nullptr,
        nullptr,
        &outPath
    );

    if (SUCCEEDED(dhr)) {
        std::wstring outMsg = L"Unprotected file saved: ";
        if (outPath) { outMsg += outPath; IpcFreeMemory((LPVOID)outPath); }
        else {
            std::wstring outputPath = g_selectedFile;
            size_t pos = outputPath.rfind(L".pfile");
            if (pos != std::wstring::npos && pos == outputPath.length() - 6) {
                outputPath = outputPath.substr(0, outputPath.length() - 6) + L".unprotected";
            } else {
                outputPath += L".unprotected";
            }
            outMsg += outputPath;
        }
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), outMsg);
    } else {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Decryption failed (IpcfDecryptFile)");
    }
}

static void OnGetInfo(HWND hDlg)
{
    AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Getting protection info...");
    
    if (g_selectedFile.empty())
    {
        AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Error: No file selected");
        return;
    }
    
    // Get suggested decrypted path (as a proxy to confirm readability)
    LPCWSTR suggest = nullptr;
    HRESULT phr = IpcfGetDecryptedFilePath(g_selectedFile.c_str(), IPCF_DF_FLAG_DEFAULT, &suggest);
    if (SUCCEEDED(phr)) {
        if (suggest) { 
            AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Suggested decrypted path: " + std::wstring(suggest)); 
            IpcFreeMemory((LPVOID)suggest); 
        }
    }
    AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"Protection information retrieved.");
}

static void OnEditRights(HWND hDlg)
{
    // Show rights editor dialog
    INT_PTR res = DialogBoxParamW(GetModuleHandleW(NULL), MAKEINTRESOURCEW(IDD_RIGHTS_EDITOR), hDlg, [](HWND dlg, UINT msg, WPARAM w, LPARAM l)->INT_PTR {
        switch (msg)
        {
        case WM_INITDIALOG:
        {
            // Initialize list view columns
            HWND hList = GetDlgItem(dlg, IDC_LIST_USERS);
            LVCOLUMNW col = { 0 }; col.mask = LVCF_TEXT | LVCF_WIDTH; col.cx = 110; col.pszText = (LPWSTR)L"User"; ListView_InsertColumn(hList, 0, &col);
            col.cx = 120; col.pszText = (LPWSTR)L"Rights"; ListView_InsertColumn(hList, 1, &col);
            // Populate existing
            for (size_t i = 0; i < g_userRights.size(); ++i) {
                const auto &e = g_userRights[i];
                std::wstringstream rs; for (size_t j = 0; j < e.rights.size(); ++j) { if (j) rs << L","; rs << e.rights[j]; }
                LVITEMW it = { 0 }; it.mask = LVIF_TEXT; it.iItem = (int)i; it.pszText = (LPWSTR)e.user.c_str(); ListView_InsertItem(hList, &it);
                ListView_SetItemText(hList, (int)i, 1, (LPWSTR)rs.str().c_str());
            }
            return TRUE;
        }
        case WM_COMMAND:
            switch (LOWORD(w))
            {
            case IDC_BTN_ADD_USER:
            {
                wchar_t user[256] = {0}; GetDlgItemTextW(dlg, IDC_EDIT_USER, user, 255);
                if (!*user) { MessageBoxW(dlg, L"Enter a user or ANYONE", L"Validation", MB_OK | MB_ICONWARNING); break; }
                std::vector<std::wstring> rights;
                if (IsDlgButtonChecked(dlg, IDC_CHK_VIEW) == BST_CHECKED) rights.push_back(L"VIEW");
                if (IsDlgButtonChecked(dlg, IDC_CHK_EDIT) == BST_CHECKED) rights.push_back(L"EDIT");
                if (IsDlgButtonChecked(dlg, IDC_CHK_PRINT) == BST_CHECKED) rights.push_back(L"PRINT");
                if (IsDlgButtonChecked(dlg, IDC_CHK_EXPORT) == BST_CHECKED) rights.push_back(L"EXPORT");
                if (IsDlgButtonChecked(dlg, IDC_CHK_EXTRACT) == BST_CHECKED) rights.push_back(L"EXTRACT");
                if (IsDlgButtonChecked(dlg, IDC_CHK_COMMENT) == BST_CHECKED) rights.push_back(L"COMMENT");
                if (IsDlgButtonChecked(dlg, IDC_CHK_FORWARD) == BST_CHECKED) rights.push_back(L"FORWARD");
                if (rights.empty()) { MessageBoxW(dlg, L"Select at least one right", L"Validation", MB_OK | MB_ICONWARNING); break; }
                g_userRights.push_back({ user, rights });
                // Append to list
                HWND hList = GetDlgItem(dlg, IDC_LIST_USERS);
                LVITEMW it = { 0 }; it.mask = LVIF_TEXT; it.iItem = (int)g_userRights.size() - 1; it.pszText = user; ListView_InsertItem(hList, &it);
                std::wstringstream rs; for (size_t j = 0; j < rights.size(); ++j) { if (j) rs << L","; rs << rights[j]; }
                std::wstring rsStr = rs.str();
                ListView_SetItemText(hList, it.iItem, 1, (LPWSTR)rsStr.c_str());
                break;
            }
            case IDC_BTN_REMOVE_USER:
            {
                HWND hList = GetDlgItem(dlg, IDC_LIST_USERS);
                int sel = ListView_GetNextItem(hList, -1, LVNI_SELECTED);
                if (sel >= 0 && sel < (int)g_userRights.size()) {
                    g_userRights.erase(g_userRights.begin() + sel);
                    ListView_DeleteItem(hList, sel);
                }
                break;
            }
            case IDOK:
                EndDialog(dlg, IDOK); return TRUE;
            case IDCANCEL:
                EndDialog(dlg, IDCANCEL); return TRUE;
            }
            break;
        }
        return FALSE;
    }, 0);

    // Update summary
    if (res == IDOK) {
        if (g_userRights.empty()) g_rightsSummary = L"No users configured";
        else {
            std::wstringstream s; for (size_t i = 0; i < g_userRights.size(); ++i) {
                if (i) s << L"; ";
                s << g_userRights[i].user << L" (";
                for (size_t j = 0; j < g_userRights[i].rights.size(); ++j) { if (j) s << L","; s << g_userRights[i].rights[j]; }
                s << L")";
            }
            g_rightsSummary = s.str();
        }
        SetDlgItemTextW(hDlg, IDC_LBL_RIGHTS_SUM, g_rightsSummary.c_str());
    }
}

INT_PTR CALLBACK DlgProc(HWND hDlg, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_INITDIALOG:
        {
            InitCommonControls();
            int hr = IpcInitializeEnvironment();
            if (hr < 0) {
                AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"MSIPC initialization failed");
            } else {
                AppendText(GetDlgItem(hDlg, IDC_EDIT_LOG), L"MSIPC initialized");
            }
            CheckRadioButton(hDlg, IDC_RDO_TEMPLATE, IDC_RDO_CUSTOM, IDC_RDO_TEMPLATE);
            SetDlgItemTextW(hDlg, IDC_LBL_RIGHTS_SUM, g_rightsSummary.c_str());
        }
        return TRUE;
    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDC_BTN_BROWSE: OnBrowse(hDlg); return TRUE;
        case IDC_BTN_PROTECT: OnProtect(hDlg); return TRUE;
        case IDC_BTN_UNPROTECT: OnUnprotect(hDlg); return TRUE;
        case IDC_BTN_GETINFO: OnGetInfo(hDlg); return TRUE;
        case IDC_BTN_EDIT_RIGHTS: OnEditRights(hDlg); return TRUE;
        case IDCANCEL: EndDialog(hDlg, 0); return TRUE;
        }
        break;
    }
    return FALSE;
}

int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE, LPWSTR, int)
{
    return (int)DialogBoxParamW(hInstance, MAKEINTRESOURCEW(IDD_MAIN_DIALOG), NULL, DlgProc, 0);
}


