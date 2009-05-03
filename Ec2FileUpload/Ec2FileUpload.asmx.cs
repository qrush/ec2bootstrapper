using System;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Ec2FileUpload
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Ec2FileUpload : System.Web.Services.WebService
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll",
            EntryPoint = "CloseHandle", 
            SetLastError = true, 
            CharSet = CharSet.Auto, 
            CallingConvention = CallingConvention.StdCall)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("kernel32", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr handle, Int32 milliseconds);

        [DllImport("advapi32.dll",
            EntryPoint = "CreateProcessAsUser", 
            SetLastError = true, 
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, 
            String lpApplicationName,
            String lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, 
            bool bInheritHandle,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory, 
            ref STARTUPINFO lpStartupInfo, 
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll",
            EntryPoint = "DuplicateTokenEx")]
        public extern static bool DuplicateTokenEx(IntPtr ExistingTokenHandle,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            int TokenType,
            int ImpersonationLevel,
            ref IntPtr DuplicateTokenHandle);

        [WebMethod]
        public string UploadAndInstallMsiFile(
            string fileName,
            string encodedFile)
        {
            string tempFileName = string.Empty;
            string guid = System.Guid.NewGuid().ToString();

            try
            {
                //
                // Impersonate the caller
                //
                using (((WindowsIdentity)HttpContext.Current.User.Identity).Impersonate())
                {
                    //
                    // Create a temporary file
                    //
                    tempFileName = Path.GetTempPath() + fileName;

                    //
                    // Write out the caller data
                    //

                    FileStream stream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write);

                    byte[] fileData = Convert.FromBase64String(encodedFile);
                    stream.Write(fileData, 0, fileData.Length);
                    stream.Close();


                    IntPtr Token = new IntPtr(0);
                    IntPtr DupedToken = new IntPtr(0);

                    SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                    sa.bInheritHandle = false;
                    sa.Length = Marshal.SizeOf(sa);
                    sa.lpSecurityDescriptor = (IntPtr)0;

                    Token = WindowsIdentity.GetCurrent().Token;

                    const uint GENERIC_ALL = 0x10000000;

                    const int SecurityImpersonation = 2;
                    const int TokenType = 1;

                    bool ret = DuplicateTokenEx(Token,
                        GENERIC_ALL,
                        ref sa,
                        SecurityImpersonation,
                        TokenType,
                        ref DupedToken);

                    if (ret == false)
                    {
                        throw new Exception("DuplicateTokenEx failed. error = " + Marshal.GetLastWin32Error());
                    }

                    STARTUPINFO si = new STARTUPINFO();
                    si.cb = Marshal.SizeOf(si);
                    si.lpDesktop = "";

                    string commandLine = @"C:\Scripts\Ec2AppInstaller.exe " + tempFileName + " " + guid;

                    PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
                    ret = CreateProcessAsUser(DupedToken,
                        null,
                        commandLine,
                        ref sa,
                        ref sa,
                        false,
                        0,
                        (IntPtr)0,
                        null,
                        ref si,
                        out pi);

                    if (ret == false)
                    {
                        throw new Exception("CreateProcessAsUser failed. error = " + Marshal.GetLastWin32Error());
                    }

                    RegistryKey key = Registry.LocalMachine;
                    RegistryKey subkey = key.OpenSubKey(@"Software\JWSecure\Ec2Bootstrapper", true);
                    //-2 still in the process of installation
                    subkey.SetValue(guid, -2, RegistryValueKind.DWord);

                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    CloseHandle(DupedToken);
                }
            }
            catch (Exception)
            {
                //log error here for debug purpose
                guid = "";
            }
            finally
            {
                if (string.IsNullOrEmpty(guid))
                {
                    if (File.Exists(tempFileName) == true)
                        File.Delete(tempFileName);
                }
            }

            return guid;
        }

        [WebMethod]
        public int GetInstallationStatus(
            string guid)
        {
            uint[] ret = new uint[2];

            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    return -1;
                }

                RegistryKey key = Registry.LocalMachine;
                RegistryKey subkey = key.OpenSubKey(@"Software\JWSecure\Ec2Bootstrapper");
                if (subkey != null)
                    return Convert.ToInt32(subkey.GetValue(guid));
                else
                    return -1 ;
            }
            catch (Exception)
            {
            }
            return -1;
        }
    }
}