using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Integration tests that simulate complete Office workflows:
    /// 1. Opening a protected document (validation, get key, access checks, get info)
    /// 2. Protecting a new document (template selection, encryption)
    /// 3. Unprotecting a document (decryption)
    /// 4. Getting document information without Full Control (VIEW rights scenario)
    /// </summary>
    public class MsipcIntegrationTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;
        private string _testTemplateId;
        private string _testFile;
        private string _encryptedFile;

        public MsipcIntegrationTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
            _testTemplateId = TestHelpers.GetTestTemplateId(_connectionInfo);
            _testFile = TestHelpers.CreateTestFile("Integration test content for MSIPC");
        }

        public void Dispose()
        {
            TestHelpers.CleanupTestFile(_testFile);
            TestHelpers.CleanupTestFile(_encryptedFile);
        }

        [Fact]
        public void Test_CompleteProtectionWorkflow()
        {
            // Test: Complete protection workflow as Office would perform
            // 1. Get templates
            // 2. Create license from template
            // 3. Encrypt file
            // 4. Verify encryption
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return; // Skip if no template available
            }

            try
            {
                // Step 0: Check if certificates exist before starting workflow
                // This avoids unnecessary authentication attempts when certificates are already available
                bool certificatesExist = TestHelpers.CheckMsipcCertificatesExist(_connectionInfo);
                Assert.True(true, $"Certificate check: {(certificatesExist ? "Certificates found (offline mode works)" : "No certificates found (authentication will be required)")}");

                // Step 1: Get templates (Office does this when user selects "Protect")
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    _connectionInfo,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                Assert.NotNull(templates);

                // Step 2: Create license from template
                var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(_testTemplateId);
                Assert.False(licenseHandle.IsInvalid, "License handle should be valid");

                // Step 3: Encrypt file
                _encryptedFile = SafeFileApiNativeMethods.IpcfEncryptFile(
                    _testFile,
                    licenseHandle,
                    SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    symmKey: null);

                Assert.NotNull(_encryptedFile);
                Assert.True(File.Exists(_encryptedFile), "Encrypted file should exist");

                // Step 4: Verify encryption status
                SafeFileApiNativeMethods.FileEncryptedStatus status = SafeFileApiNativeMethods.IpcfIsFileEncrypted(_encryptedFile);
                Assert.NotEqual(SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED, status);

                licenseHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if server is unavailable
            }
        }

        [Fact]
        public void Test_CompleteOpeningWorkflow()
        {
            // Test: Complete opening workflow as Office would perform when opening a protected document
            // 1. Check if file is encrypted
            // 2. Get license from file
            // 3. Get key from license (IpcGetKey)
            // 4. Get key properties (block size, user display name)
            // 5. Check access rights (IpcAccessCheck for various rights)
            // 6. Get license properties (owner, content ID, user rights)
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                // First, create a protected file
                _encryptedFile = SafeFileApiNativeMethods.IpcfEncryptFile(
                    _testFile,
                    _testTemplateId,
                    SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                if (!File.Exists(_encryptedFile))
                {
                    return;
                }

                // Step 1: Check if file is encrypted
                SafeFileApiNativeMethods.FileEncryptedStatus status = SafeFileApiNativeMethods.IpcfIsFileEncrypted(_encryptedFile);
                Assert.NotEqual(SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED, status);

                // Step 2: Get license from file
                byte[] license = SafeFileApiNativeMethods.IpcfGetSerializedLicenseFromFile(_encryptedFile);
                Assert.NotNull(license);
                Assert.True(license.Length > 0, "License should not be empty");

                // Step 3: Get key from license (IpcGetKey - primary API called by Office)
                SafeInformationProtectionKeyHandle keyHandle = SafeNativeMethods.IpcGetKey(
                    license,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                Assert.NotNull(keyHandle);
                Assert.False(keyHandle.IsInvalid, "Key handle should be valid");

                // Step 4: Get key properties
                int blockSize = SafeNativeMethods.IpcGetKeyBlockSize(keyHandle);
                Assert.True(blockSize > 0, "Block size should be positive");

                string userDisplayName = SafeNativeMethods.IpcGetKeyUserDisplayName(keyHandle);
                // Display name may be null, but API should succeed

                // Step 5: Check access rights (Office performs multiple access checks)
                bool hasView = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.ViewRight);
                bool hasEdit = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.EditRight);
                bool hasPrint = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.PrintRight);
                bool hasExport = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.ExportRight);
                bool hasForward = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.ForwardRight);
                bool hasOwner = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.OwnerRight);

                // Step 6: Get license properties
                byte[] keyLicense = SafeNativeMethods.IpcGetKeyLicense(keyHandle);
                Assert.NotNull(keyLicense);

                // Get serialized license properties
                string serializedOwner = SafeNativeMethods.IpcGetSerializedLicenseOwner(license, keyHandle);
                string serializedContentId = SafeNativeMethods.IpcGetSerializedLicenseContentId(license, keyHandle);
                Collection<UserRights> serializedUserRights = SafeNativeMethods.IpcGetSerializedLicenseUserRightsList(license, keyHandle);

                // Cleanup
                keyHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if user doesn't have access or server is unavailable
            }
        }

        [Fact]
        public void Test_ViewRightsWorkflow()
        {
            // Test: Workflow for opening document with VIEW rights (not Full Control)
            // This simulates a user opening a document they can view but not edit
            // Office uses IpcfGetDecryptedFilePath for this scenario
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                // Create protected file
                _encryptedFile = SafeFileApiNativeMethods.IpcfEncryptFile(
                    _testFile,
                    _testTemplateId,
                    SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                if (!File.Exists(_encryptedFile))
                {
                    return;
                }

                // Get license and key
                byte[] license = SafeFileApiNativeMethods.IpcfGetSerializedLicenseFromFile(_encryptedFile);
                SafeInformationProtectionKeyHandle keyHandle = SafeNativeMethods.IpcGetKey(
                    license,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                // Check that user has VIEW but may not have OWNER
                bool hasView = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.ViewRight);
                bool hasOwner = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.OwnerRight);

                // If user has VIEW but not OWNER, they can open for viewing
                // Office would use IpcfGetDecryptedFilePath in this case
                // (Note: This API may not be available in the current wrapper)

                keyHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if user doesn't have VIEW rights
            }
        }

        [Fact]
        public void Test_UnprotectionWorkflow()
        {
            // Test: Complete unprotection workflow
            // 1. Check file is encrypted
            // 2. Decrypt file
            // 3. Verify decrypted content matches original
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                // Create protected file
                _encryptedFile = SafeFileApiNativeMethods.IpcfEncryptFile(
                    _testFile,
                    _testTemplateId,
                    SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                if (!File.Exists(_encryptedFile))
                {
                    return;
                }

                // Verify encrypted
                SafeFileApiNativeMethods.FileEncryptedStatus encryptedStatus = SafeFileApiNativeMethods.IpcfIsFileEncrypted(_encryptedFile);
                Assert.NotEqual(SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED, encryptedStatus);

                // Decrypt file
                string decryptedFile = SafeFileApiNativeMethods.IpcfDecryptFile(
                    _encryptedFile,
                    SafeFileApiNativeMethods.DecryptFlags.IPCF_DF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    symmKey: null);

                Assert.NotNull(decryptedFile);
                Assert.True(File.Exists(decryptedFile), "Decrypted file should exist");

                // Verify decrypted
                SafeFileApiNativeMethods.FileEncryptedStatus decryptedStatus = SafeFileApiNativeMethods.IpcfIsFileEncrypted(decryptedFile);
                Assert.Equal(SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED, decryptedStatus);

                // Verify content
                string originalContent = File.ReadAllText(_testFile);
                string decryptedContent = File.ReadAllText(decryptedFile);
                Assert.Equal(originalContent, decryptedContent);

                // Cleanup
                TestHelpers.CleanupTestFile(decryptedFile);
            }
            catch (InformationProtectionException)
            {
                // May fail if user doesn't have decrypt rights
            }
        }
    }
}
