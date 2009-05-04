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
using System.Threading;

namespace Ec2Bootstrapperlib
{
    public class CEc2Instance
    {
        const string jwAmiImageId                 = "ami-60ab4d09"; //"ami-0529ce6c";// 
        const string jwCertFile                   = "server.crt";
        const string jwSecurityGroupName          = "JWSecureEc2FileLoad";
        const string jwSecurityGroupDescription   = "Used for msi upload";
        const string jwKeyPairName                = "JWSecureFileUploadKey";
        const string jwKeyPairFileName            = "JWSecureFileUploadKey.pem";
        const string publicDnsTitle               = " Public DNS:";
        const string platformTitle                = " Platform:";

        CAwsConfig _awsConfig;
        AmazonEC2 _service;
        string _instanceId;
        string _amiId;
        string _securityGroups;
        string _type;
        string _status;
        string _publicDns;
        string _zone;
        string _launchTime;
        string _platform;
        string _keyPairName;

        public struct SDeployInfo
        {
            public Ec2FileUploadProxy.Ec2FileUpload proxy;
            public string installId;
        };

        public string instanceId
        {
            get { return _instanceId; }
            set {_instanceId = value; }
        }

        public string imageId
        {
            get { return _amiId; }
            set {_amiId = value; }
        }

        public string securityGroups
        {
            get { return _securityGroups; }
            set {_securityGroups = value; }
        }
        public string type
        {
            get { return _type; }
            set {_type = value; }
        }
        public string status
        {
            get { return _status; }
            set {_status = value; }
        }
        public string publicDns
        {
            get { return _publicDns; }
            set {_publicDns = value; }
        }
        public string zone
        {
            get { return _zone; }
            set {_zone = value; }
        }
        public string launchTime
        {
            get { return _launchTime; }
            set {_launchTime = value; }
        }
        public string platform
        {
            get { return _platform; }
            set {_platform = value; }
        }

        public string keyPairName
        {
            get { return _keyPairName; }
            set {_keyPairName = value; }
        }

        public CEc2Instance(CAwsConfig amsConfig)
        {
            _awsConfig = amsConfig;
            _service = new AmazonEC2Client(_awsConfig.awsAccessKey, _awsConfig.awsSecretKey);
        }

        public CEc2Instance()
        {
        }

        public string header
        {
            get { return instanceId + publicDnsTitle + publicDns + platformTitle + platform; }
        }

        public string content
        {
            get
            {
                return 
                    "      AMI Id: " + imageId + Environment.NewLine +
                    "      Security Groups: " + securityGroups + Environment.NewLine +
                    "      Type: " + type + Environment.NewLine +
                    "      Status: " + status + Environment.NewLine +
                    "      Key Pair Name: " + keyPairName;
            }
        }

        static public string getInsanceIdFromHeader(string header)
        {
            if (string.IsNullOrEmpty(header) == false)
            {
                return header.Substring(0, header.IndexOf(publicDnsTitle));
            }
            return "";
        }

        static public string getPublicDnsFromHeader(string header) 
        {
            if (string.IsNullOrEmpty(header) == false)
            {
                int dnsIndex = header.IndexOf(publicDnsTitle) + publicDnsTitle.Length;
                int dnslen = header.IndexOf(platformTitle) - dnsIndex;
                return header.Substring(dnsIndex, dnslen);
            }
            return "";
        }

        static public bool isWindowsPlatform(string header)
        {
            if (string.IsNullOrEmpty(header) == false)
            {
                int platformIndex = header.IndexOf(platformTitle) + platformTitle.Length;
                return string.Compare(header.Substring(platformIndex), "windows", true) == 0;
            }
            return false;
        }

        static public string deployableAmiImageId
        {
            get { return jwAmiImageId; }
        }

