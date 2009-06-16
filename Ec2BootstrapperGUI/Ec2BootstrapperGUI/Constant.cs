using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ec2BootstrapperGUI
{
    internal class ConstantString
    {
        public const string ContactAmazon = "Contacting Amazon ...";
        public const string Done = "Done";
        public const string Ready = "Ready";
        public const string NoInstance = "No instances found";
        public const string NoAmi = "No AMI found";
        public const string Launching = "Launching instance ...";
        public const string DeployFailed = "Deploy failed.";
        public const string InstallProgress = "Installation is in progress ...";
        public const string InstanceReady = "Waiting for new instance (~5 minutes)";
        public const string UploadMsi = "Uploading the MSI file ...";
        public const string ThreadAborted = "Thread aborted";
        public const string LaunchFailed = "Launch failed";
        public const string WaitingForExit = "Waiting for exit ...";
    }
}
