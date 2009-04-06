using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Web.Services;
using System.Windows.Forms;
using Amazon.EC2;
using Amazon.EC2.Util;
using Amazon.EC2.Model;
using Amazon.EC2.Mock;

namespace Ec2Bootstrapperlib
{
    internal class CEc2Context
    {
        public string accessKeyId;
        public string secretAccessKey;
        public string instanceId;
        public string publicDns;
        public string securityGroupName;
    };

    public class CEc2Bootstrapper
    {
        const string amiImageId = "ami-0529ce6c";
        const string certFile = "server.crt";
        const string securityGroupName = "JWSecureEc2FileLoad";
        const string securityGroupDescription = "Used for msi upload";
        const string keyName = "JWSecureFileUploadKey";
        const string keyFileName = "JWSecureFileUploadKey.pem";

        CEc2Context contxt;
        AmazonEC2 service;

        public static void Run(string accessKeyId, string secretKey)
        {
            try
            {
                CEc2Bootstrapper bootStrapper = new CEc2Bootstrapper(accessKeyId, secretKey);
                bootStrapper.CreateKayPair();
                bootStrapper.LaunchInstance();
                bootStrapper.GetSiteUrl();
                bootStrapper.DownloadAndInstallCertificate();
                bootStrapper.GetAdministratorPassord();
                bootStrapper.UploadAndInstallMsi();
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
        }

        private CEc2Bootstrapper(string accessKeyId, string secretKey)
        {
            contxt = new CEc2Context();
            contxt.accessKeyId = accessKeyId;
            contxt.secretAccessKey = secretKey;
            service = new AmazonEC2Client(accessKeyId, secretKey);
        }

        private void LaunchInstance()
        {
            try
            {
                RunInstancesRequest request = new RunInstancesRequest();

                request.ImageId = amiImageId;
                request.MinCount = 1;
                request.MaxCount = 1;

                CreateSecurityGroup();
                request.SecurityGroup.Add(securityGroupName);

                CreateKayPair();
                request.KeyName = keyName;

                RunInstancesResponse response = service.RunInstances(request);

                if (response.IsSetRunInstancesResult())
                {
                    RunInstancesResult runInstancesResult = response.RunInstancesResult;
                    if (runInstancesResult.IsSetReservation())
                    {
                        if (runInstancesResult.Reservation.RunningInstance[0].IsSetInstanceId())
                        {
                            contxt.instanceId = runInstancesResult.Reservation.RunningInstance[0].InstanceId;
                        }
                        if (runInstancesResult.Reservation.RunningInstance[0].IsSetPublicDnsName())
                        {
                            contxt.publicDns = runInstancesResult.Reservation.RunningInstance[0].PublicDnsName;
                        }
                    }
                }

                if (string.IsNullOrEmpty(contxt.instanceId) == true)
                    throw new Exception("No instance id is returned.");

                //return after the instance started up
                bool pending = true;
                while (pending == true)
                {
                    System.Threading.Thread.Sleep(5 * 1000);

                    DescribeInstancesRequest describeRequest = new DescribeInstancesRequest();
                    describeRequest.InstanceId.Add(contxt.instanceId);
                    DescribeInstancesResponse describeResponse = service.DescribeInstances(describeRequest);
                    DescribeInstancesResult describeResult = describeResponse.DescribeInstancesResult;

                    if (describeResult.Reservation.Count != 1)
                        throw new Exception("more than one instance with the same id");
                    if (describeResult.Reservation[0].RunningInstance.Count != 1)
                        throw new Exception("more than one running instance has the same id");

                    pending = describeResult.Reservation[0].RunningInstance[0].InstanceState.Name != "running";
                }
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
        }

        private void UploadAndInstallMsi()
        {
            try
            {
                Stream fileStream = null;

                //
                // Get administrator's password
                //
                string password = GetAdministratorPassord(); 

                //
                // Get the MSI file
                //

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "MSI files (*.msi)|*.msi";
                if (DialogResult.OK != ofd.ShowDialog())
                {
                    throw new Exception("Error: open file dialog failed");
                }

                if (null == (fileStream = ofd.OpenFile()))
                {
                    throw new Exception("Error: failed to open selected file");
                }

                byte[] fileBytes = new byte[fileStream.Length];
                fileStream.Read(fileBytes, 0, (int)fileStream.Length);

                //
                // Create the web service proxy
                //

                Ec2FileUploadProxy.Ec2FileUpload fileUpload =
                    new Ec2FileUploadProxy.Ec2FileUpload();

                fileUpload.Url = "https://" + contxt.publicDns + "/ec2fileupload.asmx";

                //
                // Get client credentials
                //

                CredentialCache cache = new CredentialCache();
                cache.Add(
                    new Uri(fileUpload.Url),
                    "Basic",
                    new NetworkCredential("administrator", password));
                fileUpload.Credentials = cache;

                //
                // Call the web service
                //

                int error = fileUpload.UploadAndInstallMsiFile(
                    "Ec2Install.msi", Convert.ToBase64String(fileBytes));
                if (error != 0)
                {
                    throw new Exception("Error: upload failed");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UploadAndInstallMsi fails." + ex.Message);
            }
        }

        private void DownloadAndInstallCertificate()
        {
            try
            {
                string req = "http://" + contxt.publicDns + "/" + certFile;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(req);
                request.Method = "GET";
                request.ContentType = "text/xml";

                // Get the response.
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            // Read the content, which is the public key of the server
                            string responseFromServer = reader.ReadToEnd();

                            string currentDir = Directory.GetCurrentDirectory();
                            string certFilePath = currentDir + "\\" + certFile;
                            if (File.Exists(certFilePath))
                            {
                                File.Delete(certFilePath);
                            }

                            TextWriter tw = new StreamWriter(certFilePath);
                            tw.Write(responseFromServer);
                            tw.Close();

                            //install certificate
                            InstallCertificate(certFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DownloadAndInstallCertificate fails." + ex.Message);
            }
        }

        private void InstallCertificate(string file)
        {
            try
            {
                var serviceRuntimeUserCertificateStore = new X509Store(StoreName.Root);
                serviceRuntimeUserCertificateStore.Open(OpenFlags.ReadWrite);
                X509Certificate2 cert = new X509Certificate2(file);
                serviceRuntimeUserCertificateStore.Add(cert);
                serviceRuntimeUserCertificateStore.Close();
                File.Delete(file);
            }
            catch (Exception ex)
            {
                throw new Exception("InstallCertificate fails." + ex.Message);
            }
        }

        // EC2 doesn't have API to access password
        private string GetAdministratorPassord()
        {
            try
            {
                // /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(
                        "cmd", @"/c ec2-get-password.cmd " +
                        contxt.instanceId +
                        " -k " + GetKeyFilePath());

                //redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;

                // Do not create a console window.
                procStartInfo.CreateNoWindow = true;

                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = procStartInfo;
                process.Start();

                // Get the output into a string
                return process.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new Exception("GetAdministratorPassord fails." + ex.Message);
            }
        }

        private void GetSiteUrl()
        {
            try
            {
                //contxt.instanceId = "i-9e7d1df7";
                DescribeInstancesRequest request = new DescribeInstancesRequest();
                request.InstanceId.Add(contxt.instanceId);
                DescribeInstancesResponse response = service.DescribeInstances(request);
                DescribeInstancesResult describeInstancesResult = response.DescribeInstancesResult;

                List<Reservation> reservationList = describeInstancesResult.Reservation;

                //need to finish this
                contxt.publicDns = reservationList[0].RunningInstance[0].PublicDnsName;
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
        }

        private void CreateSecurityGroup()
        {
            try
            {
                CreateSecurityGroupRequest requestSecurirtyGroup = new CreateSecurityGroupRequest();
                requestSecurirtyGroup.GroupName = securityGroupName;
                requestSecurirtyGroup.GroupDescription = securityGroupDescription;
                CreateSecurityGroupResponse responseSecurityGroup = service.CreateSecurityGroup(requestSecurirtyGroup);

                AuthorizeSecurityGroupIngressRequest requestAuthz = new AuthorizeSecurityGroupIngressRequest();
                requestAuthz.GroupName = securityGroupName;
                requestAuthz.IpProtocol = "tcp";
                requestAuthz.CidrIp = "0.0.0.0/0";

                decimal[] ports = { 80, 443, 1443, 3389 };
                foreach (decimal port in ports)
                {
                    requestAuthz.FromPort = port;
                    requestAuthz.ToPort = port;
                    AuthorizeSecurityGroupIngressResponse responseAuthz = service.AuthorizeSecurityGroupIngress(requestAuthz);
                }
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
        }

        private string GetKeyFilePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         "\\" + keyFileName;
        }

        private void CreateKayPair()
        {
            try
            {
                CreateKeyPairRequest request = new CreateKeyPairRequest();
                request.KeyName = keyName;
                CreateKeyPairResponse response = service.CreateKeyPair(request);
                if (response.IsSetCreateKeyPairResult())
                {
                    CreateKeyPairResult createKeyPairResult = response.CreateKeyPairResult;
                    if (createKeyPairResult.IsSetKeyPair())
                    {
                        string keyFilePath = GetKeyFilePath();
                        if (File.Exists(keyFilePath))
                            File.Delete(keyFilePath);

                        using (FileStream stream = new FileStream(
                            keyFilePath, FileMode.Create, FileAccess.Write))
                        {
                            KeyPair keyPair = createKeyPairResult.KeyPair;
                            if (keyPair.IsSetKeyMaterial())
                            {
                                byte[] fileData = new UTF8Encoding(true).GetBytes(keyPair.KeyMaterial);

                                stream.Write(fileData, 0, fileData.Length);
                            }
                        }
                    }
                } 
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
        }
    }
}
