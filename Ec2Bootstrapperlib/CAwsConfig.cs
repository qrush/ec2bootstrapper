using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace Ec2Bootstrapperlib
{
    public class CAwsConfig
    {
        Configuration _config;

        public CAwsConfig()
        {
            try
            {
                string configFile = getEc2BootstrapperDirectory();
                string thisExe = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

                configFile += "\\" + thisExe + ".config";
                _config = ConfigurationManager.OpenExeConfiguration(configFile);
            }
            catch (Exception)
            {
            }
        }

        static public string getEc2BootstrapperDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         "\\JW Secure\\EC2 Bootstrapper";
        }

        public string awsAccessKey
        {
            get { return read("AwsAccessKey"); }
        }

        public string awsSecretKey
        {
            get { return read("AwsSecretKey"); }
        }

        public string ec2CertPath
        {
            get { return read("Ec2CertPath"); }
        }

        public string ec2Home
        {
            get { return read("Ec2Home"); }
        }

        public string ec2UserPrivateKeyFile
        {
            get { return read("Ec2UserPrivateKey"); }
        }

        public string javaHome
        {
            get { return read("JavaHome"); }
        }

        public bool isConfigured()
        {
            try
            {
                bool configured = !(string.IsNullOrEmpty(ec2Home) ||
                   string.IsNullOrEmpty(awsSecretKey) ||
                   string.IsNullOrEmpty(awsAccessKey) ||
                   string.IsNullOrEmpty(ec2CertPath) ||
                   string.IsNullOrEmpty(javaHome) ||
                   string.IsNullOrEmpty(ec2UserPrivateKeyFile));
                if(configured == false)
                    return false;
                
                //check by picking an script file
                if(File.Exists(ec2Home + @"\bin\ec2-get-password.cmd") == false)
                    return false;

                if (File.Exists(javaHome + @"\bin\java.exe") == false)
                    return false;

                if (File.Exists(ec2UserPrivateKeyFile) == false)
                    return false;

                if (File.Exists(ec2CertPath) == false)
                    return false;

                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public string read(string key)
        {
            if (_config == null)
                throw new Exception("Not installed properly");
            if (_config.AppSettings.Settings[key] == null)
                return null;
            return _config.AppSettings.Settings[key].Value;
        }

        public void write(string key, string value)
        {
            if (_config == null)
                throw new Exception("Not installed properly");
            KeyValueConfigurationElement keyElem = _config.AppSettings.Settings[key];
            if (keyElem == null)
            {
                _config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                keyElem.Value = value;
            }
        }

        public void commit()
        {
            if (_config == null)
                throw new Exception("Not installed properly");
            _config.Save(ConfigurationSaveMode.Modified);
        }

        public string getKeyFilePath(string keyName)
        {
            try
            {
                return read(keyName);
            }
            catch (Exception)
            {
            }
            return "";
        }

        public void setKeyFilePath(string keyName, string keyFilePath)
        {
            write(keyName, keyFilePath);
        }
    }
}
