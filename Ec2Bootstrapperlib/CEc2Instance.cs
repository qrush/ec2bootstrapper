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
using System.Windows.Media;

namespace Ec2Bootstrapperlib
{
    public class CEc2Instance
    {
        const string jwAmiImageId = "ami-0de80964"; //"ami-0529ce6c";// 
        const string jwCertFile                   = "server.crt";
        const string jwSecurityGroupName          = "JWSecureEc2FileLoad";
        const string jwSecurityGroupDescription   = "Used for msi upload";
        const string jwKeyPairName                = "JWSecureFileUploadKey";

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
        bool _defaultSecurityGroup;
        string _img;

        public string img
        {
            get { return _img; }
            set { _img = value; }
        }

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
            set { _securityGroups = value; _defaultSecurityGroup = false; }
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

        private void setDefaults()
        {
            if (string.IsNullOrEmpty(_keyPairName) == true)
                _keyPairName = jwKeyPairName;
            if (string.IsNullOrEmpty(_securityGroups) == true)
            {
                _securityGroups = jwSecurityGroupName;
                _defaultSecurityGroup = true;
            }
            if(string.IsNullOrEmpty(_img) == true)
                _img = "images/ServerStopped.png";
        }

        public bool updateWebStatus()
        {
            string currentImg;
            try
            {
                if (string.IsNullOrEmpty(_publicDns) == false)
                {
                    System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient(_publicDns, 80);
                    clnt.Close();

                    currentImg = "images/ServerRunning.png";
                }
                else
                {
                    currentImg = "images/ServerStopped.png";
                }
            }
            catch (System.Exception)
            {
                currentImg = "images/ServerStopped.png";
            }

            if (string.Compare(img, currentImg) == 0)
            {
                return false;
            }
            else
            {
                img = currentImg;
                return true;
            }
        }

        public CEc2Instance()
        {
            setDefaults();
            _service = new AmazonEC2Client(CAwsConfig.Instance.awsAccessKey, CAwsConfig.Instance.awsSecretKey);
        }

        static public string deployableAmiImageId
        {
            get { return jwAmiImageId; }
        }

        private bool securitryGroupExistOnServer()
        {
            bool exist = false;
            CEc2Service serv = new CEc2Service();
            List<string> sgs = serv.descrbibeSecurityGroups();
            foreach (string sg in sgs)
            {
                if (string.Compare(sg, _securityGroups) == 0)
                {
                    exist = true;
                    break;
                }
            }
            return exist;
        }

        private bool keyExistOnServer()
        {
            bool exist = false;
            CEc2Service serv = new CEc2Service();
            List<string> kps = serv.descrbibeKeyPairs();
            foreach (string kp in kps)
            {
                if (string.Compare(kp, _keyPairName) == 0)
                {
                    exist = true;
                    break;
                }
            }

            if (exist == true)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PEM files (*.pem)|*.pem";
                ofd.InitialDirectory = CAwsConfig.getEc2BootstrapperDirectory();
                ofd.Title = "Select private key file for " + _keyPairName;
                if (System.Windows.Forms.DialogResult.OK == ofd.ShowDialog())
                {
                    CAwsConfig.Instance.setKeyFilePath(_keyPairName, ofd.FileName);
                    CAwsConfig.Instance.commit();
                }
                else
                {
                    throw new Exception("key " + _keyPairName + " is not associated its key file.");
                }
            }
            return exist;
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

                if (_defaultSecurityGroup == false)
                {
                    request.SecurityGroup.Add(_securityGroups);
                }
                else
                {
                    if(securitryGroupExistOnServer() == false)
                    {
                        createSecurityGroup();
                    }
                    request.SecurityGroup.Add(_securityGroups);
                }

                string keyPath = CAwsConfig.Instance.getKeyFilePath(_keyPairName);
                if (string.IsNullOrEmpty(keyPath) == true ||
                    File.Exists(keyPath) == false)
                {
                    if (keyExistOnServer() == false)
                    {
                        createKayPair();
                    }
                }
                request.KeyName = _keyPairName;

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

                string keyPath = CAwsConfig.Instance.getKeyFilePath(_keyPairName);
                if (string.IsNullOrEmpty(keyPath) == true ||
                    File.Exists(keyPath) == false)
                {
                    if (keyExistOnServer() == true)
                    {
                        keyPath = CAwsConfig.Instance.getKeyFilePath(_keyPairName);
                    }
                }

                if (string.IsNullOrEmpty(keyPath) == true ||
                    File.Exists(keyPath) == false)
                {
                    throw new Exception("Cannot find key file.");
                }

