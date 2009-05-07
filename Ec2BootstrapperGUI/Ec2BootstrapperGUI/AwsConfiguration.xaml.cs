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
using System.IO;
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
                AwsSecretKey.Text = _config.read("AwsSecretKey");
                Ec2CertPath.Text = _config.read("Ec2CertPath");
                Ec2Home.Text = _config.read("Ec2Home");
                Ec2UserPrivateKey.Text = _config.read("Ec2UserPrivateKey");
                JavaHome.Text = _config.read("JavaHome");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            SaveButton.IsEnabled = false;
        }

        public Dashboard dashboard
        {
            set { _dashboard = value; }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                if (_config != null)
                {
                    _config.write("AwsAccessKey", AwsAccessKey.Text);
                    _config.write("AwsSecretKey", AwsSecretKey.Text);
                    _config.write("Ec2CertPath", Ec2CertPath.Text);
                    _config.write("Ec2Home", Ec2Home.Text);
                    _config.write("Ec2UserPrivateKey", Ec2UserPrivateKey.Text);
                    _config.write("JavaHome", JavaHome.Text);
                    _config.commit();

                    if (_dashboard != null)
                        _dashboard.checkConfig();

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void certPathBt_Click(object sender, RoutedEventArgs e)
        {
            string path = getFilePath("Please select your EC2 certificate file");
            if(string.IsNullOrEmpty(path) == false)
                Ec2CertPath.Text = path;
        }

        private void ec2HomeBt_Click(object sender, RoutedEventArgs e)
        {
            string dir = getDirectoryPath("Please select the directory where EC2 was installed");
            if (string.IsNullOrEmpty(dir) == false)
                Ec2Home.Text = dir;
        }

        private void ec2UserPrivKeyBt_Click(object sender, RoutedEventArgs e)
        {
            string path = getFilePath("Please select your EC2 perivate key file");
            if (string.IsNullOrEmpty(path) == false)
                Ec2UserPrivateKey.Text = path;
        }

        private void javaHomeBt_Click(object sender, RoutedEventArgs e)
        {
            string dir = getDirectoryPath("Please select the directory where Java was installed.");
            if (string.IsNullOrEmpty(dir) == false)
                JavaHome.Text = dir;
        }

        string getFilePath(string title)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Key files (*.pem)|*.pem";
            ofd.Title = title;
            if (System.Windows.Forms.DialogResult.OK == ofd.ShowDialog())
            {
                return ofd.FileName;
            }
            return "";
        }

        string getDirectoryPath(string description)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = description;
            folder.ShowNewFolderButton = false;
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return folder.SelectedPath;
            }
            return "";
        }

        private void config_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CueBannerTextBox tb = (CueBannerTextBox)sender;
            if (tb.Text.Length == 0 || tb.Text == tb.PromptText)
            {
                tb.UsePrompt = true;
                tb.Text = tb.PromptText;
            }
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CueBannerTextBox tb = (CueBannerTextBox)sender;
            if (tb.UsePrompt)
            {
                tb.UsePrompt = false;
                tb.Text = string.Empty;
            }
        }

        private void registerHyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://aws.amazon.com");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}