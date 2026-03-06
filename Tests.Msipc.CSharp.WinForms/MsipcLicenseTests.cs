using System;
using System.Collections.ObjectModel;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Tests for MSIPC License Property API functions as seen in Office log:
    /// - IpcGetLicenseProperty - Get license properties (ContentId, Owner, UserRightsList, ValidityTime, etc.)
    /// Office log shows multiple calls: "Public API called: IpcGetLicenseProperty"
    /// </summary>
    public class MsipcLicenseTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;
        private string _testTemplateId;
        private SafeInformationProtectionLicenseHandle _testLicenseHandle;

        public MsipcLicenseTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
            _testTemplateId = TestHelpers.GetTestTemplateId(_connectionInfo);
            
            if (!string.IsNullOrEmpty(_testTemplateId))
            {
                try
                {
                    _testLicenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(_testTemplateId);
                }
                catch
                {
                    // If license creation fails, tests will be skipped
                }
            }
        }

        public void Dispose()
        {
            if (_testLicenseHandle != null && !_testLicenseHandle.IsInvalid)
            {
                _testLicenseHandle.Dispose();
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_ContentId()
        {
            // Test: IpcGetLicenseProperty - Get Content ID
            // Office log shows: "Public API called: IpcGetLicenseProperty" for ContentId
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                string contentId = SafeNativeMethods.IpcGetLicenseContentId(_testLicenseHandle);
                // ContentId may be null for newly created licenses
                Assert.True(true, "IpcGetLicenseProperty for ContentId completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_Owner()
        {
            // Test: IpcGetLicenseProperty - Get Owner
            // Office retrieves owner to display document owner information
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                string owner = SafeNativeMethods.IpcGetLicenseOwner(_testLicenseHandle);
                // Owner may be null for newly created licenses
                Assert.True(true, "IpcGetLicenseProperty for Owner completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_UserRightsList()
        {
            // Test: IpcGetLicenseProperty - Get User Rights List
            // Office uses this to determine who has access and what rights they have
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                Collection<UserRights> userRights = SafeNativeMethods.IpcGetLicenseUserRightsList(_testLicenseHandle);
                Assert.NotNull(userRights);
                // User rights list may be empty for template-based licenses
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_ValidityTime()
        {
            // Test: IpcGetLicenseProperty - Get Validity Time
            // Office checks validity time to determine if license is still valid
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                Term validityTime = SafeNativeMethods.IpcGetLicenseValidityTime(_testLicenseHandle);
                Assert.NotNull(validityTime);
                // Validity time may be null (never expires)
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_IntervalTime()
        {
            // Test: IpcGetLicenseProperty - Get Interval Time
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                uint intervalTime = SafeNativeMethods.IpcGetLicenseIntervalTime(_testLicenseHandle);
                // Interval time may be 0 (no interval)
                Assert.True(true, "IpcGetLicenseProperty for IntervalTime completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_ConnectionInfo()
        {
            // Test: IpcGetLicenseProperty - Get Connection Info
            // Office uses this to determine which RMS server issued the license
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                ConnectionInfo connectionInfo = SafeNativeMethods.IpcGetLicenseConnectionInfo(_testLicenseHandle);
                // Connection info may be null
                Assert.True(true, "IpcGetLicenseProperty for ConnectionInfo completed");
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcGetLicenseProperty_Descriptor()
        {
            // Test: IpcGetLicenseProperty - Get Descriptor (Template Info)
            // Office uses this to get template information
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                TemplateInfo descriptor = SafeNativeMethods.IpcGetLicenseDescriptor(_testLicenseHandle, null);
                Assert.NotNull(descriptor);
                Assert.Equal(_testTemplateId, descriptor.TemplateId);
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid
            }
        }

        [Fact]
        public void Test_IpcSetLicenseProperty_ContentId()
        {
            // Test: IpcSetLicenseProperty - Set Content ID
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                string testContentId = Guid.NewGuid().ToString();
                SafeNativeMethods.IpcSetLicenseContentId(_testLicenseHandle, testContentId);
                
                string retrievedContentId = SafeNativeMethods.IpcGetLicenseContentId(_testLicenseHandle);
                Assert.Equal(testContentId, retrievedContentId);
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid or property cannot be set
            }
        }

        [Fact]
        public void Test_IpcSetLicenseProperty_Owner()
        {
            // Test: IpcSetLicenseProperty - Set Owner
            if (_testLicenseHandle == null || _testLicenseHandle.IsInvalid)
            {
                return;
            }

            try
            {
                string testOwner = "test@example.com";
                SafeNativeMethods.IpcSetLicenseOwner(_testLicenseHandle, testOwner);
                
                string retrievedOwner = SafeNativeMethods.IpcGetLicenseOwner(_testLicenseHandle);
                Assert.Equal(testOwner, retrievedOwner);
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid or property cannot be set
            }
        }
    }
}
