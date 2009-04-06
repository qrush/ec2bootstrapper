using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ec2Bootstrapper
{
    //
    // this is the command line tool for ec2bootstrapper. 
    //
    class Program
    {
        static void Main(string[] args)
        {
            if (checkEnvironment() == false)
                return;

            string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            Ec2Bootstrapperlib.CEc2Bootstrapper.Run(accessKey, secretKey);

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
