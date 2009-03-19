using System;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;

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
        [WebMethod]
        public bool UploadAndInstallMsiFile(
            string fileName,
            string encodedFile)
        {
            bool status = false;
            WindowsImpersonationContext impersonation = null;

            try
            {
                //
                // Impersonate the caller
                //

                impersonation =
                    ((WindowsIdentity)HttpContext.Current.User.Identity).Impersonate();

                //
                // Create a temporary file
                //

                string tempFileName = Path.GetTempPath() + "\\" + fileName;

                //
                // Write out the caller data
                //

                FileStream stream = new FileStream(
                    tempFileName, FileMode.Create, FileAccess.Write);
                byte[] fileData = Convert.FromBase64String(encodedFile);
                stream.Write(fileData, 0, fileData.Length);
                stream.Close();

                //
                // Execute the file as an MSI package
                //

                ProcessStartInfo startInfo = new ProcessStartInfo(
                    "msiexec.exe");
                startInfo.Arguments = "/i " + tempFileName + " /quiet";

                Process msiExec = Process.Start(startInfo);
                msiExec.WaitForExit();

                //
                // Report status
                //

                if (0 == msiExec.ExitCode)
                {
                    File.Delete(tempFileName);
                    status = true;
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                impersonation.Undo();
            }

            return status;
        }
    }
}