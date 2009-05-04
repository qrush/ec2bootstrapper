using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ec2Bootstrapperlib;

namespace Ec2Bootstrapper
{
    //
    // this is the command line tool for ec2bootstrapper. 
    //
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args != null)
                {
                    if (args[0] == "-h" || args[0] == "/h")
                    {
                        Console.WriteLine("This will launch an instance of ami-11af4978 and prompt you to install a msi program on the created instance. You will be charged by Amazon on an hourly rate.");
                        Console.WriteLine("Make sure these environent variables are set:");
                        Console.WriteLine("    AWS_ACCESS_KEY_ID");
                        Console.WriteLine("    AWS_SECRET_ACCESS_KEY");
                        Console.WriteLine("    EC2_PRIVATE_KEY");
                        Console.WriteLine("    EC2_HOME");
                        Console.WriteLine("    JAVA_HOME");
                        Console.WriteLine("Then run Ec2Bootstrapper [password]");
                        Console.WriteLine("where the password is the AMI instance administrator's password. It is optional if you have not changed it yet.");
                        return;
                    }
                }
                if (checkEnvironment() == false)
                    return;

                string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
                string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

                Ec2Bootstrapperlib.CEc2Instance instance = new Ec2Bootstrapperlib.CEc2Instance();
                instance.imageId = Ec2Bootstrapperlib.CEc2Instance.deployableAmiImageId;
                instance.launch();
                CEc2Instance.SDeployInfo deployInfo = instance.uploadAndInstallMsi(args[0], null);
                if (string.IsNullOrEmpty(deployInfo.installId) == true)
                {
                    Console.WriteLine("Installation is not started on the instance successfully.");
                    return;
                }
                Console.WriteLine("Installation is started on the instance successfully. Checking installation progress...");
                instance.checkDeploymentStatus(deployInfo);

                Console.WriteLine("Installtaion succeeds");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception: " + ex.Message);
            }
        }

        static bool checkEnvironmentVariable(string var)
        {
            string value = Environment.GetEnvironmentVariable(var);
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Please set environment variable " + var);
                return false;
            }
            return true;
        }

        static bool checkEnvironment()
        {
            string[] vars = {
                    "AWS_ACCESS_KEY_ID",
                    "AWS_SECRET_ACCESS_KEY",
                    "EC2_PRIVATE_KEY",
                    "EC2_HOME",
            };
            foreach (string var in vars)
                if (checkEnvironmentVariable(var) == false)
                    return false;
            return true;
        }
    }
}
