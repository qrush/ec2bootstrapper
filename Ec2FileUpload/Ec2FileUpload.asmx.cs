using System;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;

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
        public int UploadAndInstallMsiFile(
            string fileName,
            string encodedFile)
        {
            int error = -1;
            string tempFileName = string.Empty;

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
                        return Marshal.GetLastWin32Error();

                    STARTUPINFO si = new STARTUPINFO();
                    si.cb = Marshal.SizeOf(si);
                    si.lpDesktop = "";

                    string commandLine = @"msiexec.exe /qn /i " + tempFileName;

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
                        return Marshal.GetLastWin32Error();

                    // Wait until child process exits.
                    const int INFINITE = -1;
                    WaitForSingleObject(pi.hProcess, INFINITE);

                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    ret = CloseHandle(DupedToken);
                    if (ret == false)
                        return Marshal.GetLastWin32Error();

                    File.Delete(tempFileName);
                    error = 0;
                }
            }
            catch (IOException)
            {
                error = -2;
            }
            finally
            {
                if(File.Exists(tempFileName) == true)
                    File.Delete(tempFileName);
            }

            return error;
        }
    }
}