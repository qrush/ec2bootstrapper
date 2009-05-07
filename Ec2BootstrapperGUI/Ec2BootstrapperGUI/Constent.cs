using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ec2BootstrapperGUI
{
    internal class ConstantString
    {
        public const string ContactAmazon = "Contacting Amazon...";
        public const string Done = "Done";
        public const string Ready = "Ready";
        public const string NoInstance = "No instance found";
        public const string NoAmi = "No Ami found";
        public const string Launching = "Launching instance...";
        public const string DeployFailed = "Deploy failed.";
        public const string InstallProgress = "Installation is in progress...";
        public const string InstanceReady = "Wait for instance ready(~5 minutes)";
        public const string UploadMsi = "Uploading your msi file...";
        public const string ThreadAborted = "Thread aborted";
    }
}
