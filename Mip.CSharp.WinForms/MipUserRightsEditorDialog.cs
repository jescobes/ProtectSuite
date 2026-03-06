using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Mip.CSharp.WinForms
{
    /// <summary>
    /// Simple DTO for MIP custom rights (UI parity with Msipc; MIP protection uses template/label).
    /// </summary>
    public class MipUserRights
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Rights { get; set; } = new List<string>();
    }

    /// <summary>
    /// Dialog to edit user rights for MIP. Same UI as Msipc; actual MIP protection uses template/label.
    /// </summary>
    public partial class MipUserRightsEditorDialog : Form
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

        public List<MipUserRights> UserRightsList { get; private set; }

        private static readonly string ViewRight = "VIEW";
        private static readonly string EditRight = "EDIT";
        private static readonly string PrintRight = "PRINT";
        private static readonly string ExportRight = "EXPORT";
        private static readonly string ExtractRight = "EXTRACT";
        private static readonly string CommentRight = "COMMENT";
        private static readonly string ForwardRight = "FORWARD";
        private static readonly string OwnerRight = "OWNER";
        private static readonly string ViewRightsDataRight = "VIEWRIGHTSDATA";
        private static readonly string DocEditRight = "DOCEDIT";
        private static readonly string SaveRight = "SAVE";

        public MipUserRightsEditorDialog()
        {
            InitializeComponent();
            UserRightsList = new List<MipUserRights>();
        }

        private void InitializeComponent()
        {
            this.Text = "Edit User Rights (MIP)";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblUsers = new Label
            {
                Text = "Users and Rights:",
                Location = new Point(12, 12),
                Size = new Size(120, 20)
            };

            this.lstUsers = new ListView
            {
                Location = new Point(12, 35),
                Size = new Size(350, 280),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            this.lstUsers.Columns.Add("User", 120);
            this.lstUsers.Columns.Add("Rights", 220);
            this.lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;

            this.grpAddUser = new GroupBox
            {
                Text = "Add User",
                Location = new Point(380, 35),
                Size = new Size(200, 295)
            };

            this.lblUser = new Label { Text = "User (email or ANYONE):", Location = new Point(10, 25), Size = new Size(180, 20) };
            this.txtUser = new TextBox { Location = new Point(10, 45), Size = new Size(180, 22), Text = "ANYONE" };
            this.lblRights = new Label { Text = "Rights:", Location = new Point(10, 75), Size = new Size(50, 20) };

            this.chkView = new CheckBox { Text = "VIEW", Location = new Point(10, 95), Size = new Size(80, 20), Checked = true };
            this.chkEdit = new CheckBox { Text = "EDIT", Location = new Point(10, 115), Size = new Size(80, 20) };
            this.chkPrint = new CheckBox { Text = "PRINT", Location = new Point(10, 135), Size = new Size(80, 20) };
            this.chkExport = new CheckBox { Text = "EXPORT", Location = new Point(10, 155), Size = new Size(80, 20) };
            this.chkExtract = new CheckBox { Text = "EXTRACT", Location = new Point(10, 175), Size = new Size(80, 20) };
            this.chkComment = new CheckBox { Text = "COMMENT", Location = new Point(10, 195), Size = new Size(80, 20) };
            this.chkForward = new CheckBox { Text = "FORWARD", Location = new Point(10, 215), Size = new Size(80, 20) };
            this.chkFullControl = new CheckBox { Text = "FULL CONTROL", Location = new Point(10, 235), Size = new Size(120, 20) };

            this.btnAdd = new Button { Text = "Add User", Location = new Point(10, 260), Size = new Size(80, 25) };
            this.btnAdd.Click += BtnAdd_Click;
            this.btnRemove = new Button { Text = "Remove", Location = new Point(100, 260), Size = new Size(80, 25), Enabled = false };
            this.btnRemove.Click += BtnRemove_Click;

            this.grpAddUser.Controls.AddRange(new Control[] {
                this.lblUser, this.txtUser, this.lblRights,
                this.chkView, this.chkEdit, this.chkPrint, this.chkExport, this.chkExtract,
                this.chkComment, this.chkForward, this.chkFullControl, this.btnAdd, this.btnRemove
            });

            var btnSave = new Button { Text = "Save...", Location = new Point(12, 325), Size = new Size(80, 25) };
            btnSave.Click += BtnSave_Click;
            var btnLoad = new Button { Text = "Load...", Location = new Point(100, 325), Size = new Size(80, 25) };
            btnLoad.Click += BtnLoad_Click;

            this.btnOK = new Button { Text = "OK", Location = new Point(420, 350), Size = new Size(75, 25), DialogResult = DialogResult.OK };
            this.btnCancel = new Button { Text = "Cancel", Location = new Point(500, 350), Size = new Size(75, 25), DialogResult = DialogResult.Cancel };

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

            var rights = new List<string>();
            if (chkView.Checked) rights.Add(ViewRight);
            if (chkEdit.Checked) { rights.Add(EditRight); rights.Add(DocEditRight); rights.Add(SaveRight); }
            if (chkPrint.Checked) rights.Add(PrintRight);
            if (chkExport.Checked) rights.Add(ExportRight);
            if (chkExtract.Checked) rights.Add(ExtractRight);
            if (chkComment.Checked) rights.Add(CommentRight);
            if (chkForward.Checked) rights.Add(ForwardRight);
            if (chkFullControl.Checked) rights.Add(OwnerRight);
            if (!chkFullControl.Checked) rights.Add(ViewRightsDataRight);

            if (rights.Count == 0)
            {
                MessageBox.Show("Please select at least one right", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UserRightsList.Add(new MipUserRights { UserId = user, Rights = rights });
            RefreshUserList();

            txtUser.Text = "ANYONE";
            chkView.Checked = true;
            chkEdit.Checked = chkPrint.Checked = chkExport.Checked = chkExtract.Checked = chkComment.Checked = chkForward.Checked = chkFullControl.Checked = false;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItems.Count > 0)
            {
                UserRightsList.RemoveAt(lstUsers.SelectedIndices[0]);
                RefreshUserList();
            }
        }

        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = lstUsers.SelectedItems.Count > 0;
        }

        private void RefreshUserList()
        {
            lstUsers.Items.Clear();
            foreach (var ur in UserRightsList)
            {
                var item = new ListViewItem(ur.UserId);
                item.SubItems.Add(string.Join(", ", ur.Rights));
                lstUsers.Items.Add(item);
            }
        }

        public void LoadExistingRights(List<MipUserRights> existingRights)
        {
            UserRightsList.Clear();
            if (existingRights != null)
                UserRightsList.AddRange(existingRights);
            RefreshUserList();
        }

        private class UserRightsData
        {
            public string UserId { get; set; }
            public List<string> Rights { get; set; }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (UserRightsList.Count == 0)
            {
                MessageBox.Show("No user rights to save.", "Save User Rights", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var d = new SaveFileDialog())
            {
                d.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                d.DefaultExt = "json";
                d.FileName = "custom_rights.json";
                d.Title = "Save Custom User Rights";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var data = UserRightsList.Select(ur => new UserRightsData { UserId = ur.UserId, Rights = ur.Rights }).ToList();
                        File.WriteAllText(d.FileName, JsonConvert.SerializeObject(data, Formatting.Indented));
                        MessageBox.Show($"Saved to {d.FileName}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog())
            {
                d.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                d.DefaultExt = "json";
                d.Title = "Load Custom User Rights";
                if (d.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var data = JsonConvert.DeserializeObject<List<UserRightsData>>(File.ReadAllText(d.FileName));
                        if (data == null || data.Count == 0) return;
                        var result = MessageBox.Show($"Load {data.Count} user(s)? Yes=Replace, No=Append", "Load", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (result == DialogResult.Cancel) return;
                        if (result == DialogResult.Yes) UserRightsList.Clear();
                        foreach (var x in data)
                            UserRightsList.Add(new MipUserRights { UserId = x.UserId ?? "", Rights = x.Rights ?? new List<string>() });
                        RefreshUserList();
                        MessageBox.Show($"Loaded {data.Count} user(s).", "Load Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }
    }
}
