using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Threading;

namespace Ec2AppInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            //-1: unknown exception
            int errorCode = -1;
            string msiFile = null;
            string guid = null;
            try
            {
                if (args == null || args.Length != 2)
                {
                    throw new Exception("Null argument");
                }

                msiFile = args[0];
                if (msiFile.EndsWith(".msi") == false ||
                    File.Exists(msiFile) == false)
                {
                    throw new Exception("Invalid argument");
                }
                guid = args[1];

                //this process will stop iis.
                //wait for the web method return
                Thread.Sleep(1000);

                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(
                        "msiexec.exe", @"/qn /i " + msiFile + " /l* c:\\setupLog.txt");

                //redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.LoadUserProfile = true;

                // Do not create a console window.
                procStartInfo.CreateNoWindow = true;

                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process msiExec = new System.Diagnostics.Process();
                msiExec.StartInfo = procStartInfo;
                msiExec.Start();

                msiExec.WaitForExit();
                errorCode = msiExec.ExitCode;
            }
            catch (Exception)
            {
                //Console.WriteLine(ex.Message);
            }

            if (string.IsNullOrEmpty(guid) == false)
            {
                RegistryKey key = Registry.LocalMachine;
                if (key != null)
                {
                    RegistryKey subkey = key.OpenSubKey(@"Software\JW Secure\EC2 Bootstrapper", true);
                    if (subkey != null)
                    {
                        if (subkey.GetValue(guid) == null)
                        {
                            subkey.SetValue("GuidNotExist", guid, RegistryValueKind.String);
                        }
                        else
                        {
                            subkey.SetValue(guid, errorCode, RegistryValueKind.DWord);
                        }
                    }
                }
            }

            if (msiFile != null)
            {
                if (File.Exists(msiFile) == true)
                    File.Delete(msiFile);
            }
        }
    }
}
