using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

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
        private CheckBox chkFullControl;
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
            this.lstUsers.Size = new Size(350, 280);
            this.lstUsers.View = View.Details;
            this.lstUsers.FullRowSelect = true;
            this.lstUsers.GridLines = true;
            this.lstUsers.Columns.Add("User", 120);
            this.lstUsers.Columns.Add("Rights", 220);
            this.lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;

            // Add user group
            this.grpAddUser = new GroupBox();
            this.grpAddUser.Text = "Add User";
            this.grpAddUser.Location = new Point(380, 35);
            this.grpAddUser.Size = new Size(200, 295);

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

            this.chkFullControl = new CheckBox();
            this.chkFullControl.Text = "FULL CONTROL";
            this.chkFullControl.Location = new Point(10, 235);
            this.chkFullControl.Size = new Size(120, 20);
            this.chkFullControl.Checked = false;

            this.btnAdd = new Button();
            this.btnAdd.Text = "Add User";
            this.btnAdd.Location = new Point(10, 260);
            this.btnAdd.Size = new Size(80, 25);
            this.btnAdd.Click += BtnAdd_Click;

            this.btnRemove = new Button();
            this.btnRemove.Text = "Remove";
            this.btnRemove.Location = new Point(100, 260);
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
            this.grpAddUser.Controls.Add(this.chkFullControl);
            this.grpAddUser.Controls.Add(this.btnAdd);
            this.grpAddUser.Controls.Add(this.btnRemove);

            // Save/Load buttons
            var btnSave = new Button();
            btnSave.Text = "Save...";
            btnSave.Location = new Point(12, 325);
            btnSave.Size = new Size(80, 25);
            btnSave.Click += BtnSave_Click;

            var btnLoad = new Button();
            btnLoad.Text = "Load...";
            btnLoad.Location = new Point(100, 325);
            btnLoad.Size = new Size(80, 25);
            btnLoad.Click += BtnLoad_Click;

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
            this.Controls.Add(btnSave);
            this.Controls.Add(btnLoad);
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
            if (this.chkEdit.Checked)
            {
                rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.EditRight);
                // Add DOCEDIT and SAVE when EDIT is selected
                rights.Add("DOCEDIT");
                rights.Add("SAVE");
            }
            if (this.chkPrint.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.PrintRight);
            if (this.chkExport.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ExportRight);
            if (this.chkExtract.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ExtractRight);
            if (this.chkComment.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.CommentRight);
            if (this.chkForward.Checked) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ForwardRight);
            bool hasOwner = this.chkFullControl.Checked;
            if (hasOwner) rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.OwnerRight);
            
            // Add VIEWRIGHTSDATA always, except when OWNER is selected (OWNER already includes it)
            if (!hasOwner)
            {
                rights.Add(Microsoft.InformationProtectionAndControl.CommonRights.ViewRightsDataRight);
            }

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
            this.chkFullControl.Checked = false;
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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (this.UserRightsList.Count == 0)
            {
                MessageBox.Show("No user rights to save.", "Save User Rights", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.DefaultExt = "json";
                saveDialog.FileName = "custom_rights.json";
                saveDialog.Title = "Save Custom User Rights";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var rightsData = new List<UserRightsData>();
                        foreach (var ur in this.UserRightsList)
                        {
                            rightsData.Add(new UserRightsData
                            {
                                UserId = ur.UserId,
                                UserIdType = ur.UserIdType.ToString(),
                                Rights = ur.Rights.ToList()
                            });
                        }

                        string json = JsonConvert.SerializeObject(rightsData, Formatting.Indented);
                        File.WriteAllText(saveDialog.FileName, json);
                        MessageBox.Show($"User rights saved successfully to:\n{saveDialog.FileName}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving user rights: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openDialog.FilterIndex = 1;
                openDialog.DefaultExt = "json";
                openDialog.FileName = "custom_rights.json";
                openDialog.Title = "Load Custom User Rights";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = File.ReadAllText(openDialog.FileName);
                        var rightsData = JsonConvert.DeserializeObject<List<UserRightsData>>(json);

                        if (rightsData == null || rightsData.Count == 0)
                        {
                            MessageBox.Show("The file does not contain valid user rights data.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Ask user if they want to replace or append
                        var result = MessageBox.Show(
                            $"Load {rightsData.Count} user right(s) from file?\n\n" +
                            "Yes = Replace current rights\n" +
                            "No = Append to current rights\n" +
                            "Cancel = Do nothing",
                            "Load User Rights",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }

                        if (result == DialogResult.Yes)
                        {
                            this.UserRightsList.Clear();
                        }

                        foreach (var data in rightsData)
                        {
                            try
                            {
                                var userIdType = (Microsoft.InformationProtectionAndControl.UserIdType)Enum.Parse(
                                    typeof(Microsoft.InformationProtectionAndControl.UserIdType),
                                    data.UserIdType);
                                
                                var rightsCollection = new System.Collections.ObjectModel.Collection<string>(data.Rights);
                                var userRights = new Microsoft.InformationProtectionAndControl.UserRights(userIdType, data.UserId, rightsCollection);
                                this.UserRightsList.Add(userRights);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error loading user rights for {data.UserId}: {ex.Message}", "Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        RefreshUserList();
                        MessageBox.Show($"Loaded {rightsData.Count} user right(s) successfully.", "Load Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading user rights: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Helper class for JSON serialization
        private class UserRightsData
        {
            public string UserId { get; set; }
            public string UserIdType { get; set; }
            public List<string> Rights { get; set; }
        }
    }
}
