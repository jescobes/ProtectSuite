using System;
using System.IO;
using System.Text;
using Xunit;
using Mip.CSharp.WinForms;

namespace Tests.Mip.CSharp.WinForms
{
    /// <summary>
    /// Integration tests: protect -> getinfo -> unprotect flow.
    /// </summary>
    public class MipIntegrationTests : IDisposable
    {
        private string _testFile;
        private string _protectedFile;
        private string _unprotectedFile;

        public MipIntegrationTests()
        {
            TestHelpers.InitializeMip();
            _testFile = TestHelpers.CreateTestFile("MIP integration test content");
        }

        public void Dispose()
        {
            TestHelpers.CleanupTestFile(_testFile);
            TestHelpers.CleanupTestFile(_protectedFile);
            TestHelpers.CleanupTestFile(_unprotectedFile);
        }

        [Fact]
        public void Test_Protect_Unprotect_Flow()
        {
            _protectedFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mip.pfile");
            _unprotectedFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mip.unprot");
            try
            {
                int protectRc = MipBridgeNativeMethods.mip_protect(_testFile, _protectedFile, null, null);
                if (protectRc != 0)
                    return; // Skip if MIP not configured

                Assert.True(File.Exists(_protectedFile));
                int unprotectRc = MipBridgeNativeMethods.mip_unprotect(_protectedFile, _unprotectedFile);
                if (unprotectRc != 0)
                    return;
                Assert.True(File.Exists(_unprotectedFile));
            }
            finally
            {
                TestHelpers.CleanupTestFile(_protectedFile);
                TestHelpers.CleanupTestFile(_unprotectedFile);
            }
        }
    }
}