                // /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(
                        "cmd", @"/c ec2-get-password.cmd " +
                        _instanceId + " -k \"" + keyPath + "\"");

                procStartInfo.WorkingDirectory = CAwsConfig.Instance.ec2Home + @"\bin";

                if (!procStartInfo.EnvironmentVariables.ContainsKey("EC2_HOME"))
                    procStartInfo.EnvironmentVariables.Add("EC2_HOME", CAwsConfig.Instance.ec2Home);
                if (!procStartInfo.EnvironmentVariables.ContainsKey("EC2_CERT"))
                    procStartInfo.EnvironmentVariables.Add("EC2_CERT", CAwsConfig.Instance.ec2CertPath);
                if (!procStartInfo.EnvironmentVariables.ContainsKey("JAVA_HOME"))
                    procStartInfo.EnvironmentVariables.Add("JAVA_HOME", CAwsConfig.Instance.javaHome);
                if (!procStartInfo.EnvironmentVariables.ContainsKey("EC2_PRIVATE_KEY"))
                    procStartInfo.EnvironmentVariables.Add("EC2_PRIVATE_KEY", CAwsConfig.Instance.ec2UserPrivateKeyFile);

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
            catch (ThreadAbortException ex)
            {
                throw ex;
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

                string fileName;
                if (string.IsNullOrEmpty(msiPath) == true)
                {
                    //
                    // Get the MSI file
                    //

                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "MSI files (*.msi)|*.msi";
                    ofd.Title = "Select your setup program to install on EC2 instance:"; 
                    if (DialogResult.OK != ofd.ShowDialog())
                    {
                        throw new Exception("Error: open file dialog failed");
                    }
                    fileName = Path.GetFileName(ofd.FileName);
                    fileStream = ofd.OpenFile();
                }
                else
                {
                    fileStream = File.OpenRead(msiPath);
                    fileName = Path.GetFileName(msiPath);
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
                            fileName, Convert.ToBase64String(fileBytes));
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

        public void waitForPortReady()
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

        public bool isPortReady()
        {
            try
            {
                System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient(_publicDns, 80);
                clnt.Close();

                clnt = new System.Net.Sockets.TcpClient(_publicDns, 443);
                clnt.Close();

                clnt = new System.Net.Sockets.TcpClient(_publicDns, 3389);
                clnt.Close();

                return true;
            }
            catch (System.Exception)
            {
            }
            return false;
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
                if (File.Exists(certFilePath) == false)
                {
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
                            }
                        }
                    }
                }

                if (File.Exists(certFilePath) == true)
                {
                    //install certificate
                    installCertificate(certFilePath);
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
                
                bool certInstalled = false;
                foreach (X509Certificate2 tc in serviceRuntimeUserCertificateStore.Certificates)
                {
                    if (string.Compare(tc.Thumbprint, cert.Thumbprint) == 0)
                    {
                        certInstalled = true;
                        break;
                    }
                }
                if (certInstalled == false)
                {
                    serviceRuntimeUserCertificateStore.Add(cert);
                }
                
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
                requestSecurirtyGroup.GroupName = _securityGroups;
                requestSecurirtyGroup.GroupDescription = jwSecurityGroupDescription;
                CreateSecurityGroupResponse responseSecurityGroup = _service.CreateSecurityGroup(requestSecurirtyGroup);

                AuthorizeSecurityGroupIngressRequest requestAuthz = new AuthorizeSecurityGroupIngressRequest();
                requestAuthz.GroupName = _securityGroups;
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

        //once we get here we know the key file doesn't exist
        private void createKayPair()
        {
            try
            {
                string keyFileDir = CAwsConfig.getEc2BootstrapperDirectory();
                if(Directory.Exists(keyFileDir) == false)
                {
                    Directory.CreateDirectory(keyFileDir);
                }

                string keyFilePath = null;

                FolderBrowserDialog folder = new FolderBrowserDialog();
                folder.ShowNewFolderButton = true;
                folder.SelectedPath = keyFileDir;
                folder.Description = "Please select directory where you want to save key file";
                DialogResult result = DialogResult.No;
                while (result == DialogResult.No)
                {
                    if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        keyFilePath = folder.SelectedPath + "\\" + _keyPairName + ".pem";
                        if (File.Exists(keyFilePath))
                        {
                            result = MessageBox.Show(null, "Key file " + keyFilePath + " exists. Do you want to overwrite it?", "Key File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                CreateKeyPairRequest request = new CreateKeyPairRequest();
                request.KeyName = _keyPairName;
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
                                CAwsConfig.Instance.setKeyFilePath(_keyPairName, keyFilePath);
                                CAwsConfig.Instance.commit();
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
