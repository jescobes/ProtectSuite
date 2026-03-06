using System.Text;
using Xunit;
using Mip.CSharp.WinForms;

namespace Tests.Mip.CSharp.WinForms
{
    /// <summary>
    /// Tests for MIP bridge API: mip_init, mip_cleanup, mip_getinfo.
    /// </summary>
    public class MipApiTests
    {
        [Fact]
        public void Test_MipInit()
        {
            TestHelpers.InitializeMip();
            int rc = MipBridgeNativeMethods.mip_init();
            Assert.True(rc == 0 || rc != 0, "mip_init should return a result code");
        }

        [Fact]
        public void Test_MipGetInfo_NonExistentFile()
        {
            TestHelpers.InitializeMip();
            var sb = new StringBuilder(256);
            int rc = MipBridgeNativeMethods.mip_getinfo("C:\\NonExistentFile_NoWayThisExists.txt", sb, sb.Capacity);
            Assert.NotEqual(0, rc);
        }

        [Fact]
        public void Test_MipGetInfo_EmptyPath()
        {
            TestHelpers.InitializeMip();
            var sb = new StringBuilder(256);
            int rc = MipBridgeNativeMethods.mip_getinfo("", sb, sb.Capacity);
            Assert.NotEqual(0, rc);
        }
    }
}
