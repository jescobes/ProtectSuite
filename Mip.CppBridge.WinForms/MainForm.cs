using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Mip.CppBridge.WinForms
{
    public partial class MainForm : Form
    {
        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_init();

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_protect(
            [MarshalAs(UnmanagedType.LPWStr)] string inFile,
            [MarshalAs(UnmanagedType.LPWStr)] string outFile,
            [MarshalAs(UnmanagedType.LPWStr)] string templateId,
            [MarshalAs(UnmanagedType.LPWStr)] string labelId);

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_unprotect(
            [MarshalAs(UnmanagedType.LPWStr)] string inFile,
            [MarshalAs(UnmanagedType.LPWStr)] string outFile);

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_getinfo(
            [MarshalAs(UnmanagedType.LPWStr)] string inFile,
            StringBuilder info,
            int capacity);

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mip_cleanup();

        private TextBox txtFilePath;
        private TextBox txtResult;
        private TextBox txtTemplateId;
        private TextBox txtLabelId;
        private Button btnSelectFile;
        private Button btnProtect;
        private Button btnUnprotect;
        private Button btnGetInfo;
        private Label lblFile;
        private Label lblResult;
        private Label lblTemplateId;
        private Label lblLabelId;
        private GroupBox grpProtection;

        private string selectedFilePath;

        public MainForm()
        {
            InitializeComponent();
            InitializeMip();
        }

        private void InitializeComponent()
        {
            this.Text = "MIP C++ Bridge - WinForms";
            this.Width = 700;
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // File selection
            this.lblFile = new Label
            {
                Text = "File:",
                Left = 20,
                Top = 20,
                Width = 80,
                Height = 20
            };

            this.txtFilePath = new TextBox
            {
                Left = 20,
                Top = 45,
                Width = 500,
                Height = 25,
                ReadOnly = true
            };

            this.btnSelectFile = new Button
            {
                Text = "Browse...",
                Left = 530,
                Top = 43,
                Width = 100,
                Height = 30
            };

            // Protection options
            this.grpProtection = new GroupBox
            {
                Text = "Protection Options",
                Left = 20,
                Top = 85,
                Width = 610,
                Height = 100
            };

            this.lblTemplateId = new Label
            {
                Text = "Template ID (optional):",
                Left = 10,
                Top = 25,
                Width = 150,
                Height = 20
            };

            this.txtTemplateId = new TextBox
            {
                Left = 10,
                Top = 50,
                Width = 280,
                Height = 25
            };

            this.lblLabelId = new Label
            {
                Text = "Label ID (optional):",
                Left = 310,
                Top = 25,
                Width = 150,
                Height = 20
            };

            this.txtLabelId = new TextBox
            {
                Left = 310,
                Top = 50,
                Width = 280,
                Height = 25
            };

            this.grpProtection.Controls.Add(this.lblTemplateId);
            this.grpProtection.Controls.Add(this.txtTemplateId);
            this.grpProtection.Controls.Add(this.lblLabelId);
            this.grpProtection.Controls.Add(this.txtLabelId);

            // Action buttons
            this.btnProtect = new Button
            {
                Text = "Protect",
                Left = 20,
                Top = 200,
                Width = 120,
                Height = 35
            };

            this.btnUnprotect = new Button
            {
                Text = "Unprotect",
                Left = 150,
                Top = 200,
                Width = 120,
                Height = 35
            };

            this.btnGetInfo = new Button
            {
                Text = "Get Info",
                Left = 280,
                Top = 200,
                Width = 120,
                Height = 35
            };

            // Result log
            this.lblResult = new Label
            {
                Text = "Result:",
                Left = 20,
                Top = 250,
                Width = 80,
                Height = 20
            };

            this.txtResult = new TextBox
            {
                Left = 20,
                Top = 275,
                Width = 610,
                Height = 200,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            // Add controls to form
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.grpProtection);
            this.Controls.Add(this.btnProtect);
            this.Controls.Add(this.btnUnprotect);
            this.Controls.Add(this.btnGetInfo);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.txtResult);

            // Event handlers
            this.btnSelectFile.Click += BtnSelectFile_Click;
            this.btnProtect.Click += BtnProtect_Click;
            this.btnUnprotect.Click += BtnUnprotect_Click;
            this.btnGetInfo.Click += BtnGetInfo_Click;
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeMip()
        {
            try
            {
                int rc = mip_init();
                if (rc == 0)
                {
                    AppendResult("MIP bridge initialized successfully.");
                }
                else
                {
                    AppendResult($"MIP bridge initialization failed with error code: {rc}");
                }
            }
            catch (Exception ex)
            {
                AppendResult($"Error initializing MIP bridge: {ex.Message}");
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Select File"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    this.selectedFilePath = ofd.FileName;
                    this.txtFilePath.Text = ofd.FileName;
                    AppendResult($"Selected file: {ofd.FileName}");
                }
            }
        }

        private void BtnProtect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            if (!File.Exists(this.selectedFilePath))
            {
                AppendResult("Selected file does not exist.");
                return;
            }

            try
            {
                AppendResult("Protecting file...");

                string outFile = Path.ChangeExtension(this.selectedFilePath, ".mip.pfile");
                string templateId = string.IsNullOrWhiteSpace(this.txtTemplateId.Text) ? null : this.txtTemplateId.Text;
                string labelId = string.IsNullOrWhiteSpace(this.txtLabelId.Text) ? null : this.txtLabelId.Text;

                int rc = mip_protect(this.selectedFilePath, outFile, templateId, labelId);

                if (rc == 0)
                {
                    AppendResult($"File protected successfully: {outFile}");
                    if (!string.IsNullOrEmpty(templateId))
                        AppendResult($"  Template ID: {templateId}");
                    if (!string.IsNullOrEmpty(labelId))
                        AppendResult($"  Label ID: {labelId}");
                }
                else
                {
                    AppendResult($"Protection failed with error code: {rc}");
                }
            }
            catch (Exception ex)
            {
                AppendResult($"Error protecting file: {ex.Message}");
            }
        }

        private void BtnUnprotect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            if (!File.Exists(this.selectedFilePath))
            {
                AppendResult("Selected file does not exist.");
                return;
            }

            try
            {
                AppendResult("Unprotecting file...");

                string outFile = Path.ChangeExtension(this.selectedFilePath, ".mip.unprot");

                int rc = mip_unprotect(this.selectedFilePath, outFile);

                if (rc == 0)
                {
                    AppendResult($"File unprotected successfully: {outFile}");
                }
                else
                {
                    AppendResult($"Unprotection failed with error code: {rc}");
                }
            }
            catch (Exception ex)
            {
                AppendResult($"Error unprotected file: {ex.Message}");
            }
        }

        private void BtnGetInfo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.selectedFilePath))
            {
                AppendResult("Please select a file first.");
                return;
            }

            if (!File.Exists(this.selectedFilePath))
            {
                AppendResult("Selected file does not exist.");
                return;
            }

            try
            {
                AppendResult("Getting protection information...");

                StringBuilder sb = new StringBuilder(2048);
                int rc = mip_getinfo(this.selectedFilePath, sb, sb.Capacity);

                if (rc == 0)
                {
                    AppendResult("=== Protection Information ===");
                    AppendResult(sb.ToString());
                }
                else
                {
                    AppendResult($"GetInfo failed with error code: {rc}");
                }
            }
            catch (Exception ex)
            {
                AppendResult($"Error getting file information: {ex.Message}");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mip_cleanup();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private void AppendResult(string message)
        {
            if (this.txtResult.InvokeRequired)
            {
                this.txtResult.Invoke(new Action<string>(AppendResult), message);
                return;
            }

            this.txtResult.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            this.txtResult.SelectionStart = this.txtResult.Text.Length;
            this.txtResult.ScrollToCaret();
        }
    }
}