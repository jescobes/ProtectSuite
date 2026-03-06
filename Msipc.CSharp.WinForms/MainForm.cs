using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.InformationProtectionAndControl;
using Microsoft.Identity.Client;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Security.Principal;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace Msipc.CSharp.WinForms
{
    public partial class MainForm : Form
    {
        private Button btnSelectFile;
        private Button btnProtect;
        private Button btnUnprotect;
        private Button btnGetInfo;
        private Button btnViewContent;
        private RadioButton rbPurview;
        private RadioButton rbAdRms;
        private TextBox txtFilePath;
        private TextBox txtResult;
        private Label lblFile;
        private Label lblBackend;
        private Label lblActions;
        private Label lblResult;
        private GroupBox grpProtectionMode;
        private RadioButton rdoTemplate;
        private RadioButton rdoCustom;
        private GroupBox grpCustom;
        private Button btnEditCustomRights;
        private Label lblCustomRights;

        private string selectedFilePath;
        private BackendType selectedBackend = BackendType.Purview;
        private List<Microsoft.InformationProtectionAndControl.UserRights> customUserRights = new List<Microsoft.InformationProtectionAndControl.UserRights>();
        private string currentUserIdentity = null; // Store current user identity for automatic OWNER assignment

        public enum BackendType
        {
            Purview,
            AdRms
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeMSIPC();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "MSIPC C# - File Protection";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 600);

            // File selection
            this.lblFile = new Label();
            this.lblFile.Text = "Select File:";
            this.lblFile.Location = new Point(20, 20);
            this.lblFile.Size = new Size(80, 20);

            this.txtFilePath = new TextBox();
            this.txtFilePath.Location = new Point(20, 45);
            this.txtFilePath.Size = new Size(400, 20);
            this.txtFilePath.ReadOnly = true;

            this.btnSelectFile = new Button();
            this.btnSelectFile.Text = "Browse...";
            this.btnSelectFile.Location = new Point(430, 43);
            this.btnSelectFile.Size = new Size(80, 25);
            this.btnSelectFile.Click += BtnSelectFile_Click;

            // Backend selection
            this.lblBackend = new Label();
            this.lblBackend.Text = "Backend:";
            this.lblBackend.Location = new Point(20, 80);
            this.lblBackend.Size = new Size(80, 20);

            this.rbPurview = new RadioButton();
            this.rbPurview.Text = "Microsoft Purview";
            this.rbPurview.Location = new Point(20, 105);
            this.rbPurview.Size = new Size(150, 20);
            this.rbPurview.Checked = true;
            // Don't attach event handler yet - will attach after both radio buttons are created
            // to avoid duplicate events during initialization

            this.rbAdRms = new RadioButton();
            this.rbAdRms.Text = "AD RMS Server";
            this.rbAdRms.Location = new Point(180, 105);
            this.rbAdRms.Size = new Size(150, 20);
            
            // Now attach event handlers after both radio buttons are initialized
            // This prevents duplicate events when setting Checked = true
            this.rbPurview.CheckedChanged += RbBackend_CheckedChanged;
            this.rbAdRms.CheckedChanged += RbBackend_CheckedChanged;

            // Protection mode
            this.grpProtectionMode = new GroupBox();
            this.grpProtectionMode.Text = "Protection Mode";
            this.grpProtectionMode.Location = new Point(20, 135);
            this.grpProtectionMode.Size = new Size(540, 65);

            this.rdoTemplate = new RadioButton();
            this.rdoTemplate.Text = "Template based";
            this.rdoTemplate.Location = new Point(15, 28);
            this.rdoTemplate.Size = new Size(130, 20);
            this.rdoTemplate.Checked = true;

            this.rdoCustom = new RadioButton();
            this.rdoCustom.Text = "Custom rights";
            this.rdoCustom.Location = new Point(160, 28);
            this.rdoCustom.Size = new Size(120, 20);

            this.grpProtectionMode.Controls.Add(this.rdoTemplate);
            this.grpProtectionMode.Controls.Add(this.rdoCustom);

            // Custom rights panel
            this.grpCustom = new GroupBox();
            this.grpCustom.Text = "Custom Rights";
            this.grpCustom.Location = new Point(20, 205);
            this.grpCustom.Size = new Size(540, 70);

            this.lblCustomRights = new Label();
            this.lblCustomRights.Text = "No users configured";
            this.lblCustomRights.Location = new Point(12, 25);
            this.lblCustomRights.Size = new Size(400, 20);

            this.btnEditCustomRights = new Button();
            this.btnEditCustomRights.Text = "Edit User Rights...";
            this.btnEditCustomRights.Location = new Point(12, 45);
            this.btnEditCustomRights.Size = new Size(120, 25);
            this.btnEditCustomRights.Click += BtnEditCustomRights_Click;

            this.grpCustom.Controls.Add(this.lblCustomRights);
            this.grpCustom.Controls.Add(this.btnEditCustomRights);

            // Actions
            this.lblActions = new Label();
            this.lblActions.Text = "Actions:";
            this.lblActions.Location = new Point(20, 285);
            this.lblActions.Size = new Size(80, 20);

            this.btnProtect = new Button();
            this.btnProtect.Text = "Protect File";
            this.btnProtect.Location = new Point(20, 310);
            this.btnProtect.Size = new Size(100, 30);
            this.btnProtect.Enabled = false; // Disabled until file is selected
            this.btnProtect.Click += BtnProtect_Click;

            this.btnUnprotect = new Button();
            this.btnUnprotect.Text = "Unprotect File";
            this.btnUnprotect.Location = new Point(130, 310);
            this.btnUnprotect.Size = new Size(100, 30);
            this.btnUnprotect.Enabled = false; // Disabled until protected file is selected
            this.btnUnprotect.Click += BtnUnprotect_Click;

            this.btnGetInfo = new Button();
            this.btnGetInfo.Text = "Get Protection Info";
            this.btnGetInfo.Location = new Point(240, 310);
            this.btnGetInfo.Size = new Size(120, 30);
            this.btnGetInfo.Enabled = false; // Disabled until protected file is selected
            this.btnGetInfo.Click += BtnGetInfo_Click;

            this.btnViewContent = new Button();
            this.btnViewContent.Text = "View Content";
            this.btnViewContent.Location = new Point(370, 310);
            this.btnViewContent.Size = new Size(100, 30);
            this.btnViewContent.Enabled = false; // Disabled until file is selected
            this.btnViewContent.Click += BtnViewContent_Click;

            // Result area
            this.lblResult = new Label();
            this.lblResult.Text = "Result:";
            this.lblResult.Location = new Point(20, 350);
            this.lblResult.Size = new Size(80, 20);

            this.txtResult = new TextBox();
            this.txtResult.Location = new Point(20, 375);
            this.txtResult.Size = new Size(540, 180);
            this.txtResult.Multiline = true;
            this.txtResult.ScrollBars = ScrollBars.Vertical;
            this.txtResult.ReadOnly = true;

            // Add controls to form
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.lblBackend);
            this.Controls.Add(this.rbPurview);
            this.Controls.Add(this.rbAdRms);
            this.Controls.Add(this.grpProtectionMode);
            this.Controls.Add(this.grpCustom);
            this.Controls.Add(this.lblActions);
            this.Controls.Add(this.btnProtect);
            this.Controls.Add(this.btnUnprotect);
            this.Controls.Add(this.btnGetInfo);
            this.Controls.Add(this.btnViewContent);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.txtResult);

            this.ResumeLayout(false);
        }

        private void InitializeMSIPC()
        {
            try
            {
                SafeNativeMethods.IpcInitialize();
                AppendResult("MSIPC initialized successfully.");
                
                // Don't try to get user identity here - wait until backend is selected
                // GetMsipcUserIdentity() will be called when backend is selected (AD RMS) or on first operation
            }
            catch (Exception ex)
            {
                AppendResult($"Error initializing MSIPC: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the MSIPC authenticated user identity using existing GIC/CLC certificates (if available)
        /// Does not require authentication if certificates are already present
        /// </summary>
        private void GetMsipcUserIdentity()
        {
            // Only attempt to get identity if AD RMS backend is selected
            // For Purview, identity will be retrieved during first protect/unprotect operation
            if (selectedBackend != BackendType.AdRms)
            {
                // Don't attempt to get identity for Purview - it will be retrieved during operations
                return;
            }

            // Declare certificatesExist outside try block so it's available in catch
            bool certificatesExist = false;
            
            try
            {
                AppendResult("");
                AppendResult("MSIPC User Identity:");

                ConnectionInfo connectionInfo = GetAdrmsConnectionInfo();
                if (connectionInfo == null)
                {
                    AppendResult("  AD RMS connection info not configured.");
                    AppendResult("  Cannot retrieve MSIPC identity without connection info.");
                    AppendResult("  Identity will be available after first protect/unprotect operation.");
                    return;
                }

                // Try to get user identity - first attempt offline, then online if needed
                // Even if CheckMsipcCertificatesExist fails, we should try to get identity
                // because certificates might exist but verification might fail for various reasons
                if (connectionInfo != null)
                {
                    certificatesExist = CheckMsipcCertificatesExist(connectionInfo);
                }

                // Try to get user identity regardless of certificate check result
                // The certificate check might fail even if certificates exist (e.g., URL mismatch)
                // First try to get templates, if available use them. Otherwise try creating license from scratch.
                try
                {
                    var templates = SafeNativeMethods.IpcGetTemplateList(
                        connectionInfo,
                        forceDownload: false,
                        suppressUI: true,
                        offline: true,  // Use offline mode - certificates already exist
                        hasUserConsent: false,
                        parentWindow: (Form)null,
                        cultureInfo: null);

                    SafeInformationProtectionKeyHandle keyHandle = null;
                    try
                    {
                        if (templates != null && templates.Count > 0)
                        {
                            // Method 1: Create a temporary license from the first template to get user identity
                            try
                            {
                                string templateId = templates[0].TemplateId;
                                using (var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId))
                                {
                                    // Serialize license in offline mode to get the key (no authentication needed)
                                    byte[] licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                                        licenseHandle,
                                        SerializeLicenseFlags.KeyNoPersist,
                                        suppressUI: true,
                                        offline: true,  // Offline - use existing certificates
                                        hasUserConsent: false,
                                        parentWindow: (Form)null,
                                        out keyHandle);

                                    if (keyHandle != null && !keyHandle.IsInvalid)
                                    {
                                        // Get user display name from the key (this is typically the email address)
                                        string userDisplayName = SafeNativeMethods.IpcGetKeyUserDisplayName(keyHandle);
                                        if (!string.IsNullOrEmpty(userDisplayName))
                                        {
                                            AppendResult($"  MSIPC User: {userDisplayName}");
                                            // Store current user identity for automatic OWNER assignment
                                            currentUserIdentity = userDisplayName;
                                            
                                            // Get server information from connection info
                                            string serverInfo = "";
                                            if (connectionInfo != null)
                                            {
                                                if (connectionInfo.ExtranetUrl != null)
                                                    serverInfo = connectionInfo.ExtranetUrl.Host;
                                                else if (connectionInfo.IntranetUrl != null)
                                                    serverInfo = connectionInfo.IntranetUrl.Host;
                                            }
                                            
                                            // Also try to get server info from license
                                            try
                                            {
                                                var licenseConnInfo = SafeNativeMethods.IpcGetSerializedLicenseConnectionInfo(licenseBytes, keyHandle);
                                                if (licenseConnInfo != null)
                                                {
                                                    if (licenseConnInfo.ExtranetUrl != null)
                                                        serverInfo = licenseConnInfo.ExtranetUrl.Host;
                                                    else if (licenseConnInfo.IntranetUrl != null)
                                                        serverInfo = licenseConnInfo.IntranetUrl.Host;
                                                }
                                            }
                                            catch { }
                                            
                                            if (!string.IsNullOrEmpty(serverInfo))
                                                AppendResult($"  Server: {serverInfo}");
                                            
                                            AppendResult("  (Using existing GIC/CLC certificates - no authentication required)");
                                            return; // Success, exit early
                                        }
                                    }
                                }
                            }
                        catch
                        {
                            // Could not get identity from template, try from scratch
                            keyHandle?.Dispose();
                            keyHandle = null;
                        }
                        }

                        // Method 2: If no templates or template method failed, try creating license from scratch
                        // This works if the server allows "from scratch" licenses
                        try
                        {
                            // Get template issuers to find one that allows from-scratch licenses
                            var issuers = SafeNativeMethods.IpcGetTemplateIssuerList(
                                connectionInfo,
                                defaultServerOnly: false,
                                suppressUI: true,
                                offline: true,
                                hasUserConsent: false,
                                parentWindow: (Form)null);

                            if (issuers != null && issuers.Count > 0)
                            {
                                // Find an issuer that allows from-scratch licenses
                                var issuer = issuers.FirstOrDefault(i => i.AllowFromScratch);
                                if (issuer != null)
                                {
                                    // Create license from scratch using the TemplateIssuer
                                    using (var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromScratch(issuer))
                                    {
                                        // Serialize license in offline mode to get the key
                                        byte[] licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                                            licenseHandle,
                                            SerializeLicenseFlags.KeyNoPersist,
                                            suppressUI: true,
                                            offline: true,
                                            hasUserConsent: false,
                                            parentWindow: (Form)null,
                                            out keyHandle);

                                        if (keyHandle != null && !keyHandle.IsInvalid)
                                        {
                                            // Get user display name from the key (this is typically the email address)
                                            string userDisplayName = SafeNativeMethods.IpcGetKeyUserDisplayName(keyHandle);
                                            if (!string.IsNullOrEmpty(userDisplayName))
                                            {
                                                AppendResult($"  MSIPC User: {userDisplayName}");
                                                // Store current user identity for automatic OWNER assignment
                                                currentUserIdentity = userDisplayName;
                                                
                                                // Get server information
                                                string serverInfo = "";
                                                if (connectionInfo != null)
                                                {
                                                    if (connectionInfo.ExtranetUrl != null)
                                                        serverInfo = connectionInfo.ExtranetUrl.Host;
                                                    else if (connectionInfo.IntranetUrl != null)
                                                        serverInfo = connectionInfo.IntranetUrl.Host;
                                                }
                                                
                                                if (!string.IsNullOrEmpty(serverInfo))
                                                    AppendResult($"  Server: {serverInfo}");
                                                
                                                AppendResult("  (Using existing GIC/CLC certificates - no authentication required)");
                                                return; // Success, exit early
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // From-scratch method also failed - this is expected if server doesn't allow it
                            // No need to log error, just fall through
                        }
                    }
                    finally
                    {
                        keyHandle?.Dispose();
                    }

                    // If we get here, neither method worked
                    if (templates == null || templates.Count == 0)
                    {
                        if (certificatesExist)
                        {
                            AppendResult("  Certificates (GIC/CLC) exist but no templates are cached.");
                            AppendResult("  User identity will be retrieved when templates are downloaded (first protect/unprotect operation).");
                        }
                        else
                        {
                            AppendResult("  No certificates found. Authentication will be required on first operation.");
                        }
                    }
                    else
                    {
                        AppendResult("  Could not extract user identity from templates.");
                        AppendResult("  User identity will be available after first protect/unprotect operation.");
                    }
                }
                catch (InformationProtectionException ex)
                {
                    // Offline operation failed - try online mode as fallback
                    AppendResult($"  Offline mode failed (HRESULT: 0x{ex.ErrorCode:X8}). Trying online mode...");
                    try
                    {
                        // Try online mode - this might work if certificates exist but need refresh or URL mismatch
                        // Note: With suppressUI=true, online mode may fail if authentication is required
                        var templates = SafeNativeMethods.IpcGetTemplateList(
                            connectionInfo,
                            forceDownload: false,
                            suppressUI: true,      // suppressUI=true prevents authentication prompts
                            offline: false,         // Try online mode
                            hasUserConsent: false,
                            parentWindow: (Form)null,
                            cultureInfo: null);

                        if (templates != null && templates.Count > 0)
                        {
                            AppendResult($"  Online mode: Retrieved {templates.Count} template(s). Attempting to extract identity...");
                            // Try to get identity from template in online mode
                            try
                            {
                                string templateId = templates[0].TemplateId;
                                using (var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId))
                                {
                                    SafeInformationProtectionKeyHandle keyHandle = null;
                                    try
                                    {
                                        byte[] licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                                            licenseHandle,
                                            SerializeLicenseFlags.KeyNoPersist,
                                            suppressUI: true,
                                            offline: false,
                                            hasUserConsent: false,
                                            parentWindow: (Form)null,
                                            out keyHandle);

                                        if (keyHandle != null && !keyHandle.IsInvalid)
                                        {
                                            string userDisplayName = SafeNativeMethods.IpcGetKeyUserDisplayName(keyHandle);
                                            if (!string.IsNullOrEmpty(userDisplayName))
                                            {
                                                AppendResult($"  MSIPC User: {userDisplayName}");
                                                // Store current user identity for automatic OWNER assignment
                                                currentUserIdentity = userDisplayName;
                                                
                                                // Get server information
                                                string serverInfo = "";
                                                if (connectionInfo != null)
                                                {
                                                    if (connectionInfo.ExtranetUrl != null)
                                                        serverInfo = connectionInfo.ExtranetUrl.Host;
                                                    else if (connectionInfo.IntranetUrl != null)
                                                        serverInfo = connectionInfo.IntranetUrl.Host;
                                                }
                                                
                                                if (!string.IsNullOrEmpty(serverInfo))
                                                    AppendResult($"  Server: {serverInfo}");
                                                
                                                AppendResult("  (Retrieved via online mode)");
                                                return; // Success
                                            }
                                            else
                                            {
                                                AppendResult("  Warning: Key handle obtained but user display name is empty.");
                                            }
                                        }
                                        else
                                        {
                                            AppendResult("  Warning: License serialization did not return a valid key handle.");
                                        }
                                    }
                                    catch (InformationProtectionException ex2)
                                    {
                                        AppendResult($"  Error serializing license in online mode: HRESULT 0x{ex2.ErrorCode:X8}");
                                        AppendResult("    This may indicate authentication is required (suppressUI=true prevents prompts).");
                                    }
                                    catch (Exception ex2)
                                    {
                                        AppendResult($"  Error serializing license in online mode: {ex2.Message}");
                                    }
                                    finally
                                    {
                                        keyHandle?.Dispose();
                                    }
                                }
                            }
                            catch (InformationProtectionException ex2)
                            {
                                AppendResult($"  Error creating license from template in online mode: HRESULT 0x{ex2.ErrorCode:X8}");
                            }
                            catch (Exception ex2)
                            {
                                AppendResult($"  Error creating license from template in online mode: {ex2.Message}");
                            }
                        }
                        else
                        {
                            AppendResult("  Online mode: IpcGetTemplateList succeeded but returned no templates.");
                            AppendResult("    Server is reachable but no templates are configured.");
                            AppendResult("    Attempting to get identity using 'from scratch' method with existing certificates...");
                            
                            // Method 2: Try creating license from scratch using existing certificates
                            // This should work if certificates (GIC/CLC) exist for this server
                            try
                            {
                                // Get template issuers to find one that allows from-scratch licenses
                                var issuers = SafeNativeMethods.IpcGetTemplateIssuerList(
                                    connectionInfo,
                                    defaultServerOnly: false,
                                    suppressUI: true,
                                    offline: false,  // Online mode, but using existing certificates
                                    hasUserConsent: false,
                                    parentWindow: (Form)null);

                                if (issuers != null && issuers.Count > 0)
                                {
                                    // Find an issuer that allows from-scratch licenses
                                    var issuer = issuers.FirstOrDefault(i => i.AllowFromScratch);
                                    if (issuer != null)
                                    {
                                        AppendResult($"    Found template issuer that allows from-scratch licenses: {issuer.DisplayName}");
                                        // Create license from scratch using the TemplateIssuer
                                        using (var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromScratch(issuer))
                                        {
                                            // Set basic user rights to allow serialization (required for from-scratch licenses)
                                            // We'll use VIEW right for ANYONE - this is just to make the license valid for serialization
                                            // The actual user identity will come from the certificates when we serialize
                                            try
                                            {
                                                var basicRights = new System.Collections.ObjectModel.Collection<string>();
                                                basicRights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ViewRight);
                                                
                                                var userRightsList = new System.Collections.ObjectModel.Collection<Microsoft.InformationProtectionAndControl.UserRights>();
                                                userRightsList.Add(new Microsoft.InformationProtectionAndControl.UserRights(
                                                    Microsoft.InformationProtectionAndControl.UserIdType.IpcUser,
                                                    "ANYONE",
                                                    basicRights));
                                                
                                                SafeNativeMethods.IpcSetLicenseUserRightsList(licenseHandle, userRightsList);
                                                AppendResult("    Configured basic VIEW right for ANYONE (required for license serialization)");
                                            }
                                            catch (Exception exRights)
                                            {
                                                AppendResult($"    Warning: Could not set user rights: {exRights.Message}");
                                                // Continue anyway - might work without it in some cases
                                            }
                                            
                                            SafeInformationProtectionKeyHandle keyHandle = null;
                                            bool success = false;
                                            
                                            try
                                            {
                                                // First try with suppressUI=true (silent mode, use existing certificates)
                                                try
                                                {
                                                    AppendResult("    Attempting to serialize license (silent mode, using existing certificates)...");
                                                    byte[] licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                                                        licenseHandle,
                                                        SerializeLicenseFlags.KeyNoPersist,
                                                        suppressUI: true,
                                                        offline: false,  // Online mode - may need to refresh certificates
                                                        hasUserConsent: false,
                                                        parentWindow: (Form)null,
                                                        out keyHandle);
                                                    
                                                    if (keyHandle != null && !keyHandle.IsInvalid)
                                                    {
                                                        success = true;
                                                    }
                                                }
                                                catch (InformationProtectionException ex3)
                                                {
                                                    // If silent mode fails with 0x80040200 or similar, try with UI prompts enabled
                                                    uint errorCode = unchecked((uint)ex3.ErrorCode);
                                                    uint msipcErrorMask = 0x80040000;
                                                    
                                                    if ((errorCode & msipcErrorMask) == msipcErrorMask || errorCode == 0x80070005)
                                                    {
                                                        AppendResult($"    Silent mode failed (HRESULT: 0x{ex3.ErrorCode:X8}). Trying with UI prompts enabled...");
                                                        keyHandle?.Dispose();
                                                        keyHandle = null;
                                                        
                                                        // Retry with suppressUI=false and hasUserConsent=true to allow authentication
                                                        try
                                                        {
                                                            byte[] licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                                                                licenseHandle,
                                                                SerializeLicenseFlags.KeyNoPersist,
                                                                suppressUI: false,  // Allow UI prompts for authentication
                                                                offline: false,      // Online mode - may need to authenticate/refresh certificates
                                                                hasUserConsent: true, // User has consented to authentication
                                                                parentWindow: this,   // Use this form as parent for dialogs
                                                                out keyHandle);
                                                            
                                                            if (keyHandle != null && !keyHandle.IsInvalid)
                                                            {
                                                                success = true;
                                                            }
                                                        }
                                                        catch (InformationProtectionException)
                                                        {
                                                            // This will be handled by the outer catch block
                                                            throw;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Re-throw if it's not a certificate/auth error
                                                        throw;
                                                    }
                                                }
                                                
                                                if (success)
                                                {
                                                    try
                                                    {
                                                        // Get user display name from the key (this is typically the email address)
                                                        string userDisplayName = SafeNativeMethods.IpcGetKeyUserDisplayName(keyHandle);
                                                        if (!string.IsNullOrEmpty(userDisplayName))
                                                        {
                                                            AppendResult($"  MSIPC User: {userDisplayName}");
                                                            // Store current user identity for automatic OWNER assignment
                                                            currentUserIdentity = userDisplayName;
                                                            
                                                            // Get server information
                                                            string serverInfo = "";
                                                            if (connectionInfo != null)
                                                            {
                                                                if (connectionInfo.ExtranetUrl != null)
                                                                    serverInfo = connectionInfo.ExtranetUrl.Host;
                                                                else if (connectionInfo.IntranetUrl != null)
                                                                    serverInfo = connectionInfo.IntranetUrl.Host;
                                                            }
                                                            
                                                            // Also try to get server info from license
                                                            try
                                                            {
                                                                byte[] licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                                                                    licenseHandle,
                                                                    SerializeLicenseFlags.KeyNoPersist,
                                                                    suppressUI: false,
                                                                    offline: false,
                                                                    hasUserConsent: true,
                                                                    parentWindow: this,
                                                                    out var tempKeyHandle);
                                                                tempKeyHandle?.Dispose();
                                                                
                                                                var licenseConnInfo = SafeNativeMethods.IpcGetSerializedLicenseConnectionInfo(licenseBytes, keyHandle);
                                                                if (licenseConnInfo != null)
                                                                {
                                                                    if (licenseConnInfo.ExtranetUrl != null)
                                                                        serverInfo = licenseConnInfo.ExtranetUrl.Host;
                                                                    else if (licenseConnInfo.IntranetUrl != null)
                                                                        serverInfo = licenseConnInfo.IntranetUrl.Host;
                                                                }
                                                            }
                                                            catch { }
                                                            
                                                            if (!string.IsNullOrEmpty(serverInfo))
                                                                AppendResult($"  Server: {serverInfo}");
                                                            
                                                            AppendResult("  (Using existing or refreshed GIC/CLC certificates)");
                                                            return; // Success, exit early
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        keyHandle?.Dispose();
                                                    }
                                                }
                                                else
                                                {
                                                    keyHandle?.Dispose();
                                                    AppendResult("    Failed to serialize license: No valid key handle returned.");
                                                }
                                            }
                                            catch (InformationProtectionException ex3)
                                            {
                                                // Handle errors from serialization attempts
                                                AppendResult($"    Error serializing license from scratch: HRESULT 0x{ex3.ErrorCode:X8}");
                                                
                                                // Check for specific error codes
                                                uint errorCode = unchecked((uint)ex3.ErrorCode);
                                                uint needsOnlineError = 0x8004020D;
                                                uint accessDeniedError = 0x80070005;
                                                uint fileNotFoundError = 0x80070002;
                                                
                                                if (errorCode == needsOnlineError)
                                                {
                                                    AppendResult("      Error: IPCERROR_NEEDS_ONLINE");
                                                    AppendResult("      This is unexpected in online mode. Possible causes:");
                                                    AppendResult("        - Network connectivity issue");
                                                    AppendResult("        - Server unreachable");
                                                    AppendResult("        - Certificates expired and refresh failed");
                                                }
                                                else if (errorCode == accessDeniedError)
                                                {
                                                    AppendResult("      Error: E_ACCESSDENIED");
                                                    AppendResult("      Authentication required but suppressUI=true prevents prompts.");
                                                    AppendResult("      Solution: Perform a protect/unprotect operation to authenticate.");
                                                }
                                                else if (errorCode == fileNotFoundError)
                                                {
                                                    AppendResult("      Error: E_FILE_NOT_FOUND");
                                                    AppendResult("      Certificates not found for this server.");
                                                    AppendResult("      Solution: Perform a protect/unprotect operation to cache certificates.");
                                                }
                                                else if ((errorCode & 0x80040000) == 0x80040000)
                                                {
                                                    // MSIPC-specific error (0x8004xxxx)
                                                    AppendResult($"      MSIPC-specific error (0x{errorCode:X8})");
                                                    AppendResult("      Possible causes:");
                                                    AppendResult("        - Certificates don't match this server URL");
                                                    AppendResult("        - Certificates expired or invalid");
                                                    AppendResult("        - Server configuration mismatch");
                                                    AppendResult("        - Authentication required (suppressUI=true prevents prompts)");
                                                    AppendResult("      Solution: Verify server URL matches certificate server, or perform protect/unprotect to authenticate.");
                                                }
                                                else
                                                {
                                                    AppendResult("      Possible causes:");
                                                    AppendResult("        - Certificates don't match this server URL");
                                                    AppendResult("        - Authentication required (suppressUI=true prevents prompts)");
                                                    AppendResult("        - Server configuration issue");
                                                }
                                                
                                                // Include the exception message if available (may contain MSIPC error description)
                                                if (!string.IsNullOrEmpty(ex3.Message) && ex3.Message.Contains("HRESULT"))
                                                    AppendResult($"      MSIPC error description: {ex3.Message}");
                                            }
                                            catch (Exception ex3)
                                            {
                                                AppendResult($"    Error serializing license from scratch: {ex3.Message}");
                                            }
                                            finally
                                            {
                                                keyHandle?.Dispose();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        AppendResult("    No template issuer found that allows from-scratch licenses.");
                                    }
                                }
                                else
                                {
                                    AppendResult("    IpcGetTemplateIssuerList returned no issuers.");
                                }
                            }
                            catch (InformationProtectionException ex3)
                            {
                                AppendResult($"    Error getting template issuers: HRESULT 0x{ex3.ErrorCode:X8}");
                                AppendResult("      This may indicate authentication is required (suppressUI=true prevents prompts).");
                            }
                            catch (Exception ex3)
                            {
                                AppendResult($"    Error getting template issuers: {ex3.Message}");
                            }
                        }
                    }
                    catch (InformationProtectionException ex2)
                    {
                        // Online mode also failed
                        AppendResult($"  Online mode failed: HRESULT 0x{ex2.ErrorCode:X8}");
                        uint needsOnlineError = 0x8004020D;
                        uint accessDeniedError = 0x80070005;
                        if (ex2.ErrorCode == unchecked((int)needsOnlineError))
                        {
                            AppendResult("    Error: IPCERROR_NEEDS_ONLINE - This is unexpected in online mode.");
                            AppendResult("    Possible cause: Network connectivity issue or server unreachable.");
                        }
                        else if (ex2.ErrorCode == unchecked((int)accessDeniedError))
                        {
                            AppendResult("    Error: E_ACCESSDENIED - Authentication required but suppressUI=true prevents prompts.");
                            AppendResult("    Solution: Set suppressUI=false or perform protect/unprotect operation to authenticate.");
                        }
                    }
                    catch (Exception ex2)
                    {
                        AppendResult($"  Online mode failed: {ex2.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendResult($"  ERROR: Could not retrieve MSIPC user identity: {ex.Message}");
                if (certificatesExist)
                {
                    AppendResult("  Certificates exist but an error occurred.");
                    AppendResult("  User identity will be available after first protect/unprotect operation.");
                }
                else
                {
                    AppendResult("  Authentication will be required on first operation.");
                }
            }
        }

        /// <summary>
        /// Checks if MSIPC certificates (GIC and CLC) exist using MSIPC API function IpcGetTemplateList.
        /// This is the proper way to check for certificates - MSIPC will use cached certificates if available.
        /// Uses IpcGetTemplateList with offline=true to check if cached certificates exist.
        /// </summary>
        /// <param name="connectionInfo">Connection info to determine certificate location</param>
        /// <returns>True if certificates exist (can work offline), false otherwise</returns>
        private bool CheckMsipcCertificatesExist(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                // Without connection info, we can't check certificates via API
                return false;
            }

            AppendResult("Checking for MSIPC certificates (GIC/CLC) using IpcGetTemplateList...");
            AppendResult($"  Connection Info - Extranet: {(connectionInfo.ExtranetUrl != null ? connectionInfo.ExtranetUrl.ToString() : "null")}");
            AppendResult($"  Connection Info - Intranet: {(connectionInfo.IntranetUrl != null ? connectionInfo.IntranetUrl.ToString() : "null")}");
            AppendResult($"  Note: If certificates exist from a previous connection, SDK may use those URLs instead.");
            
            try
            {
                // Use IpcGetTemplateList with offline=true to check if cached certificates exist
                // This is the proper MSIPC API way to check for certificates
                // If certificates exist for this server, MSIPC will use them without requiring authentication
                // Note: Certificates are server-specific, so they must match the ConnectionInfo
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    connectionInfo,
                    forceDownload: false,  // Don't force download, use cache if available
                    suppressUI: true,      // Suppress UI to avoid authentication prompts
                    offline: true,         // Work offline - if this succeeds, certificates exist
                    hasUserConsent: false,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                // If we got templates (even empty list) in offline mode, certificates exist
                if (templates != null)
                {
                    AppendResult($"MSIPC certificates found via IpcGetTemplateList (offline mode works). {templates.Count} template(s) available.");
                    AppendResult("  Certificates are valid and match the configured server.");
                    return true;
                }
                else
                {
                    AppendResult("IpcGetTemplateList returned null in offline mode.");
                    AppendResult("  This may mean certificates don't exist for this server, or they need to be refreshed.");
                    return false;
                }
            }
            catch (InformationProtectionException ex)
            {
                // If offline operation fails, certificates likely don't exist or don't match this connection
                // Common error codes:
                // - 0x8004020D (IPCERROR_NEEDS_ONLINE) - Operation requires network access but offline mode was requested
                //   This means: No certificates cached for this server, or certificates don't match the server URL
                // - 0x80070005 (E_ACCESSDENIED) - No certificates or authentication required
                // - 0x80070002 (E_FILE_NOT_FOUND) - Certificates not found for this server
                // - 0x8004xxxx - MSIPC specific errors indicating certificate issues
                
                uint needsOnlineError = 0x8004020D;
                if (ex.ErrorCode == unchecked((int)needsOnlineError))
                {
                    AppendResult($"IpcGetTemplateList failed in offline mode (HRESULT: 0x{ex.ErrorCode:X8} = IPCERROR_NEEDS_ONLINE).");
                    AppendResult("  This error means: Operation requires network access but offline mode was requested.");
                    AppendResult("  Possible reasons:");
                    AppendResult("    - No GIC/CLC certificates cached for this server URL");
                    AppendResult("    - Certificates exist but are for a different server URL");
                    AppendResult("    - Server URL in config changed (certificates are server-specific)");
                    AppendResult("    - Certificates expired and need to be refreshed (requires online access)");
                    AppendResult("  Solution: Try online mode or perform a protect/unprotect operation to cache certificates.");
                }
                else
                {
                    AppendResult($"IpcGetTemplateList failed in offline mode (HRESULT: 0x{ex.ErrorCode:X8}).");
                    AppendResult("  Possible reasons:");
                    AppendResult("    - Certificates don't exist for this server");
                    AppendResult("    - Certificates exist but are for a different server");
                    AppendResult("    - Certificates are expired or invalid");
                    AppendResult("    - Server URL in config doesn't match certificate server");
                }
                return false;
            }
            catch (Exception ex)
            {
                // If check fails for any other reason, assume certificates don't exist
                AppendResult($"Error calling IpcGetTemplateList: {ex.Message}");
                AppendResult("  Certificates may not be available for this server.");
                return false;
            }
        }

        // OAuth2 (Purview) support via MSAL -> MSIPC token callback
        private class MsipcAuthContext
        {
            public string ClientId { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
            public string[] Scopes { get; set; } = new[] { "https://purview.azure.net/.default" };
            public string RedirectUri { get; set; } = "http://localhost";
        }

        private MsipcAuthContext LoadPurviewAuth()
        {
            try
            {
                var cfgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "purview.oauth.json");
                if (System.IO.File.Exists(cfgPath))
                {
                    return JsonConvert.DeserializeObject<MsipcAuthContext>(System.IO.File.ReadAllText(cfgPath)) ?? new MsipcAuthContext();
                }
            }
            catch { }
            return new MsipcAuthContext();
        }

        private class AdrmsConfig
        {
            public string serverUrl { get; set; } = string.Empty; // certification URL (used when licensingOnlyClusters=false)
            public string licensingUrl { get; set; } = string.Empty; // licensing URL (used when licensingOnlyClusters=true, optional)
            public string extranetUrl { get; set; } = string.Empty;
            public string intranetUrl { get; set; } = string.Empty;
            public bool licensingOnlyClusters { get; set; } = true; // maps to OverrideServiceDiscoveryForLicensing
        }

        /// <summary>
        /// Gets AD RMS ConnectionInfo from config file.
        /// 
        /// According to MSIPC SDK documentation and examples:
        /// - ConnectionInfo URLs can be either:
        ///   1. Base URL only: "https://server.com/" -> SDK will auto-discover standard paths (/_wmcs/Certification, /_wmcs/licensing)
        ///   2. Full path: "https://server.com/_wmcs/certificationexternal" -> SDK uses this exact path + "/certification.asmx"
        /// - SDK automatically appends /certification.asmx or /licensing.asmx to the provided URL
        /// - SDK can auto-discover standard paths from base URL, but NOT custom paths like "certificationexternal"
        /// 
        /// Examples:
        /// - Base URL: "https://server.com/" -> SDK discovers /_wmcs/Certification or /_wmcs/licensing automatically
        /// - Custom path: "https://server.com/_wmcs/certificationexternal" -> SDK uses this + "/certification.asmx"
        /// - Custom licensing: "https://server.com/_wmcs/licensingexternal" -> SDK uses this + "/licensing.asmx"
        /// 
        /// If licensingOnlyClusters=false:
        ///   - URLs should point to certification cluster (base URL or full path)
        ///   - SDK will discover licensing service from certification cluster
        ///   - Uses serverUrl (certification URL)
        /// If licensingOnlyClusters=true:
        ///   - URLs should point to licensing cluster (base URL or full path)
        ///   - SDK uses URLs directly for licensing (no discovery from certification)
        ///   - Uses licensingUrl if provided, otherwise uses serverUrl
        /// </summary>
        private ConnectionInfo GetAdrmsConnectionInfo()
        {
            var cfgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "adrms.json");
            AdrmsConfig cfg = null;
            try
            {
                if (System.IO.File.Exists(cfgPath))
                {
                    cfg = JsonConvert.DeserializeObject<AdrmsConfig>(System.IO.File.ReadAllText(cfgPath));
                }
            }
            catch { }

            Uri extranet = null;
            Uri intranet = null;
            if (cfg != null)
            {
                // Build ConnectionInfo similar to working example:
                // Uri extranetUrl = new Uri("https://protection.sealpath.com/");
                // Uri intranetUrl = extranetUrl;
                // ConnectionInfo ci = new ConnectionInfo(extranetUrl, intranetUrl);
                
                if (!string.IsNullOrWhiteSpace(cfg.extranetUrl))
                {
                    AppendResult($"Using extranetUrl from config: {cfg.extranetUrl}");
                    extranet = ValidateAndCreateUri(cfg.extranetUrl, "Extranet");
                }
                if (!string.IsNullOrWhiteSpace(cfg.intranetUrl))
                {
                    AppendResult($"Using intranetUrl from config: {cfg.intranetUrl}");
                    intranet = ValidateAndCreateUri(cfg.intranetUrl, "Intranet");
                }
                
                // If extranet/intranet URLs not specified, use serverUrl or licensingUrl
                if (extranet == null && intranet == null)
                {
                    string urlToUse = null;
                    
                    if (cfg.licensingOnlyClusters)
                    {
                        // For licensing-only clusters, prefer licensingUrl, fallback to serverUrl
                        urlToUse = !string.IsNullOrWhiteSpace(cfg.licensingUrl) ? cfg.licensingUrl : cfg.serverUrl;
                    }
                    else
                    {
                        // For certification clusters, use serverUrl (certification URL)
                        urlToUse = cfg.serverUrl;
                    }
                    
                    if (!string.IsNullOrWhiteSpace(urlToUse))
                    {
                        AppendResult($"Using serverUrl from config: {urlToUse}");
                        AppendResult($"Creating Uri directly (like working example)...");
                        
                        // Create Uri directly like the working example - simple and direct
                        try
                        {
                            Uri baseUri = new Uri(urlToUse);
                            extranet = baseUri;
                            intranet = baseUri;
                            
                            AppendResult($"ConnectionInfo will use: Extranet={extranet}, Intranet={intranet}");
                            
                            // Optional: Validate DNS (but don't fail if it fails)
                            try
                            {
                                IPAddress[] addresses = Dns.GetHostAddresses(baseUri.Host);
                                AppendResult($"DNS resolution OK: {baseUri.Host} resolves to {string.Join(", ", addresses.Select(a => a.ToString()))}");
                            }
                            catch (Exception ex)
                            {
                                AppendResult($"WARNING: DNS check failed: {ex.Message} (continuing anyway)");
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendResult($"ERROR: Failed to create Uri from '{urlToUse}': {ex.Message}");
                            return null;
                        }
                    }
                }
                
                if (extranet != null || intranet != null)
                {
                    // licensingOnlyClusters flag determines how MSIPC uses the URLs:
                    // false: Use URLs to locate certification cluster, then discover licensing service
                    // true: Use URLs directly to locate licensing server (for licensing-only clusters)
                    AppendResult($"Creating ConnectionInfo with licensingOnlyClusters={cfg.licensingOnlyClusters}");
                    ConnectionInfo ci = new ConnectionInfo(extranet, intranet, cfg.licensingOnlyClusters);
                    AppendResult($"ConnectionInfo created successfully: Extranet={ci.ExtranetUrl}, Intranet={ci.IntranetUrl}");
                    return ci;
                }
            }

            // If config missing, return null to indicate misconfiguration
            AppendResult("AD RMS config not found at config/adrms.json");
            return null;
        }

        /// <summary>
        /// Validates a server URL by checking DNS resolution and HTTP response.
        /// 
        /// If URL is a base URL (e.g., "https://server.com/"), only DNS resolution is checked.
        /// If URL includes a path (e.g., "https://server.com/_wmcs/certificationexternal"), 
        /// also checks HTTP response by appending /certification.asmx or /licensing.asmx.
        /// </summary>
        private Uri ValidateAndCreateUri(string urlString, string urlType)
        {
            try
            {
                AppendResult($"Validating {urlType} URL: {urlString}");
                
                // Parse the URI
                Uri uri;
                if (!Uri.TryCreate(urlString, UriKind.Absolute, out uri))
                {
                    AppendResult($"  ERROR: Invalid URI format: {urlString}");
                    return null;
                }

                // Check DNS resolution
                string hostname = uri.Host;
                AppendResult($"  Checking DNS resolution for: {hostname}");
                
                try
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(hostname);
                    if (addresses == null || addresses.Length == 0)
                    {
                        AppendResult($"  ERROR: DNS resolution failed for {hostname}");
                        return null;
                    }
                    AppendResult($"  DNS resolution OK: {hostname} resolves to {string.Join(", ", addresses.Select(a => a.ToString()))}");
                }
                catch (SocketException ex)
                {
                    AppendResult($"  ERROR: DNS resolution failed for {hostname}: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    AppendResult($"  WARNING: DNS check failed: {ex.Message}");
                    // Continue anyway - DNS might be temporarily unavailable
                }

                // Check HTTP response for certification.asmx or licensing.asmx
                // Only if URL includes a path (not just base URL)
                if ((uri.Scheme == "https" || uri.Scheme == "http") && uri.AbsolutePath != "/")
                {
                    AppendResult($"  Checking server response...");
                    try
                    {
                        // Build certification.asmx URL by appending to the server URL
                        // Use the exact URL from config and add "/certification.asmx"
                        string basePath = uri.AbsolutePath.TrimEnd('/');
                        string endpoint = urlType.ToLower().Contains("licensing") ? "licensing.asmx" : "certification.asmx";
                        string testUrl = $"{uri.Scheme}://{uri.Host}{basePath}/{endpoint}";
                        
                        AppendResult($"  Testing: {testUrl}");
                        
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(testUrl);
                        request.Method = "GET";
                        request.Timeout = 5000; // 5 second timeout
                        request.AllowAutoRedirect = true;
                        request.UserAgent = "MSIPC-ProtectSuite/1.0";
                        
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            AppendResult($"  Server response: HTTP {response.StatusCode} ({response.StatusDescription})");
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                AppendResult($"  {urlType} URL validation successful");
                                return uri;
                            }
                            else
                            {
                                AppendResult($"  WARNING: Unexpected HTTP status code: {response.StatusCode}");
                                // Still return the URI - might be valid but with different response
                                return uri;
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            // Even if we get an error response, the server is reachable
                            AppendResult($"  Server reachable: HTTP {response.StatusCode} ({response.StatusDescription})");
                            AppendResult($"  {urlType} URL validation successful (server responded)");
                            return uri;
                        }
                        else
                        {
                            AppendResult($"  WARNING: Could not reach server: {ex.Message}");
                            // Still return the URI - might be a timeout but server could be valid
                            return uri;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendResult($"  WARNING: Server check failed: {ex.Message}");
                        // Still return the URI - validation failed but might work anyway
                        return uri;
                    }
                }
                else
                {
                    AppendResult($"  WARNING: Unsupported URI scheme: {uri.Scheme}");
                    return uri; // Return anyway - might be valid
                }
            }
            catch (Exception ex)
            {
                AppendResult($"  ERROR: URL validation failed: {ex.Message}");
                return null;
            }
        }

        private SafeIpcPromptContext CreatePromptContextSuppressingUI(out OAuth2CallbackContext oauth2Ctx)
        {
            oauth2Ctx = null;
            if (selectedBackend != BackendType.Purview)
            {
                // For AD RMS we don't use OAuth2; caller may fall back to SSO or cert/symm key
                return SafeNativeMethods.CreateIpcPromptContext(suppressUI: true, offline: false, hasUserConsent: false, this);
            }

            var auth = LoadPurviewAuth();
            var app = PublicClientApplicationBuilder.Create(auth.ClientId)
                .WithRedirectUri(auth.RedirectUri)
                .WithAuthority(AzureCloudInstance.AzurePublic, auth.TenantId)
                .Build();

            GetAuthenticationTokenDelegate tokenDelegate = (ctx, authParams) =>
            {
                // Acquire AAD token silently if possible; otherwise, device code flow
                AuthenticationResult result = null;
                try
                {
                    result = app.AcquireTokenSilent(auth.Scopes, app.GetAccountsAsync().GetAwaiter().GetResult().FirstOrDefault())
                        .ExecuteAsync().GetAwaiter().GetResult();
                }
                catch
                {
                    result = app.AcquireTokenInteractive(auth.Scopes)
                        .WithPrompt(Prompt.NoPrompt)
                        .ExecuteAsync().GetAwaiter().GetResult();
                }

                var tokenHandle = SafeNativeMethods.IpcCreateOAuth2Token(result.AccessToken);
                return tokenHandle;
            };

            oauth2Ctx = new OAuth2CallbackContext(new object(), tokenDelegate);
            var ipcCtx = SafeNativeMethods.CreateIpcPromptContext(suppressUI: true, offline: false, hasUserConsent: false, this, oauth2Ctx);
            return ipcCtx;
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Title = "Select File to Protect/Unprotect";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    txtFilePath.Text = selectedFilePath;
                    AppendResult($"Selected file: {selectedFilePath}");
                    
                    // Detect if file is protected and update UI accordingly
                    UpdateUIForFileState();
                }
            }
        }

        /// <summary>
        /// Detects if the selected file is protected and updates UI buttons accordingly
        /// </summary>
        private void UpdateUIForFileState()
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                // No file selected or file doesn't exist
                btnProtect.Enabled = false;
                btnUnprotect.Enabled = false;
                btnGetInfo.Enabled = false;
                btnViewContent.Enabled = false;
                return;
            }

            bool isProtected = IsFileProtected(selectedFilePath);
            
            if (isProtected)
            {
                // File is protected: enable unprotect, get info, and view content; disable protect
                btnProtect.Enabled = false;
                btnUnprotect.Enabled = true;
                btnGetInfo.Enabled = true;
                btnViewContent.Enabled = true;
                AppendResult("File is protected. Protection disabled, Unprotect/View/Get Info enabled.");
            }
            else
            {
                // File is not protected: enable protect; disable unprotect, get info, view content
                btnProtect.Enabled = true;
                btnUnprotect.Enabled = false;
                btnGetInfo.Enabled = false;
                btnViewContent.Enabled = true; // Can still view unprotected files
                AppendResult("File is not protected. Protect enabled, Unprotect/Get Info disabled.");
            }
        }

        /// <summary>
        /// Checks if a file is protected using MSIPC API function IpcfIsFileEncrypted
        /// Falls back to manual format check if API fails (e.g., cross-platform scenarios)
        /// </summary>
        private bool IsFileProtected(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            // First try MSIPC API
            try
            {
                SafeFileApiNativeMethods.FileEncryptedStatus status = 
                    SafeFileApiNativeMethods.IpcfIsFileEncrypted(filePath);

                // File is protected if status is ENCRYPTED or ENCRYPTED_CUSTOM
                return status == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED ||
                       status == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED_CUSTOM;
            }
            catch (InformationProtectionException ex)
            {
                // MSIPC API failed - this might happen if:
                // - File is from different platform (Purview vs AD RMS)
                // - Configuration mismatch (tenant parameter null)
                // - File format not recognized by MSIPC
                // Log error but don't assume file is protected
                System.Diagnostics.Debug.WriteLine($"IpcfIsFileEncrypted failed (HRESULT: 0x{ex.ErrorCode:X8})");
                return false;
            }
            catch (Exception ex)
            {
                // Other errors - log and assume file is not protected
                System.Diagnostics.Debug.WriteLine($"IpcfIsFileEncrypted error: {ex.Message}");
                return false;
            }
        }

        private void RbBackend_CheckedChanged(object sender, EventArgs e)
        {
            // Only process when a radio button is checked (not when unchecked)
            // When switching from one radio to another, both events fire (one unchecked, one checked)
            RadioButton rb = sender as RadioButton;
            if (rb == null || !rb.Checked)
                return; // Ignore unchecked events
            
            BackendType newBackend;
            if (rbPurview.Checked)
                newBackend = BackendType.Purview;
            else if (rbAdRms.Checked)
                newBackend = BackendType.AdRms;
            else
                return; // No backend selected yet
            
            // Only update if backend actually changed (avoid duplicate calls)
            if (selectedBackend == newBackend)
                return;
            
            selectedBackend = newBackend;
            
            // Update MSIPC identity when backend changes to AD RMS
            // For Purview, identity will be retrieved during first operation
            GetMsipcUserIdentity();
        }

        private void BtnProtect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            try
            {
                AppendResult("Protecting file...");

                // 1) Resolve connection info based on backend
                ConnectionInfo connectionInfo = null;
                if (selectedBackend == BackendType.AdRms)
                {
                    connectionInfo = GetAdrmsConnectionInfo();
                    if (connectionInfo == null)
                    {
                        AppendResult("AD RMS connection info missing. Please configure config/adrms.json");
                        return;
                    }
                }

                // 2) Create license and protect file using MSIPC API
                SafeIpcPromptContext authCtx = CreatePromptContextSuppressingUI(out var oauth2Ctx);
                SafeInformationProtectionLicenseHandle licenseHandle = null;

                if (rdoTemplate.Checked)
                {
                    var selectedTemplate = GetSelectedTemplate();
                    if (selectedTemplate == null)
                    {
                        AppendResult("No template selected. Protection cancelled.");
                        return;
                    }

                    AppendResult($"Using template: {selectedTemplate.TemplateId}");
                    
                    // Create license from template
                    licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(selectedTemplate.TemplateId);
                }
                else
                {
                    if (this.customUserRights.Count == 0)
                    {
                        AppendResult("No custom user rights configured. Please click 'Edit User Rights...' to add users and their permissions.");
                        return;
                    }

                    // Custom rights: build a license from scratch and set user rights
                    var issuer = new TemplateIssuer(connectionInfo, "Custom", true);
                    licenseHandle = SafeNativeMethods.IpcCreateLicenseFromScratch(issuer);
                    
                    // Ensure the current user (owner) has FULL CONTROL (OWNER) permission
                    // This is automatically granted to the user who protects the document
                    // Create a deep copy of the user rights list to avoid reference issues
                    // Also add automatic permissions: VIEWRIGHTSDATA (except for OWNER) and DOCEDIT/SAVE (when EDIT is present)
                    var userRightsList = new System.Collections.ObjectModel.Collection<Microsoft.InformationProtectionAndControl.UserRights>();
                    foreach (var ur in this.customUserRights)
                    {
                        // Create a new collection for rights to ensure it's a deep copy
                        var rightsCopy = new System.Collections.ObjectModel.Collection<string>();
                        bool hasOwner = false;
                        bool hasEdit = false;
                        
                        foreach (var right in ur.Rights)
                        {
                            rightsCopy.Add(right);
                            if (right.Equals(Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight, StringComparison.OrdinalIgnoreCase))
                            {
                                hasOwner = true;
                            }
                            if (right.Equals(Microsoft.InformationProtectionAndControl.CommonRights.EditRight, StringComparison.OrdinalIgnoreCase))
                            {
                                hasEdit = true;
                            }
                        }
                        
                        // Add DOCEDIT and SAVE if EDIT is present
                        if (hasEdit)
                        {
                            if (!rightsCopy.Contains("DOCEDIT"))
                            {
                                rightsCopy.Add("DOCEDIT");
                            }
                            if (!rightsCopy.Contains("SAVE"))
                            {
                                rightsCopy.Add("SAVE");
                            }
                        }
                        
                        // Add VIEWRIGHTSDATA if not OWNER (OWNER already includes it)
                        if (!hasOwner)
                        {
                            if (!rightsCopy.Contains(Microsoft.InformationProtectionAndControl.CommonRights.ViewRightsDataRight))
                            {
                                rightsCopy.Add(Microsoft.InformationProtectionAndControl.CommonRights.ViewRightsDataRight);
                            }
                        }
                        
                        // Create a new UserRights object with the copied data
                        var urCopy = new Microsoft.InformationProtectionAndControl.UserRights(ur.UserIdType, ur.UserId, rightsCopy);
                        userRightsList.Add(urCopy);
                    }
                    
                    // Get current user identity - if not available, try to get it now
                    string ownerIdentity = currentUserIdentity;
                    if (string.IsNullOrEmpty(ownerIdentity))
                    {
                        // Try to get identity from MSIPC if available
                        try
                        {
                            // This is a fallback - ideally identity should be retrieved earlier
                            AppendResult("Warning: Current user identity not available. Attempting to retrieve...");
                            // Note: We can't easily get identity here without creating a license, so we'll proceed
                            // The owner will be set from the first user in the list or we'll use a placeholder
                        }
                        catch { }
                    }
                    
                    // Check if current user already has OWNER right
                    bool ownerHasFullControl = false;
                    if (!string.IsNullOrEmpty(ownerIdentity))
                    {
                        foreach (var ur in userRightsList)
                        {
                            if (ur.UserId.Equals(ownerIdentity, StringComparison.OrdinalIgnoreCase) &&
                                ur.Rights.Contains(Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight))
                            {
                                ownerHasFullControl = true;
                                break;
                            }
                        }
                        
                        // If owner doesn't have FULL CONTROL, add it automatically
                        if (!ownerHasFullControl)
                        {
                            // Check if owner already exists in the list
                            var ownerRights = userRightsList.FirstOrDefault(ur => ur.UserId.Equals(ownerIdentity, StringComparison.OrdinalIgnoreCase));
                            if (ownerRights != null)
                            {
                                // Add OWNER right to existing user rights, plus ensure they have VIEW
                                if (!ownerRights.Rights.Contains(Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight))
                                {
                                    ownerRights.Rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight);
                                }
                                // Ensure VIEW is also present (OWNER should imply all, but VIEW is explicit)
                                if (!ownerRights.Rights.Contains(Microsoft.InformationProtectionAndControl.CommonRights.ViewRight))
                                {
                                    ownerRights.Rights.Insert(0, Microsoft.InformationProtectionAndControl.CommonRights.ViewRight);
                                }
                                AppendResult($"Added FULL CONTROL (OWNER) and VIEW permissions to current user: {ownerIdentity}");
                            }
                            else
                            {
                                // Add new user rights entry with OWNER and VIEW permissions
                                var ownerRightsCollection = new System.Collections.ObjectModel.Collection<string>();
                                ownerRightsCollection.Add(Microsoft.InformationProtectionAndControl.CommonRights.ViewRight);
                                ownerRightsCollection.Add(Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight);
                                var newOwnerRights = new Microsoft.InformationProtectionAndControl.UserRights(
                                    Microsoft.InformationProtectionAndControl.UserIdType.Email,
                                    ownerIdentity,
                                    ownerRightsCollection);
                                userRightsList.Add(newOwnerRights);
                                AppendResult($"Automatically added current user with VIEW and FULL CONTROL (OWNER) permissions: {ownerIdentity}");
                            }
                        }
                    }
                    
                    // Set the license owner - this is important for MSIPC to know who owns the license
                    if (!string.IsNullOrEmpty(ownerIdentity))
                    {
                        try
                        {
                            SafeNativeMethods.IpcSetLicenseOwner(licenseHandle, ownerIdentity);
                            AppendResult($"Set license owner: {ownerIdentity}");
                        }
                        catch (Exception ex)
                        {
                            AppendResult($"Warning: Could not set license owner: {ex.Message}");
                        }
                    }
                    
                    AppendResult($"Setting custom rights for {userRightsList.Count} user(s)");
                    
                    // Log all permissions being set BEFORE setting them (for debugging)
                    AppendResult("Permissions to be set in license:");
                    foreach (var ur in userRightsList)
                    {
                        AppendResult($"  - User: {ur.UserId} (Type: {ur.UserIdType})");
                        AppendResult($"    Rights: {string.Join(", ", ur.Rights)}");
                        AppendResult($"    Rights count: {ur.Rights.Count}");
                    }
                    
                    // Set the user rights list in the license
                    try
                    {
                        SafeNativeMethods.IpcSetLicenseUserRightsList(licenseHandle, userRightsList);
                        AppendResult("User rights successfully set in license.");
                    }
                    catch (Exception ex)
                    {
                        AppendResult($"ERROR: Failed to set user rights in license: {ex.Message}");
                        AppendResult($"  This will prevent users from accessing the protected file.");
                        throw; // Re-throw to prevent protection with invalid permissions
                    }
                    
                    // Offer to save the license template (XrML format) before protecting
                    try
                    {
                        var result = MessageBox.Show(
                            "Do you want to save the license template before protecting?\n\n" +
                            "Options:\n" +
                            "1. XrML format (complete template with structure, some parts may be encrypted)\n" +
                            "2. Plain text XML (extracted properties, all readable)\n" +
                            "3. Both\n\n" +
                            "Yes = Save XrML template\n" +
                            "No = Save plain text template\n" +
                            "Cancel = Skip",
                            "Save License Template",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            // Save XrML format (serialized license)
                            SaveLicenseXrMLToFile(licenseHandle, selectedFilePath, "template_xrml");
                        }
                        else if (result == DialogResult.No)
                        {
                            // Save plain text template
                            SaveLicenseTemplateToFile(licenseHandle, selectedFilePath, "template_plain");
                        }
                        // Cancel = Skip, continue with protection
                    }
                    catch (Exception ex)
                    {
                        AppendResult($"Warning: Could not offer license template save: {ex.Message}");
                        // Continue with protection anyway
                    }
                }

                // Protect file using MSIPC API (IpcfEncryptFile)
                try
                {
                    string protectedFilePath = SafeFileApiNativeMethods.IpcfEncryptFile(
                        selectedFilePath,
                        licenseHandle,
                        SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: false,
                        parentWindow: this,
                        symmKey: null,
                        outputDirectory: null);

                    if (!string.IsNullOrEmpty(protectedFilePath) && System.IO.File.Exists(protectedFilePath))
                    {
                        AppendResult($"Protected file written: {protectedFilePath}");
                        
                        // Update selected file to the protected version and update UI
                        selectedFilePath = protectedFilePath;
                        txtFilePath.Text = selectedFilePath;
                        UpdateUIForFileState();
                    }
                    else
                    {
                        throw new Exception("IpcfEncryptFile did not return a valid file path");
                    }
                }
                finally
                {
                    licenseHandle?.Dispose();
                }

                AppendResult("File protection completed successfully.");
            }
            catch (Exception ex)
            {
                AppendResult($"Error protecting file: {ex.Message}");
            }
        }

        private void BtnUnprotect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            try
            {
                AppendResult("Unprotecting file...");

                // Check if file is protected using MSIPC API
                SafeFileApiNativeMethods.FileEncryptedStatus status = 
                    SafeFileApiNativeMethods.IpcfIsFileEncrypted(selectedFilePath);
                
                if (status != SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED &&
                    status != SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED_CUSTOM)
                {
                    AppendResult("File is not protected with MSIPC.");
                    return;
                }

                // Check if certificates exist before attempting authentication
                ConnectionInfo connectionInfo = null;
                if (selectedBackend == BackendType.AdRms)
                {
                    connectionInfo = GetAdrmsConnectionInfo();
                }
                bool certificatesExist = false;
                if (connectionInfo != null)
                {
                    certificatesExist = CheckMsipcCertificatesExist(connectionInfo);
                }

                // Unprotect file using MSIPC API (IpcfDecryptFile)
                SafeIpcPromptContext authCtx = CreatePromptContextSuppressingUI(out var oauth2Ctx);
                string unprotectedFilePath = SafeFileApiNativeMethods.IpcfDecryptFile(
                    selectedFilePath,
                    SafeFileApiNativeMethods.DecryptFlags.IPCF_DF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: false,
                    parentWindow: this,
                    symmKey: null,
                    outputDirectory: null);

                if (!string.IsNullOrEmpty(unprotectedFilePath) && System.IO.File.Exists(unprotectedFilePath))
                {
                    AppendResult($"Unprotected file written: {unprotectedFilePath}");
                    
                    // Update selected file to the unprotected version and update UI
                    selectedFilePath = unprotectedFilePath;
                    txtFilePath.Text = selectedFilePath;
                    UpdateUIForFileState();
                }
                else
                {
                    throw new Exception("IpcfDecryptFile did not return a valid file path");
                }

                AppendResult("File unprotection completed successfully.");
            }
            catch (InformationProtectionException ex)
            {
                AppendResult($"ERROR unprotecting file (HRESULT: 0x{ex.ErrorCode:X8}): {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendResult($"ERROR unprotecting file: {ex.Message}");
            }
        }

        private void BtnGetInfo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            try
            {
                AppendResult("Getting protection information...");

                // First check if file is protected using MSIPC API
                byte[] license = null;
                
                try
                {
                    SafeFileApiNativeMethods.FileEncryptedStatus status = 
                        SafeFileApiNativeMethods.IpcfIsFileEncrypted(selectedFilePath);
                    
                    if (status == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED ||
                        status == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED_CUSTOM)
                    {
                        AppendResult("File is protected with MSIPC.");
                        
                        // Get license from protected file using MSIPC API
                        license = SafeFileApiNativeMethods.IpcfGetSerializedLicenseFromFile(selectedFilePath);
                        if (license == null || license.Length == 0)
                        {
                            AppendResult("WARNING: Could not extract license from protected file.");
                            return;
                        }
                        AppendResult($"License extracted from file ({license.Length} bytes).");
                    }
                    else
                    {
                        AppendResult("File is not protected with MSIPC.");
                        return;
                    }
                }
                catch (InformationProtectionException ex)
                {
                    AppendResult($"ERROR: Failed to check file protection status (HRESULT: 0x{ex.ErrorCode:X8}).");
                    AppendResult($"  {ex.Message}");
                    return;
                }
                catch (Exception ex)
                {
                    AppendResult($"ERROR: Failed to check file protection status: {ex.Message}");
                    return;
                }

                if (license == null || license.Length == 0)
                {
                    AppendResult("ERROR: No license found in file.");
                    return;
                }

                // Get key from license to access license properties
                SafeInformationProtectionKeyHandle key = null;
                try
                {
                    // First try with suppressUI=true (silent mode)
                    try
                    {
                        SafeIpcPromptContext authCtx = CreatePromptContextSuppressingUI(out var oauth2Ctx);
                        key = SafeNativeMethods.IpcGetKey(
                            license,
                            suppressUI: true,
                            offline: false,
                            hasUserConsent: false,
                            parentWindow: this);
                        
                        if (key == null || key.IsInvalid)
                        {
                            throw new InformationProtectionException(unchecked((int)0x8004020A), "Key handle is invalid");
                        }
                    }
                    catch (InformationProtectionException ex)
                    {
                        // If silent mode fails, try with UI prompts enabled to allow authentication
                        uint errorCode = unchecked((uint)ex.ErrorCode);
                        uint msipcErrorMask = 0x80040000;
                        uint accessDeniedError = 0x80070005;
                        
                        if ((errorCode & msipcErrorMask) == msipcErrorMask || errorCode == accessDeniedError || errorCode == 0x8004020A)
                        {
                            AppendResult($"  Silent mode failed (HRESULT: 0x{ex.ErrorCode:X8}). Trying with UI prompts enabled...");
                            key?.Dispose();
                            key = null;
                            
                            // Retry with suppressUI=false and hasUserConsent=true to allow authentication
                            try
                            {
                                key = SafeNativeMethods.IpcGetKey(
                                    license,
                                    suppressUI: false,  // Allow UI prompts for authentication
                                    offline: false,
                                    hasUserConsent: true, // User has consented to authentication
                                    parentWindow: this);
                                
                                if (key != null && !key.IsInvalid)
                                {
                                    AppendResult("  Key acquired successfully with user authentication.");
                                }
                                else
                                {
                                    throw new InformationProtectionException(unchecked((int)0x8004020A), "Key handle is invalid after authentication");
                                }
                            }
                            catch (InformationProtectionException ex2)
                            {
                                AppendResult($"  Failed to acquire key even with UI prompts (HRESULT: 0x{ex2.ErrorCode:X8}).");
                                AppendResult("  This may indicate:");
                                AppendResult("    - User does not have permissions to access this file");
                                AppendResult("    - Authentication failed or was cancelled");
                                AppendResult("    - Certificates are missing or invalid");
                                throw; // Re-throw to be caught by outer catch
                            }
                        }
                        else
                        {
                            // Re-throw if it's not an authentication/permission error
                            throw;
                        }
                    }
                }
                catch (InformationProtectionException ex)
                {
                    AppendResult($"WARNING: Could not acquire key from license (HRESULT: 0x{ex.ErrorCode:X8}).");
                    AppendResult("  Some information may not be available without the key.");
                    AppendResult("  To view detailed permissions, you may need to:");
                    AppendResult("    - Have VIEW or higher permissions on the file");
                    AppendResult("    - Authenticate if prompted");
                    AppendResult("    - Ensure certificates are valid");
                }
                catch (Exception ex)
                {
                    AppendResult($"WARNING: Could not acquire key from license: {ex.Message}");
                    AppendResult("  Some information may not be available without the key.");
                }

                // Get license properties
                try
                {
                    // Get MSIPC user identity from the key
                    if (key != null && !key.IsInvalid)
                    {
                        try
                        {
                            string msipcUser = SafeNativeMethods.IpcGetKeyUserDisplayName(key);
                            if (!string.IsNullOrEmpty(msipcUser))
                            {
                                AppendResult($"MSIPC User Identity: {msipcUser}");
                            }
                        }
                        catch { }
                    }

                    // Get owner first to determine if we can get all permissions
                    string owner = null;
                    try
                    {
                        owner = SafeNativeMethods.IpcGetSerializedLicenseOwner(license, key);
                        if (!string.IsNullOrEmpty(owner))
                        {
                            AppendResult($"Owner: {owner}");
                        }
                    }
                    catch { }

                    // Determine if current user is the owner/protector
                    bool isOwner = false;
                    if (!string.IsNullOrEmpty(owner) && !string.IsNullOrEmpty(currentUserIdentity))
                    {
                        isOwner = owner.Equals(currentUserIdentity, StringComparison.OrdinalIgnoreCase);
                        if (isOwner)
                        {
                            AppendResult($"Current user is the owner/protector - can access all permissions.");
                        }
                        else
                        {
                            AppendResult($"Current user is NOT the owner - will show only current user's permissions.");
                        }
                    }

                    // Get user permissions - detailed information
                    if (key != null && !key.IsInvalid)
                    {
                        if (isOwner)
                        {
                            // Owner can deserialize the Publishing License and get all user permissions
                            try
                            {
                                AppendResult("");
                                AppendResult("=== All User Permissions (Owner Access) ===");
                                var allUserRights = SafeNativeMethods.IpcGetSerializedLicenseUserRightsList(license, key);
                                if (allUserRights != null && allUserRights.Count > 0)
                                {
                                    AppendResult($"Total users with permissions: {allUserRights.Count}");
                                    foreach (var userRights in allUserRights)
                                    {
                                        AppendResult($"");
                                        AppendResult($"  User: {userRights.UserId} (Type: {userRights.UserIdType})");
                                        AppendResult($"    Permissions: {string.Join(", ", userRights.Rights)}");
                                        AppendResult($"    Total permissions: {userRights.Rights.Count}");
                                    }
                                }
                                else
                                {
                                    AppendResult("No user permissions found in license.");
                                }
                            }
                            catch (Exception ex)
                            {
                                AppendResult($"WARNING: Could not retrieve all user permissions: {ex.Message}");
                            }
                        }
                        else
                        {
                            // Non-owner: Get only current user's permissions using IpcAccessCheck
                            try
                            {
                                AppendResult("");
                                AppendResult("=== Current User Permissions ===");
                                var availableRights = new List<string>();
                                
                                // Check all common rights
                                string[] allRights = new string[]
                                {
                                    Microsoft.InformationProtectionAndControl.CommonRights.ViewRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.EditRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.PrintRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.ExportRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.ExtractRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.CommentRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.ForwardRight,
                                    Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight
                                };
                                
                                foreach (string right in allRights)
                                {
                                    try
                                    {
                                        bool hasRight = SafeNativeMethods.IpcAccessCheck(key, right);
                                        if (hasRight)
                                        {
                                            availableRights.Add(right);
                                        }
                                    }
                                    catch
                                    {
                                        // Some rights may not be checkable, skip
                                    }
                                }
                                
                                if (availableRights.Count > 0)
                                {
                                    AppendResult($"Current user has {availableRights.Count} permission(s):");
                                    foreach (string right in availableRights)
                                    {
                                        AppendResult($"  - {right}");
                                    }
                                }
                                else
                                {
                                    AppendResult("Current user has no permissions on this file.");
                                    AppendResult("  Note: This may be because:");
                                    AppendResult("    - The key could not be acquired (see warnings above)");
                                    AppendResult("    - User truly has no permissions");
                                    AppendResult("    - Authentication is required but was not provided");
                                }
                            }
                            catch (Exception ex)
                            {
                                AppendResult($"WARNING: Could not check current user permissions: {ex.Message}");
                                AppendResult("  This is likely because the key could not be acquired.");
                            }
                        }
                    }
                    else
                    {
                        // Key is null or invalid - cannot check permissions
                        AppendResult("");
                        AppendResult("=== Current User Permissions ===");
                        AppendResult("ERROR: Cannot determine current user permissions.");
                        AppendResult("  Reason: Could not acquire key from license.");
                        AppendResult("  Possible causes:");
                        AppendResult("    - User does not have VIEW or higher permissions");
                        AppendResult("    - Authentication is required but suppressUI prevented prompts");
                        AppendResult("    - Certificates are missing or invalid");
                        AppendResult("    - File is protected by a different server/tenant");
                        AppendResult("  Solution: Try 'Unprotect' operation to authenticate, or ensure you have permissions.");
                    }

                    // Get content ID
                    try
                    {
                        string contentId = SafeNativeMethods.IpcGetSerializedLicenseContentId(license, key);
                        if (!string.IsNullOrEmpty(contentId))
                        {
                            AppendResult($"ContentId: {contentId}");
                        }
                    }
                    catch { }

                    // Get server info from license
                    if (key != null && !key.IsInvalid)
                    {
                        try
                        {
                            var licenseConnInfo = SafeNativeMethods.IpcGetSerializedLicenseConnectionInfo(license, key);
                            if (licenseConnInfo != null)
                            {
                                string serverInfo = "";
                                if (licenseConnInfo.ExtranetUrl != null)
                                    serverInfo = licenseConnInfo.ExtranetUrl.Host;
                                else if (licenseConnInfo.IntranetUrl != null)
                                    serverInfo = licenseConnInfo.IntranetUrl.Host;
                                if (!string.IsNullOrEmpty(serverInfo))
                                    AppendResult($"Server: {serverInfo}");
                            }
                        }
                        catch { }
                    }

                    // Get app-specific data
                    try
                    {
                        var appData = SafeNativeMethods.IpcGetSerializedLicenseAppSpecificDataNoEncryption(license, key);
                        if (appData != null && appData.Count > 0)
                        {
                            AppendResult($"AppData entries: {appData.Count}");
                            foreach (string appKey in appData.AllKeys)
                            {
                                AppendResult($"  {appKey}: {appData[appKey]}");
                            }
                        }
                    }
                    catch { }
                }
                finally
                {
                    key?.Dispose();
                }

                AppendResult("Protection information retrieved successfully.");
            }
            catch (InformationProtectionException ex)
            {
                AppendResult($"ERROR getting protection info (HRESULT: 0x{ex.ErrorCode:X8}): {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendResult($"ERROR getting protection info: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds the complete XrML template in plain text format with all permissions visible
        /// This constructs the XrML structure manually from license properties before encryption
        /// All user rights and permissions are in plain text - this is the template BEFORE encryption
        /// </summary>
        private void SaveLicenseXrMLToFile(SafeInformationProtectionLicenseHandle licenseHandle, string baseFilePath, string suffix)
        {
            try
            {
                AppendResult("Building XrML template from license properties (all permissions in plain text)...");
                
                // Build XrML document manually from license properties
                var xmlDoc = new System.Xml.XmlDocument();
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(xmlDeclaration);
                
                // Create root XrML element with namespace
                var xrmlElement = xmlDoc.CreateElement("XrML", "http://www.xrml.org/schema/2002/06/xrml-core");
                xmlDoc.AppendChild(xrmlElement);
                
                // Create ISSUEDTIME
                var issuedTimeElement = xmlDoc.CreateElement("ISSUEDTIME", "http://www.xrml.org/schema/2002/06/xrml-core");
                issuedTimeElement.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                xrmlElement.AppendChild(issuedTimeElement);
                
                // Create ISSUER section
                var issuerElement = xmlDoc.CreateElement("ISSUER", "http://www.xrml.org/schema/2002/06/xrml-core");
                xrmlElement.AppendChild(issuerElement);
                
                // Get connection info if available
                try
                {
                    ConnectionInfo connInfo = null;
                    if (selectedBackend == BackendType.AdRms)
                    {
                        connInfo = GetAdrmsConnectionInfo();
                    }
                    
                    if (connInfo != null)
                    {
                        var objectElement = xmlDoc.CreateElement("OBJECT", "http://www.xrml.org/schema/2002/06/xrml-core");
                        issuerElement.AppendChild(objectElement);
                        
                        var idElement = xmlDoc.CreateElement("ID", "http://www.xrml.org/schema/2002/06/xrml-core");
                        if (connInfo.IntranetUrl != null)
                        {
                            idElement.InnerText = connInfo.IntranetUrl.ToString();
                        }
                        else if (connInfo.ExtranetUrl != null)
                        {
                            idElement.InnerText = connInfo.ExtranetUrl.ToString();
                        }
                        objectElement.AppendChild(idElement);
                    }
                }
                catch { }
                
                // Create BODY section
                var bodyElement = xmlDoc.CreateElement("BODY", "http://www.xrml.org/schema/2002/06/xrml-core");
                xrmlElement.AppendChild(bodyElement);
                
                // Create WORK section
                var workElement = xmlDoc.CreateElement("WORK", "http://www.xrml.org/schema/2002/06/xrml-core");
                bodyElement.AppendChild(workElement);
                
                // Get and add OWNER
                try
                {
                    string owner = SafeNativeMethods.IpcGetLicenseOwner(licenseHandle);
                    if (!string.IsNullOrEmpty(owner))
                    {
                        var ownerElement = xmlDoc.CreateElement("OWNER", "http://www.xrml.org/schema/2002/06/xrml-core");
                        workElement.AppendChild(ownerElement);
                        
                        var ownerObjectElement = xmlDoc.CreateElement("OBJECT", "http://www.xrml.org/schema/2002/06/xrml-core");
                        ownerElement.AppendChild(ownerObjectElement);
                        
                        var ownerIdElement = xmlDoc.CreateElement("ID", "http://www.xrml.org/schema/2002/06/xrml-core");
                        ownerIdElement.InnerText = owner;
                        ownerObjectElement.AppendChild(ownerIdElement);
                        
                        AppendResult($"  Owner: {owner}");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Warning: Could not get owner: {ex.Message}");
                }
                
                // Get and add USER RIGHTS LIST (all permissions in plain text!)
                try
                {
                    var userRightsList = SafeNativeMethods.IpcGetLicenseUserRightsList(licenseHandle);
                    if (userRightsList != null && userRightsList.Count > 0)
                    {
                        var rightsGroupElement = xmlDoc.CreateElement("RIGHTSGROUP", "http://www.xrml.org/schema/2002/06/xrml-core");
                        workElement.AppendChild(rightsGroupElement);
                        
                        AppendResult($"  User Rights: {userRightsList.Count} user(s)");
                        
                        foreach (var userRights in userRightsList)
                        {
                            var rightsElement = xmlDoc.CreateElement("RIGHTS", "http://www.xrml.org/schema/2002/06/xrml-core");
                            rightsGroupElement.AppendChild(rightsElement);
                            
                            // Add PRINCIPAL (user)
                            var principalElement = xmlDoc.CreateElement("PRINCIPAL", "http://www.xrml.org/schema/2002/06/xrml-core");
                            rightsElement.AppendChild(principalElement);
                            
                            var principalObjectElement = xmlDoc.CreateElement("OBJECT", "http://www.xrml.org/schema/2002/06/xrml-core");
                            principalElement.AppendChild(principalObjectElement);
                            
                            var principalIdElement = xmlDoc.CreateElement("ID", "http://www.xrml.org/schema/2002/06/xrml-core");
                            principalIdElement.InnerText = userRights.UserId; // Email or "ANYONE", "OWNER", etc.
                            principalObjectElement.AppendChild(principalIdElement);
                            
                            // Add RIGHTS (permissions) - ALL IN PLAIN TEXT!
                            foreach (var right in userRights.Rights)
                            {
                                var rightElement = xmlDoc.CreateElement("RIGHT", "http://www.xrml.org/schema/2002/06/xrml-core");
                                rightsElement.AppendChild(rightElement);
                                
                                var rightNameElement = xmlDoc.CreateElement("NAME", "http://www.xrml.org/schema/2002/06/xrml-core");
                                rightNameElement.InnerText = right; // VIEW, EDIT, OWNER, etc.
                                rightElement.AppendChild(rightNameElement);
                            }
                            
                            AppendResult($"    - {userRights.UserId} ({userRights.UserIdType}): {string.Join(", ", userRights.Rights)}");
                        }
                    }
                    else
                    {
                        AppendResult("  Warning: No user rights found in license template.");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Warning: Could not get user rights list: {ex.Message}");
                }
                
                // Get and add CONTENT ID if available
                try
                {
                    string contentId = SafeNativeMethods.IpcGetLicenseContentId(licenseHandle);
                    if (!string.IsNullOrEmpty(contentId))
                    {
                        var contentIdElement = xmlDoc.CreateElement("CONTENTID", "http://www.xrml.org/schema/2002/06/xrml-core");
                        contentIdElement.InnerText = contentId;
                        workElement.AppendChild(contentIdElement);
                        AppendResult($"  ContentId: {contentId}");
                    }
                }
                catch { }
                
                // Get and add VALIDITYTIME if available
                try
                {
                    var validityTime = SafeNativeMethods.IpcGetLicenseValidityTime(licenseHandle);
                    if (validityTime != null && Microsoft.InformationProtectionAndControl.Term.IsValid(validityTime))
                    {
                        var validityElement = xmlDoc.CreateElement("VALIDITYTIME", "http://www.xrml.org/schema/2002/06/xrml-core");
                        workElement.AppendChild(validityElement);
                        
                        if (validityTime.From != null && validityTime.From != DateTime.FromFileTime(0))
                        {
                            var fromElement = xmlDoc.CreateElement("FROM", "http://www.xrml.org/schema/2002/06/xrml-core");
                            fromElement.InnerText = validityTime.From.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                            validityElement.AppendChild(fromElement);
                        }
                        
                        if (validityTime.Duration != null && validityTime.Duration.Ticks > 0)
                        {
                            var untilElement = xmlDoc.CreateElement("UNTIL", "http://www.xrml.org/schema/2002/06/xrml-core");
                            DateTime until = validityTime.From.Add(validityTime.Duration);
                            untilElement.InnerText = until.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                            validityElement.AppendChild(untilElement);
                        }
                        
                        DateTime end = validityTime.From.Add(validityTime.Duration);
                        AppendResult($"  ValidityTime: {validityTime.From} to {end}");
                    }
                }
                catch { }
                
                // Save XrML to file
                string baseName = System.IO.Path.GetFileNameWithoutExtension(baseFilePath);
                string suggestedFileName = $"{baseName}_license_{suffix}.xml";
                
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "XML files (*.xml)|*.xml|XrML files (*.xrml)|*.xrml|All files (*.*)|*.*";
                    saveDialog.FilterIndex = 1;
                    saveDialog.DefaultExt = "xml";
                    saveDialog.FileName = suggestedFileName;
                    saveDialog.Title = "Save License Template (XrML Format - Plain Text)";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Save formatted XrML
                        xmlDoc.Save(saveDialog.FileName);
                        AppendResult($"XrML template (plain text) saved successfully to: {saveDialog.FileName}");
                        AppendResult("  This file contains the complete XrML structure with ALL permissions in plain text.");
                        AppendResult("  This is the template BEFORE encryption - all user rights are visible and readable.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendResult($"ERROR: Failed to build XrML template: {ex.Message}");
                MessageBox.Show(
                    $"Failed to build XrML template:\n{ex.Message}",
                    "Save XrML Template Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Saves the license template (plain text) with all properties before serialization
        /// This extracts all license properties in plain text XML format
        /// </summary>
        private void SaveLicenseTemplateToFile(SafeInformationProtectionLicenseHandle licenseHandle, string baseFilePath, string suffix)
        {
            try
            {
                AppendResult("Extracting license template properties (plain text)...");
                
                var xmlDoc = new System.Xml.XmlDocument();
                var rootElement = xmlDoc.CreateElement("LicenseTemplate");
                xmlDoc.AppendChild(rootElement);
                
                // Get Owner
                try
                {
                    string owner = SafeNativeMethods.IpcGetLicenseOwner(licenseHandle);
                    if (!string.IsNullOrEmpty(owner))
                    {
                        var ownerElement = xmlDoc.CreateElement("Owner");
                        ownerElement.InnerText = owner;
                        rootElement.AppendChild(ownerElement);
                        AppendResult($"  Owner: {owner}");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Warning: Could not get owner: {ex.Message}");
                }
                
                // Get User Rights List (this is the key - permissions in plain text!)
                try
                {
                    var userRightsList = SafeNativeMethods.IpcGetLicenseUserRightsList(licenseHandle);
                    if (userRightsList != null && userRightsList.Count > 0)
                    {
                        var userRightsElement = xmlDoc.CreateElement("UserRightsList");
                        rootElement.AppendChild(userRightsElement);
                        
                        AppendResult($"  User Rights: {userRightsList.Count} user(s)");
                        foreach (var userRights in userRightsList)
                        {
                            var userElement = xmlDoc.CreateElement("User");
                            userElement.SetAttribute("UserId", userRights.UserId);
                            userElement.SetAttribute("UserIdType", userRights.UserIdType.ToString());
                            
                            var rightsElement = xmlDoc.CreateElement("Rights");
                            foreach (var right in userRights.Rights)
                            {
                                var rightElement = xmlDoc.CreateElement("Right");
                                rightElement.InnerText = right;
                                rightsElement.AppendChild(rightElement);
                            }
                            userElement.AppendChild(rightsElement);
                            userRightsElement.AppendChild(userElement);
                            
                            AppendResult($"    - {userRights.UserId} ({userRights.UserIdType}): {string.Join(", ", userRights.Rights)}");
                        }
                    }
                    else
                    {
                        AppendResult("  Warning: No user rights found in license template.");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Warning: Could not get user rights list: {ex.Message}");
                }
                
                // Get Content ID
                try
                {
                    string contentId = SafeNativeMethods.IpcGetLicenseContentId(licenseHandle);
                    if (!string.IsNullOrEmpty(contentId))
                    {
                        var contentIdElement = xmlDoc.CreateElement("ContentId");
                        contentIdElement.InnerText = contentId;
                        rootElement.AppendChild(contentIdElement);
                        AppendResult($"  ContentId: {contentId}");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Note: ContentId not available: {ex.Message}");
                }
                
                // Get Validity Time
                try
                {
                    var validityTime = SafeNativeMethods.IpcGetLicenseValidityTime(licenseHandle);
                    if (validityTime != null && Microsoft.InformationProtectionAndControl.Term.IsValid(validityTime))
                    {
                        var validityElement = xmlDoc.CreateElement("ValidityTime");
                        if (validityTime.From != null && validityTime.From != DateTime.FromFileTime(0))
                        {
                            validityElement.SetAttribute("From", validityTime.From.ToString("yyyy-MM-ddTHH:mm:ss"));
                        }
                        if (validityTime.Duration != null && validityTime.Duration.Ticks > 0)
                        {
                            validityElement.SetAttribute("Duration", validityTime.Duration.ToString());
                            DateTime endTime = validityTime.From.Add(validityTime.Duration);
                            validityElement.SetAttribute("To", endTime.ToString("yyyy-MM-ddTHH:mm:ss"));
                        }
                        rootElement.AppendChild(validityElement);
                        DateTime end = validityTime.From.Add(validityTime.Duration);
                        AppendResult($"  ValidityTime: {validityTime.From} to {end} (Duration: {validityTime.Duration})");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Note: ValidityTime not available: {ex.Message}");
                }
                
                // Get App Specific Data (if any)
                try
                {
                    var appData = SafeNativeMethods.IpcGetLicenseAppSpecificData(licenseHandle);
                    if (appData != null && appData.Count > 0)
                    {
                        var appDataElement = xmlDoc.CreateElement("AppSpecificData");
                        rootElement.AppendChild(appDataElement);
                        foreach (string key in appData.AllKeys)
                        {
                            var entryElement = xmlDoc.CreateElement("Entry");
                            entryElement.SetAttribute("Key", key);
                            entryElement.SetAttribute("Value", appData[key]);
                            appDataElement.AppendChild(entryElement);
                        }
                        AppendResult($"  AppSpecificData: {appData.Count} entries");
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"  Note: AppSpecificData not available: {ex.Message}");
                }
                
                // Save to file
                string baseName = System.IO.Path.GetFileNameWithoutExtension(baseFilePath);
                string suggestedFileName = $"{baseName}_license_{suffix}.xml";
                
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    saveDialog.FilterIndex = 1;
                    saveDialog.DefaultExt = "xml";
                    saveDialog.FileName = suggestedFileName;
                    saveDialog.Title = "Save License Template (Plain Text)";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        xmlDoc.Save(saveDialog.FileName);
                        AppendResult($"License template (plain text) saved successfully to: {saveDialog.FileName}");
                        AppendResult("  This file contains all license properties in plain text XML format.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendResult($"ERROR: Failed to save license template: {ex.Message}");
                MessageBox.Show(
                    $"Failed to save license template:\n{ex.Message}",
                    "Save License Template Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnViewContent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            try
            {
                AppendResult("Opening file content for viewing...");

                string fileToOpen = selectedFilePath;
                bool isProtected = false;

                // Check if file is protected using MSIPC API
                try
                {
                    SafeFileApiNativeMethods.FileEncryptedStatus status = 
                        SafeFileApiNativeMethods.IpcfIsFileEncrypted(selectedFilePath);
                    
                    if (status == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED ||
                        status == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_ENCRYPTED_CUSTOM)
                    {
                        isProtected = true;
                        AppendResult("File is protected. Opening protected file content (raw/encrypted view)...");
                        AppendResult("Note: To view decrypted content, use Unprotect first.");
                        
                        // Open the protected file directly without decrypting
                        // This shows the raw/encrypted content
                        fileToOpen = selectedFilePath;
                    }
                    else
                    {
                        AppendResult("File is not protected. Opening original file.");
                        fileToOpen = selectedFilePath;
                    }
                }
                catch (InformationProtectionException ex)
                {
                    // MSIPC API failed - this might happen if file is from different platform
                    // or configuration mismatch (e.g., tenant parameter null)
                    AppendResult($"ERROR: Failed to check file protection status via MSIPC API (HRESULT: 0x{ex.ErrorCode:X8})");
                    AppendResult("  This may occur if:");
                    AppendResult("  - The file is protected by a different platform (Purview vs AD RMS)");
                    AppendResult("  - There is a configuration mismatch (e.g., tenant not configured)");
                    AppendResult("  Opening file anyway (may show encrypted/raw content).");
                    fileToOpen = selectedFilePath; // Open anyway
                }
                catch (Exception ex)
                {
                    AppendResult($"WARNING: Failed to check file protection status: {ex.Message}");
                    AppendResult("  Opening file anyway (may show encrypted/raw content).");
                    fileToOpen = selectedFilePath; // Open anyway
                }

                // Open file in Notepad++ (or default editor)
                try
                {
                    // Try to find Notepad++ in common locations
                    string[] notepadPaths = new[]
                    {
                        @"C:\Program Files\Notepad++\notepad++.exe",
                        @"C:\Program Files (x86)\Notepad++\notepad++.exe",
                        Environment.GetEnvironmentVariable("ProgramFiles") + @"\Notepad++\notepad++.exe",
                        Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Notepad++\notepad++.exe"
                    };

                    string notepadExe = null;
                    foreach (var path in notepadPaths)
                    {
                        if (System.IO.File.Exists(path))
                        {
                            notepadExe = path;
                            break;
                        }
                    }

                    if (notepadExe != null)
                    {
                        Process.Start(notepadExe, $"\"{fileToOpen}\"");
                        AppendResult($"Opened file in Notepad++: {fileToOpen}");
                        if (isProtected)
                        {
                            AppendResult("Note: Showing protected/encrypted file content (raw view).");
                            AppendResult("      To view decrypted content, use 'Unprotect' first.");
                        }
                    }
                    else
                    {
                        // Fallback: Use default text editor
                        Process.Start(fileToOpen);
                        AppendResult($"Opened file with default application: {fileToOpen}");
                        if (isProtected)
                        {
                            AppendResult("Note: Notepad++ not found. Using default text editor.");
                            AppendResult("      Showing protected/encrypted file content (raw view).");
                            AppendResult("      To view decrypted content, use 'Unprotect' first.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppendResult($"Error opening file: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                AppendResult($"Error viewing file content: {ex.Message}");
            }
        }

        private void BtnEditCustomRights_Click(object sender, EventArgs e)
        {
            using (var dialog = new UserRightsEditorDialog())
            {
                dialog.LoadExistingRights(this.customUserRights);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.customUserRights = dialog.UserRightsList;
                    UpdateCustomRightsLabel();
                }
            }
        }

        private void UpdateCustomRightsLabel()
        {
            if (this.customUserRights.Count == 0)
            {
                this.lblCustomRights.Text = "No users configured";
            }
            else
            {
                var summary = string.Join("; ", this.customUserRights.Select(ur => 
                    $"{ur.UserId} ({string.Join(",", ur.Rights)})"));
                this.lblCustomRights.Text = summary.Length > 60 ? summary.Substring(0, 57) + "..." : summary;
            }
        }

        private TemplateInfo GetSelectedTemplate()
        {
            // Show template picker dialog and return selected template
            using (var templateDialog = new TemplatePickerDialog())
            {
                var connectionInfo = GetAdrmsConnectionInfo();
                templateDialog.LoadTemplates(connectionInfo);
                if (templateDialog.ShowDialog() == DialogResult.OK)
                {
                    return templateDialog.SelectedTemplate;
                }
            }
            return null;
        }

        private void AppendResult(string message)
        {
            if (txtResult.InvokeRequired)
            {
                txtResult.Invoke(new Action<string>(AppendResult), message);
            }
            else
            {
                txtResult.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                txtResult.SelectionStart = txtResult.Text.Length;
                txtResult.ScrollToCaret();
            }
        }
    }
}

