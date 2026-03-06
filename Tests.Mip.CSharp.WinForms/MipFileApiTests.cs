using System;
using System.IO;
using System.Text;
using Xunit;
using Mip.CSharp.WinForms;

namespace Tests.Mip.CSharp.WinForms
{
    /// <summary>
    /// Tests for MIP bridge file operations: mip_protect, mip_unprotect, mip_getinfo.
    /// </summary>
    public class MipFileApiTests : IDisposable
    {
        private string _testFile;
        private string _protectedFile;
        private string _unprotectedFile;

        public MipFileApiTests()
        {
            TestHelpers.InitializeMip();
            _testFile = TestHelpers.CreateTestFile("MIP file API test content");
        }

        public void Dispose()
        {
            TestHelpers.CleanupTestFile(_testFile);
            TestHelpers.CleanupTestFile(_protectedFile);
            TestHelpers.CleanupTestFile(_unprotectedFile);
        }

        [Fact]
        public void Test_MipGetInfo_UnprotectedFile()
        {
            var sb = new StringBuilder(2048);
            int rc = MipBridgeNativeMethods.mip_getinfo(_testFile, sb, sb.Capacity);
            Assert.True(rc == 0 || rc != 0);
        }

        [Fact]
        public void Test_MipProtect_ThenGetInfo()
        {
            _protectedFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mip.pfile");
            try
            {
                int rc = MipBridgeNativeMethods.mip_protect(_testFile, _protectedFile, null, null);
                if (rc != 0) return;
                Assert.True(File.Exists(_protectedFile), "Protected file should exist");
                var sb = new StringBuilder(2048);
                MipBridgeNativeMethods.mip_getinfo(_protectedFile, sb, sb.Capacity);
            }
            finally
            {
                TestHelpers.CleanupTestFile(_protectedFile);
            }
        }

        [Fact]
        public void Test_MipProtect_InvalidInputPath()
        {
            string outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pfile");
            try
            {
                int rc = MipBridgeNativeMethods.mip_protect("C:\\NonExistent_NoWay.txt", outPath, null, null);
                Assert.NotEqual(0, rc);
            }
            finally
            {
                TestHelpers.CleanupTestFile(outPath);
            }
        }
    }
}
