using System.Runtime.InteropServices;
using System.Text;

namespace Mip.CSharp.WinForms
{
    /// <summary>
    /// P/Invoke declarations for Mip.NativeBridge.dll (MIP SDK C++ bridge).
    /// Used by MainForm and tests.
    /// </summary>
    public static class MipBridgeNativeMethods
    {
        private const string DllName = "Mip.NativeBridge.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int mip_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int mip_protect(
            [MarshalAs(UnmanagedType.LPWStr)] string inFile,
            [MarshalAs(UnmanagedType.LPWStr)] string outFile,
            [MarshalAs(UnmanagedType.LPWStr)] string templateId,
            [MarshalAs(UnmanagedType.LPWStr)] string labelId);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int mip_unprotect(
            [MarshalAs(UnmanagedType.LPWStr)] string inFile,
            [MarshalAs(UnmanagedType.LPWStr)] string outFile);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int mip_getinfo(
            [MarshalAs(UnmanagedType.LPWStr)] string inFile,
            StringBuilder info,
            int capacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mip_cleanup();
    }
}
