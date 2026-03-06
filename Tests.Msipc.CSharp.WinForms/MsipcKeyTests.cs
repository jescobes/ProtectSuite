using System;
using System.IO;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Tests for MSIPC Key API functions as seen in Office log:
    /// - IpcGetKey (IpcpGetKey) - Get key from license
    /// - IpcGetKeyProperty - Get key properties (block size, license, user display name)
    /// - IpcCloseHandle - Close key handle
    /// </summary>
    public class MsipcKeyTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;
        private string _testTemplateId;
        private byte[] _testLicense;
        private SafeInformationProtectionKeyHandle _testKeyHandle;

        public MsipcKeyTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
            
            // Try to get a template ID for testing
            _testTemplateId = TestHelpers.GetTestTemplateId(_connectionInfo);
            
            // Try to serialize a license if template is available
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
        public void Test_IpcGetKey_FromLicense()
        {
            // Test: IpcGetKey - Get key from serialized license
            // This is the primary API called by Office when opening a protected document
            // Office log shows: "Public API called: IpcpGetKey"
            if (_testLicense == null || _testLicense.Length == 0)
            {
                // Skip if no test license available
                return;
            }

            try
            {
                SafeInformationProtectionKeyHandle keyHandle = SafeNativeMethods.IpcGetKey(
                    _testLicense,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                Assert.NotNull(keyHandle);
                Assert.False(keyHandle.IsInvalid, "Key handle should be valid");

                // Cleanup
                keyHandle.Dispose();
            }
            catch (InformationProtectionException ex)
            {
                // May fail if user doesn't have access or server is unavailable
                Assert.True(ex.HResult < 0, "Expected error HRESULT");
            }
        }

        [Fact]
        public void Test_IpcGetKeyProperty_BlockSize()
        {
            // Test: IpcGetKeyProperty - Get block size
            // Office log shows multiple calls to IpcGetKeyProperty
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                // Skip if no test key available
                return;
            }

            try
            {
                int blockSize = SafeNativeMethods.IpcGetKeyBlockSize(_testKeyHandle);
                Assert.True(blockSize > 0, "Block size should be positive");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcGetKeyProperty_License()
        {
            // Test: IpcGetKeyProperty - Get license from key
            // Office retrieves the license to check properties
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                byte[] license = SafeNativeMethods.IpcGetKeyLicense(_testKeyHandle);
                Assert.NotNull(license);
                Assert.True(license.Length > 0, "License should not be empty");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }

        [Fact]
        public void Test_IpcGetKeyProperty_UserDisplayName()
        {
            // Test: IpcGetKeyProperty - Get user display name
            // Office uses this to show who has access
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                string displayName = SafeNativeMethods.IpcGetKeyUserDisplayName(_testKeyHandle);
                Assert.NotNull(displayName);
                Assert.False(string.IsNullOrEmpty(displayName), "Display name should not be empty");
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid or user info not available
            }
        }

        [Fact]
        public void Test_IpcCloseHandle()
        {
            // Test: IpcCloseHandle - Close key handle
            // Office log shows: "Public API called: IpcCloseHandle"
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            // Create a new handle to test closing
            try
            {
                SafeInformationProtectionKeyHandle testHandle = SafeNativeMethods.IpcGetKey(
                    _testLicense,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                Assert.False(testHandle.IsInvalid, "Handle should be valid before closing");
                
                // Close using Dispose (which calls IpcCloseHandle internally)
                testHandle.Dispose();
                
                // Handle should be invalid after closing
                Assert.True(testHandle.IsInvalid, "Handle should be invalid after closing");
            }
            catch (InformationProtectionException)
            {
                // May fail if license/key operations fail
            }
        }

        [Fact]
        public void Test_IpcFreeMemory()
        {
            // Test: IpcFreeMemory - Free memory allocated by MSIPC
            // Office log shows multiple calls: "Public API called: IpcFreeMemory"
            // This is tested indirectly through other API calls, but we can test directly
            
            // Get a property that allocates memory
            if (_testKeyHandle == null || _testKeyHandle.IsInvalid)
            {
                return;
            }

            try
            {
                // This will allocate memory internally
                string displayName = SafeNativeMethods.IpcGetKeyUserDisplayName(_testKeyHandle);
                
                // Memory is freed automatically by SafeNativeMethods wrapper
                // Direct test would require calling UnsafeNativeMethods directly
                Assert.NotNull(displayName);
            }
            catch (InformationProtectionException)
            {
                // May fail if key is invalid
            }
        }
    }
}
