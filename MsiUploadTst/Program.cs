using System;
using System.Text;
using System.Web.Services;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace MsiUploadTst
{
    class CredUIHelper
    {
        public static UInt32 CREDUI_MAX_BUFFER_LENGTH = 256;

        [Flags]
        public enum CREDUI_FLAGS
        {
            INCORRECT_PASSWORD = 0x1,
            DO_NOT_PERSIST = 0x2,
            REQUEST_ADMINISTRATOR = 0x4,
            EXCLUDE_CERTIFICATES = 0x8,
            REQUIRE_CERTIFICATE = 0x10,
            SHOW_SAVE_CHECK_BOX = 0x40,
            ALWAYS_SHOW_UI = 0x80,
            REQUIRE_SMARTCARD = 0x100,
            PASSWORD_ONLY_OK = 0x200,
            VALIDATE_USERNAME = 0x400,
            COMPLETE_USERNAME = 0x800,
            PERSIST = 0x1000,
            SERVER_CREDENTIAL = 0x4000,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            KEEP_USERNAME = 0x100000,
        }

        public struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [DllImport("credui.dll", CharSet=CharSet.Ansi)]
        public static extern UInt32 CredUIPromptForCredentialsA(
            ref CREDUI_INFO creditUR,
            [In, MarshalAs(UnmanagedType.LPStr)]
                string targetName,
            IntPtr reserved1,
            UInt32 iError,
            [In, Out, MarshalAs(UnmanagedType.LPStr)]
                StringBuilder userName,
            UInt32 maxUserName,
            [In, Out, MarshalAs(UnmanagedType.LPStr)]
                StringBuilder password,
            UInt32 maxPassword,
            [MarshalAs(UnmanagedType.Bool)] 
                ref bool pfSave,
            CREDUI_FLAGS flags);
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Stream fileStream = null;
            string user = new string(
                (char) 0, (int) CredUIHelper.CREDUI_MAX_BUFFER_LENGTH);
            string password = new string(
                (char)0, (int)CredUIHelper.CREDUI_MAX_BUFFER_LENGTH);
            StringBuilder userName = new StringBuilder(user);
            StringBuilder userPassword = new StringBuilder(password);
            CredUIHelper.CREDUI_INFO credUiInfo = 
                new CredUIHelper.CREDUI_INFO();

            //
            // Get credentials
            //

            bool save = false;
            credUiInfo.cbSize = Marshal.SizeOf(credUiInfo);
            credUiInfo.pszCaptionText = "EC2 Console";
            credUiInfo.pszMessageText = "Please enter your credentials.";
            if (0 != CredUIHelper.CredUIPromptForCredentialsA(
                ref credUiInfo,
                "EC2 Server Instance",
                System.IntPtr.Zero,
                0,
                userName,
                CredUIHelper.CREDUI_MAX_BUFFER_LENGTH,
                userPassword,
                CredUIHelper.CREDUI_MAX_BUFFER_LENGTH,
                ref save,
                CredUIHelper.CREDUI_FLAGS.DO_NOT_PERSIST |
                    CredUIHelper.CREDUI_FLAGS.GENERIC_CREDENTIALS |
                    CredUIHelper.CREDUI_FLAGS.ALWAYS_SHOW_UI))
            {
                System.Console.WriteLine("Error: credential prompting failed");
                return;
            }

            //
            // Get the MSI file
            //

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "MSI files (*.msi)|*.msi";
            if (DialogResult.OK != ofd.ShowDialog())
            {
                System.Console.WriteLine("Error: open file dialog failed");
                return;
            }

            if (null == (fileStream = ofd.OpenFile()))
            {
                System.Console.WriteLine("Error: failed to open selected file");
                return;
            }

            byte[] fileBytes = new byte[fileStream.Length];
            fileStream.Read(fileBytes, 0, (int) fileStream.Length);

            //
            // Create the web service proxy
            //

            Ec2FileUploadProxy.Ec2FileUpload fileUpload =
                new Ec2FileUploadProxy.Ec2FileUpload();

            //
            // Get client credentials
            //
            
            CredentialCache cache = new CredentialCache();
            cache.Add(
                new Uri(fileUpload.Url),
                "Negotiate",
                new NetworkCredential(
                    userName.ToString(), userPassword.ToString()));
            fileUpload.Credentials = cache;

            //
            // Call the web service
            //
            
            if (false == fileUpload.UploadAndInstallMsiFile(
                "Ec2Install.msi", Convert.ToBase64String(fileBytes)))
            {
                System.Console.WriteLine("Error: upload failed");
                return;
            }

            System.Console.WriteLine("Success");
        }
    }
}