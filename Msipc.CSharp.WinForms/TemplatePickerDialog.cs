using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Msipc.CSharp.WinForms
{
    public partial class TemplatePickerDialog : Form
    {
        private ListBox lstTemplates;
        private TextBox txtTemplateInfo;
        private Button btnOK;
        private Button btnCancel;
        private Label lblTemplates;
        private Label lblInfo;

        public Microsoft.InformationProtectionAndControl.TemplateInfo SelectedTemplate { get; private set; }

        public TemplatePickerDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Protection Template";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Templates list
            this.lblTemplates = new Label();
            this.lblTemplates.Text = "Available Templates:";
            this.lblTemplates.Location = new Point(12, 12);
            this.lblTemplates.Size = new Size(120, 20);

            this.lstTemplates = new ListBox();
            this.lstTemplates.Location = new Point(12, 35);
            this.lstTemplates.Size = new Size(220, 280);
            this.lstTemplates.SelectionMode = SelectionMode.One;
            this.lstTemplates.SelectedIndexChanged += LstTemplates_SelectedIndexChanged;

            // Template info
            this.lblInfo = new Label();
            this.lblInfo.Text = "Template Details:";
            this.lblInfo.Location = new Point(250, 12);
            this.lblInfo.Size = new Size(100, 20);

            this.txtTemplateInfo = new TextBox();
            this.txtTemplateInfo.Location = new Point(250, 35);
            this.txtTemplateInfo.Size = new Size(220, 280);
            this.txtTemplateInfo.Multiline = true;
            this.txtTemplateInfo.ScrollBars = ScrollBars.Vertical;
            this.txtTemplateInfo.ReadOnly = true;

            // Buttons
            this.btnOK = new Button();
            this.btnOK.Text = "OK";
            this.btnOK.Location = new Point(320, 330);
            this.btnOK.Size = new Size(75, 25);
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Enabled = false;

            this.btnCancel = new Button();
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new Point(400, 330);
            this.btnCancel.Size = new Size(75, 25);
            this.btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(this.lblTemplates);
            this.Controls.Add(this.lstTemplates);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.txtTemplateInfo);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);

            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
        }

        public void LoadTemplates(Microsoft.InformationProtectionAndControl.ConnectionInfo connectionInfo)
        {
            try
            {
                var templates = Microsoft.InformationProtectionAndControl.SafeNativeMethods.IpcGetTemplateList(
                    connectionInfo,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: false,
                    parentWindow: this,
                    cultureInfo: System.Globalization.CultureInfo.CurrentCulture);

                if (templates != null)
                {
                    this.lstTemplates.Items.Clear();
                    foreach (var template in templates)
                    {
                        this.lstTemplates.Items.Add(template);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading templates: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LstTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lstTemplates.SelectedItem is Microsoft.InformationProtectionAndControl.TemplateInfo template)
            {
                this.SelectedTemplate = template;
                this.btnOK.Enabled = true;

                // Display template details
                var info = $"Name: {template.Name}\n" +
                          $"Description: {template.Description}\n" +
                          $"Template ID: {template.TemplateId}\n" +
                          $"Issuer: {template.IssuerDisplayName}";

                this.txtTemplateInfo.Text = info;
            }
            else
            {
                this.SelectedTemplate = null;
                this.btnOK.Enabled = false;
                this.txtTemplateInfo.Text = "";
            }
        }
    }
}
