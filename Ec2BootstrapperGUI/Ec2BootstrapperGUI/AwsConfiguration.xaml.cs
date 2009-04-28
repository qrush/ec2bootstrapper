using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ec2Bootstrapperlib;
using System.Windows.Forms;

namespace Ec2BootstrapperGUI
{
    /// <summary>
    /// Interaction logic for AwsConfiguration.xaml
    /// </summary>
    public partial class AwsConfiguration : Window
    {
        CAwsConfig _config;
        Dashboard _dashboard;

        public AwsConfiguration()
        {
            this.InitializeComponent();
            _config = new CAwsConfig();
            try
            {
                AwsAccessKey.Text = _config.read("AwsAccessKey");
                AwsSecreteKey.Text = _config.read("AwsSecreteKey");
                Ec2CertPath.Text = _config.read("Ec2CertPath");
                Ec2Home.Text = _config.read("Ec2Home");
                Ec2InstancePrivateKey.Text = _config.read("Ec2InstancePrivateKey");
                Ec2UserPrivateKey.Text = _config.read("Ec2UserPrivateKey");
                JavaHome.Text = _config.read("JavaHome");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            SaveButton.IsEnabled = false;
        }

        public void setDashboard(Dashboard db)
        {
            _dashboard = db;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                if (_config != null)
                {
                    _config.write("AwsAccessKey", AwsAccessKey.Text);
                    _config.write("AwsSecreteKey", AwsSecreteKey.Text);
                    _config.write("Ec2CertPath", Ec2CertPath.Text);
                    _config.write("Ec2Home", Ec2Home.Text);
                    _config.write("Ec2InstancePrivateKey", Ec2InstancePrivateKey.Text);
                    _config.write("Ec2UserPrivateKey", Ec2UserPrivateKey.Text);
                    _config.write("JavaHome", JavaHome.Text);
                    _config.commit();
                    this.Close();

                    if (_dashboard != null)
                        _dashboard.checkConfig();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void CertPathBt_Click(object sender, RoutedEventArgs e)
        {
            Ec2CertPath.Text = GetFilePath();
        }

        private void Ec2HomeBt_Click(object sender, RoutedEventArgs e)
        {
            Ec2Home.Text = GetDirectoryPath();
        }

        private void Ec2InstPrivKeyBt_Click(object sender, RoutedEventArgs e)
        {
            Ec2InstancePrivateKey.Text = GetFilePath();
        }

        private void Ec2UserPrivKeyBt_Click(object sender, RoutedEventArgs e)
        {
            Ec2UserPrivateKey.Text = GetFilePath();
        }

        private void JavaHomeBt_Click(object sender, RoutedEventArgs e)
        {
            JavaHome.Text = GetDirectoryPath();
        }

        string GetFilePath()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Key files (*.pem)|*.pem";
            if (System.Windows.Forms.DialogResult.OK != ofd.ShowDialog())
            {
                System.Windows.MessageBox.Show("Error: open file dialog failed");
                return "";
            }
            return ofd.FileName;
        }

        string GetDirectoryPath()
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return folder.SelectedPath;
            }
            else
            {
                System.Windows.MessageBox.Show("Error: FolderBrowserDialog failed");
                return "";
            }
        }

        private void Config_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }
    }
}