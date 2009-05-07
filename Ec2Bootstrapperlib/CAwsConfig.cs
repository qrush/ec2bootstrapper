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
        string _awsAccessKey;
        string _awsSecretKey;
        string _ec2CertPath;
        string _ec2Home;
        string _ec2UserPrivateKeyFile;
        string _javaHome;

        Configuration _config;

        public CAwsConfig()
        {
            try
            {
                string configFile = getEc2BootstrapperDirectory();
                string thisExe = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

                configFile += "\\" + thisExe + ".config";
                _config = ConfigurationManager.OpenExeConfiguration(configFile);

                readAll();
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
            get { return _awsAccessKey; }
        }

        public string awsSecretKey
        {
            get { return _awsSecretKey; }
        }

        public string ec2CertPath
        {
            get { return _ec2CertPath; }
        }

        public string ec2Home
        {
            get { return _ec2Home; }
        }

        public string ec2UserPrivateKeyFile
        {
            get { return _ec2UserPrivateKeyFile; }
        }

        public string javaHome
        {
            get { return _javaHome; }
        }

        void readAll()
        {
            _awsAccessKey = read("AwsAccessKey");
            _awsSecretKey = read("AwsSecretKey");
            _ec2CertPath = read("Ec2CertPath");
            _ec2Home = read("Ec2Home");
            _ec2UserPrivateKeyFile = read("Ec2UserPrivateKey");
            _javaHome = read("JavaHome");
        }

        public bool isConfigured()
        {
            try
            {
                return !(string.IsNullOrEmpty(_ec2Home) ||
                   string.IsNullOrEmpty(_awsSecretKey)  ||
                   string.IsNullOrEmpty(_awsAccessKey)  ||
                   string.IsNullOrEmpty(_ec2CertPath)   ||
                   string.IsNullOrEmpty(_javaHome));
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
                throw new Exception("Not installed properly");
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
                //throw new Exception("Not installed properly");
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
