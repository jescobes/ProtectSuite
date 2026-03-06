using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mip.CSharp.WinForms
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
        private List<MipUserRights> customUserRights = new List<MipUserRights>();

        public enum BackendType { Purview, AdRms }

        public MainForm()
        {
            InitializeComponent();
            InitializeMip();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "MIP C# - File Protection";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 600);

            this.lblFile = new Label { Text = "Select File:", Location = new Point(20, 20), Size = new Size(80, 20) };
            this.txtFilePath = new TextBox { Location = new Point(20, 45), Size = new Size(400, 20), ReadOnly = true };
            this.btnSelectFile = new Button { Text = "Browse...", Location = new Point(430, 43), Size = new Size(80, 25) };
            this.btnSelectFile.Click += BtnSelectFile_Click;

            this.lblBackend = new Label { Text = "Backend:", Location = new Point(20, 80), Size = new Size(80, 20) };
            this.rbPurview = new RadioButton { Text = "Microsoft Purview", Location = new Point(20, 105), Size = new Size(150, 20), Checked = true };
            this.rbAdRms = new RadioButton { Text = "AD RMS Server", Location = new Point(180, 105), Size = new Size(150, 20) };
            this.rbPurview.CheckedChanged += RbBackend_CheckedChanged;
            this.rbAdRms.CheckedChanged += RbBackend_CheckedChanged;

            this.grpProtectionMode = new GroupBox { Text = "Protection Mode", Location = new Point(20, 135), Size = new Size(540, 65) };
            this.rdoTemplate = new RadioButton { Text = "Template based", Location = new Point(15, 28), Size = new Size(130, 20), Checked = true };
            this.rdoCustom = new RadioButton { Text = "Custom rights", Location = new Point(160, 28), Size = new Size(120, 20) };
            this.grpProtectionMode.Controls.Add(this.rdoTemplate);
            this.grpProtectionMode.Controls.Add(this.rdoCustom);

            this.grpCustom = new GroupBox { Text = "Custom Rights", Location = new Point(20, 205), Size = new Size(540, 70) };
            this.lblCustomRights = new Label { Text = "No users configured", Location = new Point(12, 25), Size = new Size(400, 20) };
            this.btnEditCustomRights = new Button { Text = "Edit User Rights...", Location = new Point(12, 45), Size = new Size(120, 25) };
            this.btnEditCustomRights.Click += BtnEditCustomRights_Click;
            this.grpCustom.Controls.Add(this.lblCustomRights);
            this.grpCustom.Controls.Add(this.btnEditCustomRights);

            this.lblActions = new Label { Text = "Actions:", Location = new Point(20, 285), Size = new Size(80, 20) };
            this.btnProtect = new Button { Text = "Protect File", Location = new Point(20, 310), Size = new Size(100, 30), Enabled = false };
            this.btnProtect.Click += BtnProtect_Click;
            this.btnUnprotect = new Button { Text = "Unprotect File", Location = new Point(130, 310), Size = new Size(100, 30), Enabled = false };
            this.btnUnprotect.Click += BtnUnprotect_Click;
            this.btnGetInfo = new Button { Text = "Get Protection Info", Location = new Point(240, 310), Size = new Size(120, 30), Enabled = false };
            this.btnGetInfo.Click += BtnGetInfo_Click;
            this.btnViewContent = new Button { Text = "View Content", Location = new Point(370, 310), Size = new Size(100, 30), Enabled = false };
            this.btnViewContent.Click += BtnViewContent_Click;

            this.lblResult = new Label { Text = "Result:", Location = new Point(20, 350), Size = new Size(80, 20) };
            this.txtResult = new TextBox { Location = new Point(20, 375), Size = new Size(540, 180), Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };

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

            this.FormClosing += MainForm_FormClosing;
            this.ResumeLayout(false);
        }

        private void InitializeMip()
        {
            try
            {
                int rc = MipBridgeNativeMethods.mip_init();
                if (rc == 0)
                    AppendResult("MIP bridge initialized successfully.");
                else
                    AppendResult($"MIP bridge initialization failed with error code: {rc}");
            }
            catch (Exception ex)
            {
                AppendResult($"Error initializing MIP bridge: {ex.Message}");
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "All Files (*.*)|*.*", Title = "Select File to Protect/Unprotect" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = ofd.FileName;
                    txtFilePath.Text = selectedFilePath;
                    AppendResult($"Selected file: {selectedFilePath}");
                    UpdateUIForFileState();
                }
            }
        }

        private void UpdateUIForFileState()
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                btnProtect.Enabled = false;
                btnUnprotect.Enabled = false;
                btnGetInfo.Enabled = false;
                btnViewContent.Enabled = false;
                return;
            }

            bool isProtected = IsFileProtectedMip(selectedFilePath);
            btnViewContent.Enabled = true;
            if (isProtected)
            {
                btnProtect.Enabled = false;
                btnUnprotect.Enabled = true;
                btnGetInfo.Enabled = true;
                AppendResult("File appears protected. Unprotect/Get Info enabled.");
            }
            else
            {
                btnProtect.Enabled = true;
                btnUnprotect.Enabled = false;
                btnGetInfo.Enabled = false;
                AppendResult("File is not protected. Protect enabled.");
            }
        }

        private static bool IsFileProtectedMip(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                var sb = new StringBuilder(1024);
                int rc = MipBridgeNativeMethods.mip_getinfo(filePath, sb, sb.Capacity);
                return rc == 0 && !string.IsNullOrWhiteSpace(sb.ToString());
            }
            catch { return false; }
        }

        private void RbBackend_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null || !rb.Checked) return;
            if (rbPurview.Checked) selectedBackend = BackendType.Purview;
            else if (rbAdRms.Checked) selectedBackend = BackendType.AdRms;
            else return;
            AppendResult($"Backend: {selectedBackend}");
        }

        private void BtnEditCustomRights_Click(object sender, EventArgs e)
        {
            using (var dlg = new MipUserRightsEditorDialog())
            {
                dlg.LoadExistingRights(new List<MipUserRights>(customUserRights));
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    customUserRights = new List<MipUserRights>(dlg.UserRightsList);
                    if (customUserRights.Count == 0)
                        lblCustomRights.Text = "No users configured";
                    else
                        lblCustomRights.Text = $"{customUserRights.Count} user(s) configured";
                }
            }
        }

        private void BtnProtect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                AppendResult("Please select a valid file first.");
                return;
            }

            string templateId = null;
            string labelId = null;

            if (rdoTemplate.Checked)
            {
                using (var dlg = new MipTemplatePickerDialog())
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    templateId = string.IsNullOrWhiteSpace(dlg.TemplateId) ? null : dlg.TemplateId;
                    labelId = string.IsNullOrWhiteSpace(dlg.LabelId) ? null : dlg.LabelId;
                }
            }
            else
            {
                if (customUserRights.Count == 0)
                {
                    AppendResult("No custom user rights configured. Use 'Edit User Rights...' or switch to Template based.");
                    return;
                }
                AppendResult("Custom rights configured; MIP protection uses template/label. Opening template picker...");
                using (var dlg = new MipTemplatePickerDialog())
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    templateId = string.IsNullOrWhiteSpace(dlg.TemplateId) ? null : dlg.TemplateId;
                    labelId = string.IsNullOrWhiteSpace(dlg.LabelId) ? null : dlg.LabelId;
                }
            }

            try
            {
                AppendResult("Protecting file...");
                string outFile = Path.ChangeExtension(selectedFilePath, ".mip.pfile");
                int rc = MipBridgeNativeMethods.mip_protect(selectedFilePath, outFile, templateId, labelId);
                if (rc == 0)
                {
                    AppendResult($"Protected file written: {outFile}");
                    selectedFilePath = outFile;
                    txtFilePath.Text = selectedFilePath;
                    UpdateUIForFileState();
                }
                else
                    AppendResult($"Protection failed with error code: {rc}");
            }
            catch (Exception ex)
            {
                AppendResult($"Error protecting file: {ex.Message}");
            }
        }

        private void BtnUnprotect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }
            try
            {
                AppendResult("Unprotecting file...");
                string outFile = Path.ChangeExtension(selectedFilePath, ".mip.unprot");
                int rc = MipBridgeNativeMethods.mip_unprotect(selectedFilePath, outFile);
                if (rc == 0)
                {
                    AppendResult($"Unprotected file written: {outFile}");
                    selectedFilePath = outFile;
                    txtFilePath.Text = selectedFilePath;
                    UpdateUIForFileState();
                }
                else
                    AppendResult($"Unprotection failed with error code: {rc}");
            }
            catch (Exception ex)
            {
                AppendResult($"Error unprotecting file: {ex.Message}");
            }
        }

        private void BtnGetInfo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }
            try
            {
                AppendResult("Getting protection information...");
                var sb = new StringBuilder(4096);
                int rc = MipBridgeNativeMethods.mip_getinfo(selectedFilePath, sb, sb.Capacity);
                if (rc == 0)
                {
                    AppendResult("=== Protection Information (MIP) ===");
                    AppendResult(sb.ToString());
                }
                else
                    AppendResult($"GetInfo failed with error code: {rc}");
            }
            catch (Exception ex)
            {
                AppendResult($"Error getting protection info: {ex.Message}");
            }
        }

        private void BtnViewContent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }
            try
            {
                AppendResult("Opening file for viewing...");
                Process.Start(new ProcessStartInfo(selectedFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AppendResult($"Error opening file: {ex.Message}");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { MipBridgeNativeMethods.mip_cleanup(); }
            catch { }
        }

        private void AppendResult(string message)
        {
            if (txtResult.InvokeRequired)
            {
                txtResult.Invoke(new Action<string>(AppendResult), message);
                return;
            }
            txtResult.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtResult.SelectionStart = txtResult.Text.Length;
            txtResult.ScrollToCaret();
        }
    }
}
