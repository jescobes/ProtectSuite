using System;
using System.IO;
using Mip.CSharp.WinForms;

namespace Tests.Mip.CSharp.WinForms
{
    /// <summary>
    /// Helper class for MIP bridge unit tests.
    /// </summary>
    public static class TestHelpers
    {
        private static bool _mipInitialized;

        /// <summary>
        /// Initialize MIP bridge (mip_init). Safe to call multiple times.
        /// </summary>
        public static void InitializeMip()
        {
            if (_mipInitialized) return;
            int rc = MipBridgeNativeMethods.mip_init();
            if (rc == 0)
                _mipInitialized = true;
        }

        /// <summary>
        /// Creates a temporary test file with sample content.
        /// </summary>
        public static string CreateTestFile(string content = "Test file content for MIP protection")
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(tempFile, content);
            return tempFile;
        }

        /// <summary>
        /// Cleans up test files.
        /// </summary>
        public static void CleanupTestFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }
    }
}
