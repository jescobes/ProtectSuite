using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mip.CppBridge.WinForms
{
    public class MainForm : Form
    {
        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_init();

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_protect([MarshalAs(UnmanagedType.LPWStr)] string inFile, [MarshalAs(UnmanagedType.LPWStr)] string outFile);

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_unprotect([MarshalAs(UnmanagedType.LPWStr)] string inFile, [MarshalAs(UnmanagedType.LPWStr)] string outFile);

        [DllImport("Mip.NativeBridge.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int mip_getinfo([MarshalAs(UnmanagedType.LPWStr)] string inFile, StringBuilder info, int capacity);

        private TextBox _file;
        private TextBox _log;

        public MainForm()
        {
            Text = "MIP C++ Bridge - WinForms";
            Width = 600; Height = 420; StartPosition = FormStartPosition.CenterScreen;

            var btnBrowse = new Button { Text = "Browse...", Left = 480, Top = 20, Width = 80 };
            var lblFile = new Label { Text = "File:", Left = 20, Top = 4, Width = 80 };
            _file = new TextBox { Left = 20, Top = 22, Width = 450, ReadOnly = true };

            var btnProtect = new Button { Text = "Protect", Left = 20, Top = 60, Width = 90 };
            var btnUnprotect = new Button { Text = "Unprotect", Left = 120, Top = 60, Width = 90 };
            var btnInfo = new Button { Text = "Get Info", Left = 220, Top = 60, Width = 90 };

            var lblLog = new Label { Text = "Result:", Left = 20, Top = 95, Width = 80 };
            _log = new TextBox { Left = 20, Top = 115, Width = 540, Height = 250, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };

            Controls.Add(lblFile); Controls.Add(_file); Controls.Add(btnBrowse);
            Controls.Add(btnProtect); Controls.Add(btnUnprotect); Controls.Add(btnInfo);
            Controls.Add(lblLog); Controls.Add(_log);

            btnBrowse.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "All Files (*.*)|*.*" };
                if (ofd.ShowDialog(this) == DialogResult.OK) { _file.Text = ofd.FileName; Log($"Selected: {ofd.FileName}"); }
            };

            btnProtect.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_file.Text)) { Log("Select a file first."); return; }
                var outFile = System.IO.Path.ChangeExtension(_file.Text, ".mip.pfile");
                var rc = mip_protect(_file.Text, outFile);
                Log(rc == 0 ? $"Protected -> {outFile}" : $"Protect failed rc={rc}");
            };

            btnUnprotect.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_file.Text)) { Log("Select a file first."); return; }
                var outFile = System.IO.Path.ChangeExtension(_file.Text, ".mip.unprot");
                var rc = mip_unprotect(_file.Text, outFile);
                Log(rc == 0 ? $"Unprotected -> {outFile}" : $"Unprotect failed rc={rc}");
            };

            btnInfo.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_file.Text)) { Log("Select a file first."); return; }
                var sb = new StringBuilder(2048);
                var rc = mip_getinfo(_file.Text, sb, sb.Capacity);
                Log(rc == 0 ? sb.ToString() : $"GetInfo failed rc={rc}");
            };

            try
            {
                var rc = mip_init();
                Log(rc == 0 ? "MIP bridge initialized" : $"MIP bridge init failed rc={rc}");
            }
            catch (Exception ex)
            {
                Log($"Init error: {ex.Message}");
            }
        }

        private void Log(string msg)
        {
            _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
        }
    }
}


