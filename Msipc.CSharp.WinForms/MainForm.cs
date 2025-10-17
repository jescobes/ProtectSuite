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
using Newtonsoft.Json;

namespace Msipc.CSharp.WinForms
{
    public partial class MainForm : Form
    {
        private Button btnSelectFile;
        private Button btnProtect;
        private Button btnUnprotect;
        private Button btnGetInfo;
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
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

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
            this.rbPurview.CheckedChanged += RbBackend_CheckedChanged;

            this.rbAdRms = new RadioButton();
            this.rbAdRms.Text = "AD RMS Server";
            this.rbAdRms.Location = new Point(180, 105);
            this.rbAdRms.Size = new Size(150, 20);
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
            this.btnProtect.Click += BtnProtect_Click;

            this.btnUnprotect = new Button();
            this.btnUnprotect.Text = "Unprotect File";
            this.btnUnprotect.Location = new Point(130, 310);
            this.btnUnprotect.Size = new Size(100, 30);
            this.btnUnprotect.Click += BtnUnprotect_Click;

            this.btnGetInfo = new Button();
            this.btnGetInfo.Text = "Get Protection Info";
            this.btnGetInfo.Location = new Point(240, 310);
            this.btnGetInfo.Size = new Size(120, 30);
            this.btnGetInfo.Click += BtnGetInfo_Click;

            // Result area
            this.lblResult = new Label();
            this.lblResult.Text = "Result:";
            this.lblResult.Location = new Point(20, 350);
            this.lblResult.Size = new Size(80, 20);

