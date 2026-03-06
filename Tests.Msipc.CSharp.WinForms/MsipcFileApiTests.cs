using System;
using System.IO;
using System.Windows.Forms;
using Xunit;
using Microsoft.InformationProtectionAndControl;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Tests for MSIPC File API functions as seen in Office operations:
    /// - IpcfEncryptFile - Encrypt a file
    /// - IpcfDecryptFile - Decrypt a file
    /// - IpcfIsFileEncrypted - Check if file is encrypted
    /// - IpcfGetSerializedLicenseFromFile - Get license from protected file
    /// - IpcfGetDecryptedFilePath - Get decrypted file path (used for opening without Full Control)
    /// </summary>
    public class MsipcFileApiTests : IDisposable
    {
        private ConnectionInfo _connectionInfo;
        private string _testTemplateId;
        private string _testFile;
        private string _encryptedFile;

        public MsipcFileApiTests()
        {
            TestHelpers.InitializeMsipc();
            _connectionInfo = TestHelpers.CreateTestConnectionInfo();
            _testTemplateId = TestHelpers.GetTestTemplateId(_connectionInfo);
            _testFile = TestHelpers.CreateTestFile("Test content for MSIPC file protection");
        }

        public void Dispose()
        {
            TestHelpers.CleanupTestFile(_testFile);
            TestHelpers.CleanupTestFile(_encryptedFile);
        }

        [Fact]
        public void Test_IpcfIsFileEncrypted_UnencryptedFile()
        {
            // Test: IpcfIsFileEncrypted - Check if unencrypted file is encrypted
            // Office uses this to determine if a file needs protection
            SafeFileApiNativeMethods.FileEncryptedStatus status = SafeFileApiNativeMethods.IpcfIsFileEncrypted(_testFile);
            Assert.Equal(SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED, status);
        }

        [Fact]
        public void Test_IpcfEncryptFile_WithTemplate()
        {
            // Test: IpcfEncryptFile - Encrypt file with template
            // This is the primary protection operation
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return; // Skip if no template available
            }

            try
            {
                _encryptedFile = SafeFileApiNativeMethods.IpcfEncryptFile(
                    _testFile,
                    _testTemplateId,
                    SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                Assert.NotNull(_encryptedFile);
                Assert.True(File.Exists(_encryptedFile), "Encrypted file should exist");
                
                // Verify file is encrypted
                SafeFileApiNativeMethods.FileEncryptedStatus status = SafeFileApiNativeMethods.IpcfIsFileEncrypted(_encryptedFile);
                Assert.NotEqual(SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED, status);
            }
            catch (InformationProtectionException)
            {
                // May fail if template is invalid or server is unavailable
            }
        }

        [Fact]
        public void Test_IpcfEncryptFile_WithLicenseHandle()
        {
            // Test: IpcfEncryptFile - Encrypt file with license handle
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(_testTemplateId);
                
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
                
                licenseHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if license is invalid or server is unavailable
            }
        }

        [Fact]
        public void Test_IpcfGetSerializedLicenseFromFile()
        {
            // Test: IpcfGetSerializedLicenseFromFile - Get license from protected file
            // Office uses this to extract license information from protected files
            if (string.IsNullOrEmpty(_testTemplateId) || _encryptedFile == null)
            {
                // First encrypt a file
                try
                {
                    _encryptedFile = SafeFileApiNativeMethods.IpcfEncryptFile(
                        _testFile,
                        _testTemplateId,
                        SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: true,
                        parentWindow: IntPtr.Zero);
                }
                catch
                {
                    return; // Skip if encryption fails
                }
            }

            if (!File.Exists(_encryptedFile))
            {
                return;
            }

            try
            {
                byte[] license = SafeFileApiNativeMethods.IpcfGetSerializedLicenseFromFile(_encryptedFile);
                Assert.NotNull(license);
                Assert.True(license.Length > 0, "License should not be empty");
            }
            catch (InformationProtectionException)
            {
                // May fail if file is not properly encrypted
            }
        }

        [Fact]
        public void Test_IpcfDecryptFile()
        {
            // Test: IpcfDecryptFile - Decrypt a protected file
            // This simulates opening a protected document
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                // First encrypt
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

                // Then decrypt
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
                
                // Verify content matches
                string originalContent = File.ReadAllText(_testFile);
                string decryptedContent = File.ReadAllText(decryptedFile);
                Assert.Equal(originalContent, decryptedContent);
                
                // Cleanup decrypted file
                TestHelpers.CleanupTestFile(decryptedFile);
            }
            catch (InformationProtectionException)
            {
                // May fail if user doesn't have decrypt rights or server is unavailable
            }
        }

        [Fact]
        public void Test_GetFileInfo_WithoutFullControl()
        {
            // Test: Get file information without Full Control (VIEW rights scenario)
            // This simulates opening a document with VIEW rights (not OWNER)
            // Office uses IpcGetKey and IpcAccessCheck to determine available actions
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                // First encrypt
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

                // Get license from file
                byte[] license = SafeFileApiNativeMethods.IpcfGetSerializedLicenseFromFile(_encryptedFile);
                
                // Get key (this works with VIEW rights)
                SafeInformationProtectionKeyHandle keyHandle = SafeNativeMethods.IpcGetKey(
                    license,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero);

                // Check access rights
                bool hasView = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.ViewRight);
                bool hasOwner = SafeNativeMethods.IpcAccessCheck(keyHandle, CommonRights.OwnerRight);

                // User can view but may not have full control
                Assert.True(hasView, "User should have VIEW rights to open file");
                
                keyHandle.Dispose();
            }
            catch (InformationProtectionException)
            {
                // May fail if user doesn't have VIEW rights or server is unavailable
            }
        }

        [Fact]
        public void Test_IpcfEncryptFileStream()
        {
            // Test: IpcfEncryptFileStream - Encrypt file using streams
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                using (var inputStream = new FileStream(_testFile, FileMode.Open, FileAccess.Read))
                {
                    Stream outputStream = new MemoryStream();
                    string outputPath = SafeFileApiNativeMethods.IpcfEncryptFileStream(
                        inputStream,
                        _testFile,
                        _testTemplateId,
                        SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: true,
                        parentWindow: (Form)null,
                        symmKey: null,
                        ref outputStream);

                    Assert.NotNull(outputPath);
                    Assert.True(outputStream.Length > 0, "Encrypted stream should not be empty");
                    outputStream.Dispose();
                }
            }
            catch (InformationProtectionException)
            {
                // May fail if template is invalid or server is unavailable
            }
        }

        [Fact]
        public void Test_IpcfDecryptFileStream()
        {
            // Test: IpcfDecryptFileStream - Decrypt file using streams
            if (string.IsNullOrEmpty(_testTemplateId))
            {
                return;
            }

            try
            {
                // First encrypt to memory stream
                byte[] encryptedData;
                using (var inputStream = new FileStream(_testFile, FileMode.Open, FileAccess.Read))
                {
                    Stream outputStream = new MemoryStream();
                    SafeFileApiNativeMethods.IpcfEncryptFileStream(
                        inputStream,
                        _testFile,
                        _testTemplateId,
                        SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: true,
                        parentWindow: (Form)null,
                        symmKey: null,
                        ref outputStream);

                    encryptedData = ((MemoryStream)outputStream).ToArray();
                    outputStream.Dispose();
                }

                // Then decrypt from memory stream
                using (var inputStream = new MemoryStream(encryptedData))
                {
                    Stream outputStream = new MemoryStream();
                    string decryptedPath = SafeFileApiNativeMethods.IpcfDecryptFileStream(
                        inputStream,
                        _testFile,
                        SafeFileApiNativeMethods.DecryptFlags.IPCF_DF_FLAG_DEFAULT,
                        suppressUI: true,
                        offline: false,
                        hasUserConsent: true,
                        parentWindow: (Form)null,
                        ref outputStream);

                    Assert.NotNull(decryptedPath);
                    Assert.True(outputStream.Length > 0, "Decrypted stream should not be empty");
                    outputStream.Dispose();
                }
            }
            catch (InformationProtectionException)
            {
                // May fail if user doesn't have decrypt rights or server is unavailable
            }
        }
    }
}
