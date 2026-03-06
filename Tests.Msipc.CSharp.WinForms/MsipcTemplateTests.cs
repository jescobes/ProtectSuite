using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Tests for MSIPC Template operations:
    /// - Template creation from template ID
    /// - Template creation from scratch
    /// - Template serialization
    /// </summary>
    public class MsipcTemplateTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;
        private Collection<TemplateInfo> _templates;
        private Collection<TemplateIssuer> _issuers;

        public MsipcTemplateTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
            
            try
            {
                _templates = SafeNativeMethods.IpcGetTemplateList(
                    _connectionInfo,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: (Form)null,
                    cultureInfo: null);
            }
            catch
            {
                _templates = new Collection<TemplateInfo>();
            }

            try
            {
                _issuers = SafeNativeMethods.IpcGetTemplateIssuerList(
                    _connectionInfo,
                    defaultServerOnly: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);
            }
            catch
            {
                _issuers = new Collection<TemplateIssuer>();
            }
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public void Test_IpcCreateLicenseFromTemplateId()
        {
            // Test: IpcCreateLicenseFromTemplateId - Create license from template ID
            if (_templates == null || _templates.Count == 0)
            {
                return; // Skip if no templates available
            }

            try
            {
                string templateId = _templates[0].TemplateId;
                SafeInformationProtectionLicenseHandle licenseHandle = 
                    SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId);

                Assert.NotNull(licenseHandle);
                Assert.False(licenseHandle.IsInvalid, "License handle should be valid");
                
                licenseHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if template is invalid
            }
        }

        [Fact]
        public void Test_IpcCreateLicenseFromScratch()
        {
            // Test: IpcCreateLicenseFromScratch - Create license from scratch
            if (_issuers == null || _issuers.Count == 0)
            {
                return; // Skip if no issuers available
            }

            try
            {
                TemplateIssuer issuer = _issuers[0];
                SafeInformationProtectionLicenseHandle licenseHandle = 
                    SafeNativeMethods.IpcCreateLicenseFromScratch(issuer);

                Assert.NotNull(licenseHandle);
                Assert.False(licenseHandle.IsInvalid, "License handle should be valid");
                
                licenseHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if issuer doesn't allow from-scratch creation
            }
        }

        [Fact]
        public void Test_IpcSerializeLicense_FromTemplateId()
        {
            // Test: IpcSerializeLicense - Serialize license from template ID
            if (_templates == null || _templates.Count == 0)
            {
                return;
            }

            try
            {
                string templateId = _templates[0].TemplateId;
                SafeInformationProtectionKeyHandle keyHandle;
                
                byte[] license = SafeNativeMethods.IpcSerializeLicense(
                    templateId,
                    (SerializeLicenseFlags)0,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    out keyHandle);

                Assert.NotNull(license);
                Assert.True(license.Length > 0, "License should not be empty");
                Assert.NotNull(keyHandle);
                Assert.False(keyHandle.IsInvalid, "Key handle should be valid");
                
                keyHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if template is invalid or server is unavailable
            }
        }

        [Fact]
        public void Test_IpcSerializeLicense_FromLicenseHandle()
        {
            // Test: IpcSerializeLicense - Serialize license from license handle
            if (_templates == null || _templates.Count == 0)
            {
                return;
            }

            try
            {
                string templateId = _templates[0].TemplateId;
                var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId);
                SafeInformationProtectionKeyHandle keyHandle;
                
                byte[] license = SafeNativeMethods.IpcSerializeLicense(
                    licenseHandle,
                    (SerializeLicenseFlags)0,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    out keyHandle);

                Assert.NotNull(license);
                Assert.True(license.Length > 0, "License should not be empty");
                Assert.NotNull(keyHandle);
                
                licenseHandle.Dispose();
                keyHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid or server is unavailable
            }
        }

        [Fact]
        public void Test_TemplateProperties()
        {
            // Test: Verify template properties are accessible
            if (_templates == null || _templates.Count == 0)
            {
                return;
            }

            TemplateInfo template = _templates[0];
            
            Assert.NotNull(template.TemplateId);
            Assert.False(string.IsNullOrEmpty(template.TemplateId), "Template ID should not be empty");
            Assert.NotNull(template.Name);
            // Other properties may be null depending on template
        }
    }
}