            this.txtResult = new TextBox();
            this.txtResult.Location = new Point(20, 375);
            this.txtResult.Size = new Size(540, 200);
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
            }
            catch (Exception ex)
            {
                AppendResult($"Error initializing MSIPC: {ex.Message}");
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
            public string serverUrl { get; set; } = string.Empty; // single URL fallback
            public string extranetUrl { get; set; } = string.Empty;
            public string intranetUrl { get; set; } = string.Empty;
            public bool licensingOnlyClusters { get; set; } = true; // maps to OverrideServiceDiscoveryForLicensing
        }

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
                if (!string.IsNullOrWhiteSpace(cfg.extranetUrl))
                    extranet = new Uri(cfg.extranetUrl);
                if (!string.IsNullOrWhiteSpace(cfg.intranetUrl))
                    intranet = new Uri(cfg.intranetUrl);
                if (extranet == null && intranet == null && !string.IsNullOrWhiteSpace(cfg.serverUrl))
                {
                    // Fallback: use serverUrl for both
                    var u = new Uri(cfg.serverUrl);
                    extranet = u;
                    intranet = u;
                }
                return new ConnectionInfo(extranet, intranet, cfg.licensingOnlyClusters);
            }

            // If config missing, return null to indicate misconfiguration
            AppendResult("AD RMS config not found at config/adrms.json");
            return null;
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
                }
            }
        }

        private void RbBackend_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPurview.Checked)
                selectedBackend = BackendType.Purview;
            else if (rbAdRms.Checked)
                selectedBackend = BackendType.AdRms;
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

                // 2) Get templates and choose first available
                SafeIpcPromptContext authCtx = CreatePromptContextSuppressingUI(out var oauth2Ctx);
                byte[] licenseBytes = null;
                SafeInformationProtectionKeyHandle key = null;

                if (rdoTemplate.Checked)
                {
                    var selectedTemplate = GetSelectedTemplate();
                    if (selectedTemplate == null)
                    {
                        AppendResult("No template selected. Protection cancelled.");
                        return;
                    }

                    AppendResult($"Using template: {selectedTemplate.Name} ({selectedTemplate.TemplateId})");

                    using (var lic = SafeNativeMethods.IpcCreateLicenseFromTemplateId(selectedTemplate.TemplateId))
                    {
                        licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                            lic,
                            SerializeLicenseFlags.KeyNoPersist,
                            suppressUI: true,
                            offline: false,
                            hasUserConsent: false,
                            parentWindow: this,
                            out key);
                    }
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
                    using (var lic = SafeNativeMethods.IpcCreateLicenseFromScratch(issuer))
                    {
                        AppendResult($"Setting custom rights for {this.customUserRights.Count} user(s)");

                        SafeNativeMethods.IpcSetLicenseUserRightsList(lic, new System.Collections.ObjectModel.Collection<Microsoft.InformationProtectionAndControl.UserRights>(this.customUserRights));

                        // Serialize to produce a content key
                        licenseBytes = SafeNativeMethods.IpcSerializeLicense(
                            lic,
                            SerializeLicenseFlags.KeyNoPersist,
                            suppressUI: true,
                            offline: false,
                            hasUserConsent: false,
                            parentWindow: this,
                            out key);
                    }
                }

                // Encrypt file content with IpcEncrypt using the key; simple container: [magic][u32 licLen][lic][cipher]
                var input = System.IO.File.ReadAllBytes(selectedFilePath);
                var data = (byte[])input.Clone();
                uint block = 0;
                SafeNativeMethods.IpcEncrypt(key, block, final: true, ref data);

                using (var ms = new System.IO.MemoryStream())
                using (var bw = new System.IO.BinaryWriter(ms))
                {
                    bw.Write(Encoding.ASCII.GetBytes("MSIPCDEMO\0"));
                    bw.Write((int)licenseBytes.Length);
                    bw.Write(licenseBytes);
                    bw.Write(data);
                    bw.Flush();
                    var outPath = System.IO.Path.ChangeExtension(selectedFilePath, ".pfile");
                    System.IO.File.WriteAllBytes(outPath, ms.ToArray());
                    AppendResult($"Protected file written: {outPath}");
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

                var all = System.IO.File.ReadAllBytes(selectedFilePath);
                using (var ms = new System.IO.MemoryStream(all))
                using (var br = new System.IO.BinaryReader(ms))
                {
                    var magic = br.ReadBytes(9); // "MSIPCDEMO\0"
                    var header = Encoding.ASCII.GetString(magic);
                    if (header != "MSIPCDEMO\0")
                    {
                        AppendResult("Not a demo-protected file.");
                        return;
                    }
                    var licLen = br.ReadInt32();
                    var license = br.ReadBytes(licLen);
                    var cipher = br.ReadBytes((int)(ms.Length - ms.Position));

                    // Acquire key; UI suppressed
                    SafeIpcPromptContext authCtx = CreatePromptContextSuppressingUI(out var oauth2Ctx);
                    var key = SafeNativeMethods.IpcGetKey(
                        license,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: false,
                        parentWindow: this);

                    var plain = SafeNativeMethods.IpcDecrypt(key, 0, true, cipher);
                    var outPath = System.IO.Path.ChangeExtension(selectedFilePath, ".unprot");
                    System.IO.File.WriteAllBytes(outPath, plain);
                    AppendResult($"Unprotected file written: {outPath}");
                }

                AppendResult("File unprotection completed successfully.");
            }
            catch (Exception ex)
            {
                AppendResult($"Error unprotecting file: {ex.Message}");
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

                var all = System.IO.File.ReadAllBytes(selectedFilePath);
                using (var ms = new System.IO.MemoryStream(all))
                using (var br = new System.IO.BinaryReader(ms))
                {
                    var magic = br.ReadBytes(9);
                    var header = Encoding.ASCII.GetString(magic);
                    if (header != "MSIPCDEMO\0")
                    {
                        AppendResult("Not a demo-protected file.");
                        return;
                    }
                    var licLen = br.ReadInt32();
                    var license = br.ReadBytes(licLen);

                    // Try to get some properties without key where possible
                    SafeInformationProtectionKeyHandle key = null;
                    try
                    {
                        SafeIpcPromptContext authCtx = CreatePromptContextSuppressingUI(out var oauth2Ctx);
                        key = SafeNativeMethods.IpcGetKey(
                            license,
                            suppressUI: true,
                            offline: false,
                            hasUserConsent: false,
                            parentWindow: this);
                    }
                    catch { }

                    var owner = SafeNativeMethods.IpcGetSerializedLicenseOwner(license, key);
                    AppendResult($"Owner: {owner}");
                    var contentId = SafeNativeMethods.IpcGetSerializedLicenseContentId(license, key);
                    AppendResult($"ContentId: {contentId}");
                    var appData = SafeNativeMethods.IpcGetSerializedLicenseAppSpecificDataNoEncryption(license, key);
                    AppendResult($"AppData entries: {appData.Count}");
                    foreach (var kvp in appData)
                    {
                        AppendResult($"  {kvp.Key}: {kvp.Value}");
                    }
                }

                AppendResult("Protection information retrieved successfully.");
            }
            catch (Exception ex)
            {
                AppendResult($"Error getting protection info: {ex.Message}");
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
                var connectionInfo = CreateConnectionInfo();
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

