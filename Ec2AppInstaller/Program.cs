using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;

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

                errorCode = install(msiFile);
                if (errorCode != 0)
                {
                    //try to uninstall and then reinstall
                    try
                    {
                        Thread.Sleep(1000);
                        uninstall(msiFile);
                    }
                    catch (Exception)
                    {
                    }
                    errorCode = install(msiFile);
                }
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

        private static int install(string msiFile)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("msiexec.exe",
                @"/i " + msiFile + " /qn /l* c:\\jwInstallLog.txt");

            //redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;

            // Do not create a console window.
            procStartInfo.CreateNoWindow = true;

            // Now we create a process, assign its ProcessStartInfo and start it
            Process msiExec = new Process();
            msiExec.StartInfo = procStartInfo;
            msiExec.Start();

            msiExec.WaitForExit();
            return msiExec.ExitCode;
        }

        private static void uninstall(string msiFile)
        {
            ProcessStartInfo info = new ProcessStartInfo("msiexec.exe",
                @"/x " + msiFile + " /qn /l* c:\\jwUninstallLog.txt");
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();
        }
    }
}
