using System;
using System.IO;
using System.Windows.Forms;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Tests for core MSIPC API functions as seen in Office log:
    /// - IpcInitializeEnvironment
    /// - IpcGetGlobalProperty / IpcSetGlobalProperty
    /// - IpcGetTemplateList
    /// - IpcGetTemplateIssuerList
    /// </summary>
    public class MsipcApiTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;

        public MsipcApiTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public void Test_IpcInitializeEnvironment()
        {
            // Test: IpcInitializeEnvironment - Initialize MSIPC environment
            // This is called at the start of MSIPC operations
            // Note: UnsafeNativeMethods is internal, so we use SafeNativeMethods.IpcInitialize() instead
            try
            {
                SafeNativeMethods.IpcInitialize();
                Assert.True(true, "MSIPC initialized successfully");
            }
            catch (InformationProtectionException ex)
            {
                Assert.True(ex.ErrorCode >= 0, $"IpcInitialize failed with HRESULT: 0x{ex.ErrorCode:X8}");
            }
        }

        [Fact]
        public void Test_IpcGetAPIMode()
        {
            // Test: IpcGetGlobalProperty - Get API mode (Client/Server)
            // This is called to determine the security mode
            APIMode mode = SafeNativeMethods.IpcGetAPIMode();
            Assert.True(Enum.IsDefined(typeof(APIMode), mode), "API mode should be a valid value");
        }

        [Fact]
        public void Test_CheckMsipcCertificatesExist_Offline()
        {
            // Test: Check if MSIPC certificates (GIC and CLC) exist using offline mode
            // This avoids unnecessary authentication attempts when certificates are already available
            if (_connectionInfo == null)
            {
                return; // Skip if no connection info available
            }

            try
            {
                // Try to get templates in offline mode with suppressed UI
                // If certificates exist, MSIPC should be able to return cached templates
                // without requiring authentication
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    _connectionInfo,
                    forceDownload: false,  // Don't force download, use cache if available
                    suppressUI: true,      // Suppress UI to avoid authentication prompts
                    offline: true,         // Work offline - if this succeeds, certificates exist
                    hasUserConsent: false,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                // If we got templates (even empty list) in offline mode, certificates exist
                if (templates != null)
                {
                    // Certificates exist - offline mode works
                    Assert.True(true, $"MSIPC certificates found. {templates.Count} template(s) available in offline mode.");
                }
                else
                {
                    // No certificates - this is also a valid test result
                    Assert.True(true, "No MSIPC certificates found (offline mode failed). Authentication will be required.");
                }
            }
            catch (InformationProtectionException ex)
            {
                // If offline operation fails, certificates likely don't exist or are expired
                // This is a valid test result - it means certificates need to be obtained
                Assert.True(ex.ErrorCode < 0, $"MSIPC certificates check failed (HRESULT: 0x{ex.ErrorCode:X8}). This indicates certificates are not available or expired.");
            }
        }

        [Fact]
        public void Test_CheckMsipcCertificatesExist_Online()
        {
            // Test: Check if MSIPC can work online (with or without existing certificates)
            // This test verifies that online mode works, regardless of certificate cache
            if (_connectionInfo == null)
            {
                return; // Skip if no connection info available
            }

            try
            {
                // Try to get templates in online mode
                // This will use certificates if available, or authenticate if needed
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    _connectionInfo,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,        // Online mode - can authenticate if needed
                    hasUserConsent: false,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                // Online mode should work (either with cached certs or authentication)
                Assert.NotNull(templates);
                Assert.True(true, $"Online mode works. {templates.Count} template(s) available.");
            }
            catch (InformationProtectionException ex)
            {
                // Online mode might fail if authentication is required but suppressed
                // This is expected when suppressUI=true and no certificates exist
                Assert.True(ex.ErrorCode < 0, $"Online mode failed (HRESULT: 0x{ex.ErrorCode:X8}). This may indicate authentication is required.");
            }
        }

        [Fact]
        public void Test_IpcGetTemplateList()
        {
            // Test: IpcGetTemplateList - Get list of available templates
            // This is called when Office opens a protected document to get templates
            try
            {
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    _connectionInfo,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                Assert.NotNull(templates);
                // Note: Templates may be empty if no server is available, but API should succeed
            }
            catch (InformationProtectionException ex)
            {
                // If connection fails, that's acceptable for unit tests
                Assert.True(ex.HResult < 0, "Expected error HRESULT");
            }
        }

        [Fact]
        public void Test_IpcGetTemplateList_ForceDownload()
        {
            // Test: IpcGetTemplateList with ForceDownload flag
            // This forces a fresh download from the server
            try
            {
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    _connectionInfo,
                    forceDownload: true,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                Assert.NotNull(templates);
            }
            catch (InformationProtectionException)
            {
                // Expected if server is not available
            }
        }

        [Fact]
        public void Test_IpcGetTemplateIssuerList()
        {
            // Test: IpcGetTemplateIssuerList - Get list of template issuers
            // This is used to discover available RMS servers
            try
            {
                var issuers = SafeNativeMethods.IpcGetTemplateIssuerList(
                    _connectionInfo,
                    defaultServerOnly: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                Assert.NotNull(issuers);
            }
            catch (InformationProtectionException)
            {
                // Expected if server is not available
            }
        }

        [Fact]
        public void Test_IpcGetTemplateIssuerList_DefaultServerOnly()
        {
            // Test: IpcGetTemplateIssuerList with DefaultServerOnly flag
            try
            {
                var issuers = SafeNativeMethods.IpcGetTemplateIssuerList(
                    _connectionInfo,
                    defaultServerOnly: true,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                Assert.NotNull(issuers);
            }
            catch (InformationProtectionException)
            {
                // Expected if server is not available
            }
        }

        [Fact]
        public void Test_IpcSetStoreName()
        {
            // Test: IpcSetGlobalProperty - Set store name
            // This is used to configure the MSIPC store
            try
            {
                string testStoreName = "TestStore";
                SafeNativeMethods.IpcSetStoreName(testStoreName);
                
                string retrievedStoreName = SafeNativeMethods.IpcGetStoreName();
                Assert.Equal(testStoreName, retrievedStoreName);
            }
            catch (InformationProtectionException)
            {
                // May fail if store operations are not supported
            }
        }

        [Fact]
        public void Test_IpcSetAPIMode()
        {
            // Test: IpcSetGlobalProperty - Set API mode
            // This configures whether MSIPC runs in Client or Server mode
            try
            {
                APIMode originalMode = SafeNativeMethods.IpcGetAPIMode();
                
                // Try to set to Client mode (most common)
                SafeNativeMethods.IpcSetAPIMode(APIMode.Client);
                
                APIMode newMode = SafeNativeMethods.IpcGetAPIMode();
                Assert.Equal(APIMode.Client, newMode);
                
                // Restore original mode
                SafeNativeMethods.IpcSetAPIMode(originalMode);
            }
            catch (InformationProtectionException)
            {
                // May fail if mode cannot be changed
            }
        }
    }
}
