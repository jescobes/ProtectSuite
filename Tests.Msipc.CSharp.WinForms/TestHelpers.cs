using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Microsoft.InformationProtectionAndControl;
using Newtonsoft.Json;

namespace Tests.Msipc.CSharp.WinForms
{
    /// <summary>
    /// Helper class for MSIPC unit tests
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a temporary test file with sample content
        /// </summary>
        public static string CreateTestFile(string content = "Test file content for MSIPC protection")
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(tempFile, content);
            return tempFile;
        }

        /// <summary>
        /// Cleans up test files
        /// </summary>
        public static void CleanupTestFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch { }
            }
        }

        /// <summary>
        /// Creates a test ConnectionInfo for AD RMS
        /// Reads from config/adrms.json (same as the main application)
        /// </summary>
        public static ConnectionInfo CreateTestConnectionInfo()
        {
            // Try to read from config file (same location as main application)
            try
            {
                // Try multiple paths: from test output directory, or from source config directory
                string[] possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "config", "adrms.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "adrms.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "config", "adrms.json")
                };

                string configPath = null;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        configPath = path;
                        break;
                    }
                }

                if (configPath != null && File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<AdRmsConfig>(json);
                    
                    if (config != null)
                    {
                        Uri extranet = null;
                        Uri intranet = null;
                        
                        if (!string.IsNullOrWhiteSpace(config.extranetUrl))
                            extranet = new Uri(config.extranetUrl);
                        if (!string.IsNullOrWhiteSpace(config.intranetUrl))
                            intranet = new Uri(config.intranetUrl);
                        if (extranet == null && intranet == null && !string.IsNullOrWhiteSpace(config.serverUrl))
                        {
                            // Fallback: use serverUrl for both
                            var u = new Uri(config.serverUrl);
                            extranet = u;
                            intranet = u;
                        }

                        if (extranet != null || intranet != null)
                        {
                            return new ConnectionInfo(extranet, intranet, config.licensingOnlyClusters);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If config read fails, fall back to defaults
                System.Diagnostics.Debug.WriteLine($"Error reading AD RMS config: {ex.Message}");
            }

            // Default fallback (only if config file not found or invalid)
            return new ConnectionInfo(
                new Uri("https://localhost/_wmcs/licensing"),
                new Uri("https://localhost/_wmcs/licensing"));
        }

        /// <summary>
        /// Configuration class for AD RMS (matches MainForm.AdrmsConfig)
        /// </summary>
        private class AdRmsConfig
        {
            public string serverUrl { get; set; } = string.Empty;
            public string extranetUrl { get; set; } = string.Empty;
            public string intranetUrl { get; set; } = string.Empty;
            public bool licensingOnlyClusters { get; set; } = true;
        }

        /// <summary>
        /// Initializes MSIPC environment for testing
        /// </summary>
        public static void InitializeMsipc()
        {
            try
            {
                SafeNativeMethods.IpcInitialize();
            }
            catch
            {
                // If initialization fails, tests will handle it gracefully
                // UnsafeNativeMethods is internal and cannot be accessed from test project
            }
        }

        /// <summary>
        /// Gets a test template ID (if available)
        /// </summary>
        public static string GetTestTemplateId(ConnectionInfo connectionInfo)
        {
            try
            {
                    var templates = SafeNativeMethods.IpcGetTemplateList(
                    connectionInfo,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                if (templates != null && templates.Count > 0)
                {
                    return templates[0].TemplateId;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Creates a test license handle from template
        /// </summary>
        public static SafeInformationProtectionLicenseHandle CreateTestLicense(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentException("Template ID cannot be null or empty", nameof(templateId));
            }

            return SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId);
        }

        /// <summary>
        /// Waits for async operations to complete
        /// </summary>
        public static void WaitForCompletion(int milliseconds = 1000)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Checks if MSIPC certificates (GIC and CLC) exist using MSIPC API functions.
        /// Uses IpcGetTemplateList with offline=true to check if cached certificates exist.
        /// </summary>
        /// <param name="connectionInfo">Connection info to determine certificate location</param>
        /// <returns>True if certificates exist (can work offline), false otherwise</returns>
        public static bool CheckMsipcCertificatesExist(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                return false;
            }

            try
            {
                // Try to get templates in offline mode with suppressed UI
                // If certificates exist, MSIPC should be able to return cached templates
                // without requiring authentication
                var templates = SafeNativeMethods.IpcGetTemplateList(
                    connectionInfo,
                    forceDownload: false,  // Don't force download, use cache if available
                    suppressUI: true,      // Suppress UI to avoid authentication prompts
                    offline: true,         // Work offline - if this succeeds, certificates exist
                    hasUserConsent: false,
                    parentWindow: (Form)null,
                    cultureInfo: null);

                // If we got templates (even empty list) in offline mode, certificates exist
                return templates != null;
            }
            catch (InformationProtectionException)
            {
                // If offline operation fails, certificates likely don't exist or are expired
                return false;
            }
            catch
            {
                // If check fails for any other reason, assume certificates don't exist
                return false;
            }
        }
    }
}
