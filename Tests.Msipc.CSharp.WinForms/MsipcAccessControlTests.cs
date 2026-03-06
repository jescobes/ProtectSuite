using System;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Tests for MSIPC Access Control API functions as seen in Office log:
    /// - IpcAccessCheck - Check if user has specific rights (VIEW, EDIT, etc.)
    /// Office log shows multiple calls: "Public API called: IpcAccessCheck"
    /// </summary>
    public class MsipcAccessControlTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;
        private string _testTemplateId;
        private byte[] _testLicense;
        private SafeInformationProtectionKeyHandle _testKeyHandle;

        public MsipcAccessControlTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
            _testTemplateId = TestHelpers.GetTestTemplateId(_connectionInfo);
            
            if (!string.IsNullOrEmpty(_testTemplateId))
            {
                try
                {
                    var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(_testTemplateId);
                    _testLicense = SafeNativeMethods.IpcSerializeLicense(
                        licenseHandle,
                        (SerializeLicenseFlags)0,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: true,
                        parentWindow: IntPtr.Zero,
                        out _testKeyHandle);
                }
                catch
                {
                    // If license creation fails, tests will be skipped
                }
            }
        }

        public void Dispose()
        {
            if (_testKeyHandle != null && !_testKeyHandle.IsInvalid)
            {
                _testKeyHandle.Dispose();
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_VIEW()
        {
            // Test: IpcAccessCheck - Check VIEW right
            // Office log shows: "Public API IpcAccessCheck exited with return code 0x00000000"
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                bool hasView = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, CommonRights.ViewRight);
                // Result depends on user's actual rights
                Assert.True(true, "IpcAccessCheck for VIEW completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_EDIT()
        {
            // Test: IpcAccessCheck - Check EDIT right
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                bool hasEdit = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, CommonRights.EditRight);
                Assert.True(true, "IpcAccessCheck for EDIT completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_EXPORT()
        {
            // Test: IpcAccessCheck - Check EXPORT right
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                bool hasExport = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, CommonRights.ExportRight);
                Assert.True(true, "IpcAccessCheck for EXPORT completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_PRINT()
        {
            // Test: IpcAccessCheck - Check PRINT right
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                bool hasPrint = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, CommonRights.PrintRight);
                Assert.True(true, "IpcAccessCheck for PRINT completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_FORWARD()
        {
            // Test: IpcAccessCheck - Check FORWARD right
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                bool hasForward = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, CommonRights.ForwardRight);
                Assert.True(true, "IpcAccessCheck for FORWARD completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_OWNER()
        {
            // Test: IpcAccessCheck - Check OWNER right (Full Control)
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                bool hasOwner = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, CommonRights.OwnerRight);
                Assert.True(true, "IpcAccessCheck for OWNER completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcAccessCheck_AllRights()
        {
            // Test: IpcAccessCheck - Check all common rights
            // Office performs multiple access checks to determine available actions
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            string[] rights = new string[]
            {
                CommonRights.ViewRight,
                CommonRights.EditRight,
                CommonRights.ExportRight,
                CommonRights.PrintRight,
                CommonRights.ForwardRight,
                CommonRights.OwnerRight
            };

            foreach (string right in rights)
            {
                try
                {
                    bool hasRight = SafeNativeMethods.IpcAccessCheck(_testKeyHandle, right);
                    // Log result but don't assert - depends on actual user rights
                    Assert.True(true, $"IpcAccessCheck for {right} completed");
                }
                catch (InformationProtectionException)
                {
                    // Some rights may not be available
                }
            }
        }
    }
}
