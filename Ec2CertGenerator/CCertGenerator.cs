using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Net;

namespace Ec2CertGenerator
{
    public partial class CCertGenerator : ServiceBase
    {
        const string Address = "http://169.254.169.254/2008-09-01/meta-data/public-hostname";

        public CCertGenerator()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("Ec2CertGenerator"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "Ec2CertGenerator", "Application");
            }
            eventLog1.Source = "Ec2CertGenerator";
            eventLog1.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            //this should not take too long.
            try
            {                
                if (File.Exists(@"C:\Scripts\MakeSslBinding.cmd") == false)
                {
                    return;
                }

                string hostName;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Address);
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
                            hostName = reader.ReadToEnd();
                        }
                    }
                }

                if (string.IsNullOrEmpty(hostName) == true)
                {
                    throw new Exception("Could not fetch public DNS");
                }

                eventLog1.WriteEntry("Retrieved public DNS is " + hostName, EventLogEntryType.Information);

                ProcessStartInfo startInfo = new ProcessStartInfo(
                    @"C:\Scripts\MakeSslBinding.cmd ");
                startInfo.Arguments = hostName;

                Process msiExec = Process.Start(startInfo);
                msiExec.WaitForExit();
                int error = msiExec.ExitCode;
                if (error != 0)
                {
                    throw new Exception("MakeSslBinding.cmd returns error " + error.ToString());
                }

                File.Move(@"C:\Scripts\MakeSslBinding.cmd", @"C:\Scripts\MakeSslBinding_done.cmd");

                eventLog1.WriteEntry("MakeSslBinding.cmd finished successfully.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                //log an entry to event log here.
                eventLog1.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            //change the property of this service to manually startup if possible.
        }
    }
}
