using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace vincy
{
    public class Downloader
    {
        /// <summary>
        /// The URLMON library contains this function, URLDownloadToFile, which is a way
        /// to download files without user prompts.  The ExecWB( _SAVEAS ) function always
        /// prompts the user, even if _DONTPROMPTUSER parameter is specified, for "internet
        /// security reasons".  This function gets around those reasons.
        /// </summary>
        /// <param name="pCaller">Pointer to caller object (AX).</param>
        /// <param name="szURL">String of the URL.</param>
        /// <param name="szFileName">String of the destination filename/path.</param>
        /// <param name="dwReserved">[reserved].</param>
        /// <param name="lpfnCB">A callback function to monitor progress or abort.</param>
        /// <returns>0 for okay.</returns>
        [DllImport("urlmon.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 URLDownloadToFile(
            [MarshalAs(UnmanagedType.IUnknown)] object pCaller,
            [MarshalAs(UnmanagedType.LPWStr)] string szURL,
            [MarshalAs(UnmanagedType.LPWStr)] string szFileName,
            Int32 dwReserved,
            IntPtr lpfnCB);
        // This version maps HRESULT to exception:
        [DllImport("urlmon.dll", CharSet = CharSet.Auto, PreserveSig = false, EntryPoint = "URLDownloadToFile")]
        public static extern void URLDownloadToFile2(
            [MarshalAs(UnmanagedType.IUnknown)] object pCaller,
            [MarshalAs(UnmanagedType.LPTStr)] string szURL,
            [MarshalAs(UnmanagedType.LPTStr)] string szFileName,
            Int32 dwReserved,
            IntPtr lpfnCB);
    }
}
