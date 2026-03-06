using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mip.CSharp.WinForms
{
    /// <summary>
    /// Dialog to pick or enter Template ID and Label ID for MIP protection.
    /// MIP SDK uses template and label for protection (no server template list in this bridge).
    /// </summary>
    public partial class MipTemplatePickerDialog : Form
    {
        private TextBox txtTemplateId;
        private TextBox txtLabelId;
        private Button btnOK;
        private Button btnCancel;
        private Label lblTemplateId;
        private Label lblLabelId;

        public string TemplateId => txtTemplateId?.Text?.Trim() ?? string.Empty;
        public string LabelId => txtLabelId?.Text?.Trim() ?? string.Empty;

        public MipTemplatePickerDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Protection Template (MIP)";
            this.Size = new Size(450, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.lblTemplateId = new Label
            {
                Text = "Template ID (optional):",
                Location = new Point(12, 20),
                Size = new Size(140, 20)
            };

            this.txtTemplateId = new TextBox
            {
                Location = new Point(12, 42),
                Size = new Size(400, 22)
            };

            this.lblLabelId = new Label
            {
                Text = "Label ID (optional):",
                Location = new Point(12, 72),
                Size = new Size(140, 20)
            };

            this.txtLabelId = new TextBox
            {
                Location = new Point(12, 94),
                Size = new Size(400, 22)
            };

            this.btnOK = new Button
            {
                Text = "OK",
                Location = new Point(250, 130),
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };

            this.btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(335, 130),
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(this.lblTemplateId);
            this.Controls.Add(this.txtTemplateId);
            this.Controls.Add(this.lblLabelId);
            this.Controls.Add(this.txtLabelId);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);

            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
        }
    }
}