        public void launch()
        {
            //instance started.
            if (string.IsNullOrEmpty(_instanceId) == false)
            {
                return;
            }

            try
            {
                RunInstancesRequest request = new RunInstancesRequest();

                request.ImageId = _amiId;
                request.MinCount = 1;
                request.MaxCount = 1;

                if (string.IsNullOrEmpty(_securityGroups) == true)
                {
                    createSecurityGroup();
                    request.SecurityGroup.Add(jwSecurityGroupName);
                }
                else
                {
                    request.SecurityGroup.Add(_securityGroups);
                }

                if (string.IsNullOrEmpty(_keyPairName) == true)
                {
                    createKayPair();
                    request.KeyName = jwKeyPairName;
                }
                else
                {
                    request.KeyName = _keyPairName;
                }

                RunInstancesResponse response = _service.RunInstances(request);

                if (response.IsSetRunInstancesResult())
                {
                    RunInstancesResult runInstancesResult = response.RunInstancesResult;
                    if (runInstancesResult.IsSetReservation())
                    {
                        if (runInstancesResult.Reservation.RunningInstance[0].IsSetInstanceId())
                        {
                            _instanceId = runInstancesResult.Reservation.RunningInstance[0].InstanceId;
                        }
                        if (runInstancesResult.Reservation.RunningInstance[0].IsSetPublicDnsName())
                        {
                            _publicDns = runInstancesResult.Reservation.RunningInstance[0].PublicDnsName;
                        }
                    }
                }

                if (string.IsNullOrEmpty(_instanceId) == true)
                    throw new Exception("No instance id is returned.");

                //return after the instance started up
                bool pending = true;
                while (pending == true)
                {
                    System.Threading.Thread.Sleep(5 * 1000);

                    DescribeInstancesRequest describeRequest = new DescribeInstancesRequest();
                    describeRequest.InstanceId.Add(_instanceId);
                    DescribeInstancesResponse describeResponse = _service.DescribeInstances(describeRequest);
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
                throw new Exception("Caught Exception: " + ex.XML);
            }
        }

        public void reboot()
        {
            try
            {
                RebootInstancesRequest request = new RebootInstancesRequest();
                request.InstanceId.Add(_instanceId);

                RebootInstancesResponse response = _service.RebootInstances(request);
            }
            catch (AmazonEC2Exception ex)
            {
                throw new Exception("Caught Exception: " + ex.XML);
            }
        }

        // EC2 doesn't have API to access password
        public string getAdministratorPassord()
        {
            try
            {
                waitForPortReady();

                // /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(
                        "cmd", @"/c ec2-get-password.cmd " +
                        _instanceId +
                        " -k " + CAwsConfig.getEc2BootstrapperDirectory() + "\\" + jwKeyPairFileName);

                procStartInfo.WorkingDirectory = _awsConfig.ec2Home + @"\bin";

                if(!procStartInfo.EnvironmentVariables.ContainsKey("EC2_HOME"))
                    procStartInfo.EnvironmentVariables.Add("EC2_HOME", _awsConfig.ec2Home);
                if (!procStartInfo.EnvironmentVariables.ContainsKey("EC2_CERT"))
                    procStartInfo.EnvironmentVariables.Add("EC2_CERT", _awsConfig.ec2CertPath);
                if (!procStartInfo.EnvironmentVariables.ContainsKey("JAVA_HOME"))
                    procStartInfo.EnvironmentVariables.Add("JAVA_HOME", _awsConfig.javaHome);
                if (!procStartInfo.EnvironmentVariables.ContainsKey("EC2_PRIVATE_KEY"))
                    procStartInfo.EnvironmentVariables.Add("EC2_PRIVATE_KEY", _awsConfig.ec2UserPrivateKeyFile);

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
                string pw = process.StandardOutput.ReadToEnd();
                if (string.IsNullOrEmpty(pw) == false && pw.EndsWith("\r\n"))
                {
                    pw = pw.Substring(0, pw.Length - 2);
                }

                return pw;
            }
            catch (Exception ex)
            {
                throw new Exception("GetAdministratorPassord fails. " + ex.Message);
            }
        }

        public void checkDeploymentStatus(SDeployInfo deployInfo)
        {
            //forever: user can cancel checking at any time
            while (true)
            {
                int status = -2;
                try
                {
                    status = deployInfo.proxy.GetInstallationStatus(deployInfo.installId);
                }
                catch (Exception)
                {
                }

                if (status == 0)
                {
                    break;
                }
                if (status == -2)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                throw new Exception("UploadAndInstallMsiFile failed." + status.ToString());
            }
        }

