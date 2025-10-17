using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Msipc.CSharp.WinForms
{
    public partial class UserRightsEditorDialog : Form
    {
        private ListView lstUsers;
        private TextBox txtUser;
        private CheckBox chkView;
        private CheckBox chkEdit;
        private CheckBox chkPrint;
        private CheckBox chkExport;
        private CheckBox chkExtract;
        private CheckBox chkComment;
        private CheckBox chkForward;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnOK;
        private Button btnCancel;
        private Label lblUser;
        private Label lblRights;
        private GroupBox grpAddUser;

        public List<Microsoft.InformationProtectionAndControl.UserRights> UserRightsList { get; private set; }

        public UserRightsEditorDialog()
        {
            InitializeComponent();
            UserRightsList = new List<Microsoft.InformationProtectionAndControl.UserRights>();
        }

        private void InitializeComponent()
        {
            this.Text = "Edit User Rights";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Users list
            var lblUsers = new Label();
            lblUsers.Text = "Users and Rights:";
            lblUsers.Location = new Point(12, 12);
            lblUsers.Size = new Size(120, 20);

            this.lstUsers = new ListView();
            this.lstUsers.Location = new Point(12, 35);
            this.lstUsers.Size = new Size(350, 300);
            this.lstUsers.View = View.Details;
            this.lstUsers.FullRowSelect = true;
            this.lstUsers.GridLines = true;
            this.lstUsers.Columns.Add("User", 150);
            this.lstUsers.Columns.Add("Rights", 180);
            this.lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;

            // Add user group
            this.grpAddUser = new GroupBox();
            this.grpAddUser.Text = "Add User";
            this.grpAddUser.Location = new Point(380, 35);
            this.grpAddUser.Size = new Size(200, 300);

            this.lblUser = new Label();
            this.lblUser.Text = "User (email or ANYONE):";
            this.lblUser.Location = new Point(10, 25);
            this.lblUser.Size = new Size(180, 20);

            this.txtUser = new TextBox();
            this.txtUser.Location = new Point(10, 45);
            this.txtUser.Size = new Size(180, 22);
            this.txtUser.Text = "ANYONE";

            this.lblRights = new Label();
            this.lblRights.Text = "Rights:";
            this.lblRights.Location = new Point(10, 75);
            this.lblRights.Size = new Size(50, 20);

            this.chkView = new CheckBox();
            this.chkView.Text = "VIEW";
            this.chkView.Location = new Point(10, 95);
            this.chkView.Size = new Size(80, 20);
            this.chkView.Checked = true;

            this.chkEdit = new CheckBox();
            this.chkEdit.Text = "EDIT";
            this.chkEdit.Location = new Point(10, 115);
            this.chkEdit.Size = new Size(80, 20);

            this.chkPrint = new CheckBox();
            this.chkPrint.Text = "PRINT";
            this.chkPrint.Location = new Point(10, 135);
            this.chkPrint.Size = new Size(80, 20);

            this.chkExport = new CheckBox();
            this.chkExport.Text = "EXPORT";
            this.chkExport.Location = new Point(10, 155);
            this.chkExport.Size = new Size(80, 20);

            this.chkExtract = new CheckBox();
            this.chkExtract.Text = "EXTRACT";
            this.chkExtract.Location = new Point(10, 175);
            this.chkExtract.Size = new Size(80, 20);

            this.chkComment = new CheckBox();
            this.chkComment.Text = "COMMENT";
            this.chkComment.Location = new Point(10, 195);
            this.chkComment.Size = new Size(80, 20);

            this.chkForward = new CheckBox();
            this.chkForward.Text = "FORWARD";
            this.chkForward.Location = new Point(10, 215);
            this.chkForward.Size = new Size(80, 20);

            this.btnAdd = new Button();
            this.btnAdd.Text = "Add User";
            this.btnAdd.Location = new Point(10, 245);
            this.btnAdd.Size = new Size(80, 25);
            this.btnAdd.Click += BtnAdd_Click;

            this.btnRemove = new Button();
            this.btnRemove.Text = "Remove";
            this.btnRemove.Location = new Point(100, 245);
            this.btnRemove.Size = new Size(80, 25);
            this.btnRemove.Enabled = false;
            this.btnRemove.Click += BtnRemove_Click;

            this.grpAddUser.Controls.Add(this.lblUser);
            this.grpAddUser.Controls.Add(this.txtUser);
            this.grpAddUser.Controls.Add(this.lblRights);
            this.grpAddUser.Controls.Add(this.chkView);
            this.grpAddUser.Controls.Add(this.chkEdit);
            this.grpAddUser.Controls.Add(this.chkPrint);
            this.grpAddUser.Controls.Add(this.chkExport);
            this.grpAddUser.Controls.Add(this.chkExtract);
            this.grpAddUser.Controls.Add(this.chkComment);
            this.grpAddUser.Controls.Add(this.chkForward);
            this.grpAddUser.Controls.Add(this.btnAdd);
            this.grpAddUser.Controls.Add(this.btnRemove);

            // Buttons
            this.btnOK = new Button();
            this.btnOK.Text = "OK";
            this.btnOK.Location = new Point(420, 350);
            this.btnOK.Size = new Size(75, 25);
            this.btnOK.DialogResult = DialogResult.OK;

            this.btnCancel = new Button();
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new Point(500, 350);
            this.btnCancel.Size = new Size(75, 25);
            this.btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblUsers);
            this.Controls.Add(this.lstUsers);
            this.Controls.Add(this.grpAddUser);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);

            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var user = this.txtUser.Text.Trim();
            if (string.IsNullOrEmpty(user))
            {
                MessageBox.Show("Please enter a user email or ANYONE", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rights = new System.Collections.ObjectModel.Collection<string>();
            if (this.chkView.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ViewRight);
            if (this.chkEdit.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.EditRight);
            if (this.chkPrint.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.PrintRight);
            if (this.chkExport.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ExportRight);
            if (this.chkExtract.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ExtractRight);
            if (this.chkComment.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.CommentRight);
            if (this.chkForward.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ForwardRight);

            if (rights.Count == 0)
            {
                MessageBox.Show("Please select at least one right", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userIdType = user.Equals("ANYONE", StringComparison.OrdinalIgnoreCase) ? Microsoft.InformationProtectionAndControl.UserIdType.IpcUser : Microsoft.InformationProtectionAndControl.UserIdType.Email;
            var userRights = new Microsoft.InformationProtectionAndControl.UserRights(userIdType, user, rights);

            this.UserRightsList.Add(userRights);
            RefreshUserList();

            // Clear form
            this.txtUser.Text = "ANYONE";
            this.chkView.Checked = true;
            this.chkEdit.Checked = false;
            this.chkPrint.Checked = false;
            this.chkExport.Checked = false;
            this.chkExtract.Checked = false;
            this.chkComment.Checked = false;
            this.chkForward.Checked = false;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (this.lstUsers.SelectedItems.Count > 0)
            {
                var index = this.lstUsers.SelectedIndices[0];
                this.UserRightsList.RemoveAt(index);
                RefreshUserList();
            }
        }

        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnRemove.Enabled = this.lstUsers.SelectedItems.Count > 0;
        }

        private void RefreshUserList()
        {
            this.lstUsers.Items.Clear();
            foreach (var userRights in this.UserRightsList)
            {
                var rightsText = string.Join(", ", userRights.Rights);
                var item = new ListViewItem(userRights.UserId);
                item.SubItems.Add(rightsText);
                this.lstUsers.Items.Add(item);
            }
        }

        public void LoadExistingRights(List<Microsoft.InformationProtectionAndControl.UserRights> existingRights)
        {
            this.UserRightsList.Clear();
            if (existingRights != null)
            {
                this.UserRightsList.AddRange(existingRights);
            }
            RefreshUserList();
        }
    }
}