        public SDeployInfo uploadAndInstallMsi(string adminPassword, string msiPath)
        {
            Stream fileStream = null;
            SDeployInfo deployInfo = new SDeployInfo();
            try
            {
                if (string.IsNullOrEmpty(_publicDns) == true)
                    getSiteUrl();

                waitForPortReady();

                //
                // Get administrator's password
                //
                string password = adminPassword;
                if (string.IsNullOrEmpty(password) == true)
                {
                    password = getAdministratorPassord();
                    if (string.IsNullOrEmpty(password))
                    {
                        throw new Exception("Invalid password.");
                    }
                }

                //user can cancel the thread if wanted.
                while (true)
                {
                    try
                    {
                        downloadAndInstallCertificate();
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(msiPath) == true)
                {
                    //
                    // Get the MSI file
                    //

                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "MSI files (*.msi)|*.msi";
                    if (DialogResult.OK != ofd.ShowDialog())
                    {
                        throw new Exception("Error: open file dialog failed");
                    }
                    fileStream = ofd.OpenFile();
                }
                else
                {
                    fileStream = File.OpenRead(msiPath);
                }

                if (fileStream == null)
                {
                    throw new Exception("Error: failed to open selected file");
                }

                byte[] fileBytes = new byte[fileStream.Length];
                fileStream.Read(fileBytes, 0, (int)fileStream.Length);

                //
                // Create the web service proxy
                //

                deployInfo.proxy = new Ec2FileUploadProxy.Ec2FileUpload();
                deployInfo.proxy.Url = "https://" + _publicDns + "/ec2fileupload/ec2fileupload.asmx";

                //
                // Get client credentials
                //

                CredentialCache cache = new CredentialCache();
                cache.Add(
                    new Uri(deployInfo.proxy.Url),
                    "Basic",
                    new NetworkCredential("administrator", password));
                deployInfo.proxy.Credentials = cache;

                //
                // Call the web service
                //
                while (true)
                {
                    try
                    {
                        deployInfo.installId = deployInfo.proxy.UploadAndInstallMsiFile(
                            "Ec2Install.msi", Convert.ToBase64String(fileBytes));
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }

                if (string.IsNullOrEmpty(deployInfo.installId) == true)
                {
                    throw new Exception("Error: UploadAndInstallMsiFile failed.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UploadAndInstallMsi fails." + ex.Message);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
            return deployInfo;
        }

        private void waitForPortReady()
        {
            while (true)
            {
                try
                {
                    System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient(_publicDns, 80);
                    clnt.Close();

                    clnt = new System.Net.Sockets.TcpClient(_publicDns, 443);
                    clnt.Close();

                    clnt = new System.Net.Sockets.TcpClient(_publicDns, 3389);
                    clnt.Close();

                    break;
                }
                catch (System.Exception)
                {
                }
                Thread.Sleep(1000);
            }
        }

        private void downloadAndInstallCertificate()
        {
            try
            {
                string ec2BootstrapperDir = CAwsConfig.getEc2BootstrapperDirectory();
                if (Directory.Exists(ec2BootstrapperDir) == false)
                    Directory.CreateDirectory(ec2BootstrapperDir);

                string instanceDir = ec2BootstrapperDir + "\\" + _instanceId;
                if (Directory.Exists(instanceDir) == false)
                    Directory.CreateDirectory(instanceDir);                

                string certFilePath = instanceDir + "\\" + jwCertFile;
                if (File.Exists(certFilePath))
                {
                    return;
                }
                
                string req = "http://" + _publicDns + "/" + jwCertFile;
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

                            TextWriter tw = new StreamWriter(certFilePath);
                            tw.Write(responseFromServer);
                            tw.Close();

                            //install certificate
                            installCertificate(certFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DownloadAndInstallCertificate fails." + ex.Message);
            }
        }

        private void installCertificate(string file)
        {
            try
            {
                var serviceRuntimeUserCertificateStore = new X509Store(StoreName.Root);
                serviceRuntimeUserCertificateStore.Open(OpenFlags.ReadWrite);
                X509Certificate2 cert = new X509Certificate2(file);
                serviceRuntimeUserCertificateStore.Add(cert);
                serviceRuntimeUserCertificateStore.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("InstallCertificate fails." + ex.Message);
            }
        }

        private void getSiteUrl()
        {
            try
            {
                DescribeInstancesRequest request = new DescribeInstancesRequest();
                request.InstanceId.Add(_instanceId);
                DescribeInstancesResponse response = _service.DescribeInstances(request);
                DescribeInstancesResult describeInstancesResult = response.DescribeInstancesResult;

                List<Reservation> reservationList = describeInstancesResult.Reservation;

                //need to finish this
                _publicDns = reservationList[0].RunningInstance[0].PublicDnsName;
            }
            catch (AmazonEC2Exception ex)
            {
                throw new Exception("Caught Exception: " + ex.XML);
            }
        }

        private void createSecurityGroup()
        {
            try
            {
                CreateSecurityGroupRequest requestSecurirtyGroup = new CreateSecurityGroupRequest();
                requestSecurirtyGroup.GroupName = jwSecurityGroupName;
                requestSecurirtyGroup.GroupDescription = jwSecurityGroupDescription;
                CreateSecurityGroupResponse responseSecurityGroup = _service.CreateSecurityGroup(requestSecurirtyGroup);

                AuthorizeSecurityGroupIngressRequest requestAuthz = new AuthorizeSecurityGroupIngressRequest();
                requestAuthz.GroupName = jwSecurityGroupName;
                requestAuthz.IpProtocol = "tcp";
                requestAuthz.CidrIp = "0.0.0.0/0";

                decimal[] ports = { 80, 443, 1443, 3389 };
                foreach (decimal port in ports)
                {
                    requestAuthz.FromPort = port;
                    requestAuthz.ToPort = port;
                    AuthorizeSecurityGroupIngressResponse responseAuthz = _service.AuthorizeSecurityGroupIngress(requestAuthz);
                }
            }
            catch (AmazonEC2Exception ex)
            {
                throw new Exception("Caught Exception: " + ex.XML);
            }
        }

        private void createKayPair()
        {
            try
            {
                //check if key pair exists; 
                string keyFileDir = CAwsConfig.getEc2BootstrapperDirectory();
                if(Directory.Exists(keyFileDir) == false)
                {
                    Directory.CreateDirectory(keyFileDir);
                }

                string keyFilePath = keyFileDir + "\\" + jwKeyPairFileName;
                if (File.Exists(keyFilePath))
                    return;

                CreateKeyPairRequest request = new CreateKeyPairRequest();
                request.KeyName = jwKeyPairName;
                CreateKeyPairResponse response = _service.CreateKeyPair(request);
                if (response.IsSetCreateKeyPairResult())
                {
                    CreateKeyPairResult createKeyPairResult = response.CreateKeyPairResult;
                    if (createKeyPairResult.IsSetKeyPair())
                    {
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
                throw new Exception("Caught Exception: " + ex.XML);
            }
        }
    }
}
