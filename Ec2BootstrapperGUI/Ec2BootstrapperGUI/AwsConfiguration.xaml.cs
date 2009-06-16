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
        Dashboard _dashboard;

        public AwsConfiguration()
        {
            this.InitializeComponent();
            try
            {
                AwsAccessKey.Text = CAwsConfig.Instance.read("AwsAccessKey");
                AwsSecretKey.Text = CAwsConfig.Instance.read("AwsSecretKey");
                Ec2CertPath.Text = CAwsConfig.Instance.read("Ec2CertPath");
                Ec2Home.Text = CAwsConfig.Instance.read("Ec2Home");
                Ec2UserPrivateKey.Text = CAwsConfig.Instance.read("Ec2UserPrivateKey");
                JavaHome.Text = CAwsConfig.Instance.read("JavaHome");
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

        private void saveModificationAndClose()
        {
            try
            {
                CAwsConfig.Instance.write("AwsAccessKey", AwsAccessKey.ActucalText);
                CAwsConfig.Instance.write("AwsSecretKey", AwsSecretKey.ActucalText);
                CAwsConfig.Instance.write("Ec2CertPath", Ec2CertPath.ActucalText);
                CAwsConfig.Instance.write("Ec2Home", Ec2Home.ActucalText);
                CAwsConfig.Instance.write("Ec2UserPrivateKey", Ec2UserPrivateKey.ActucalText);
                CAwsConfig.Instance.write("JavaHome", JavaHome.ActucalText);
                CAwsConfig.Instance.commit();

                if (_dashboard != null)
                    _dashboard.configurationReady();

                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private bool verifyConfiguration()
        {
            //access key
            if (string.IsNullOrEmpty(AwsAccessKey.ActucalText) == true)
            {
                System.Windows.MessageBox.Show(
                    "AWS Access Key field cannot be empty. Please enter your access key.",
                    "Incorrect Access Key", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            //secret key
            if (string.IsNullOrEmpty(AwsSecretKey.ActucalText) == true)
            {
                System.Windows.MessageBox.Show(
                    "AWS Secret Key field cannot be empty. Please enter your secret key.",
                    "Incorrect Secret Key", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            //certificate path
            if (string.IsNullOrEmpty(Ec2CertPath.ActucalText) == true)
            {
                System.Windows.MessageBox.Show(
                    "Ec2 User Certificate field cannot be empty. Please enter the correct value.",
                    "Incorrect EC2 User Certificate File",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (File.Exists(Ec2CertPath.ActucalText) == false)
            {
                System.Windows.MessageBox.Show(
                    "Cannot find " + Ec2CertPath.ActucalText + ". Please correct EC2 Certificate field.",
                    "Incorrect EC2 User Certificate File",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                if (Ec2CertPath.UsePrompt == false)
                    Ec2CertPath.Foreground = Brushes.Red;
                return false;
            }

            //ec2 home
            if (string.IsNullOrEmpty(Ec2Home.ActucalText) == true)
            {
                System.Windows.MessageBox.Show(
                    "EC2 Home field cannot be empty. Please enter EC2 Home path.",
                    "Incorrect EC2Home", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (File.Exists(Ec2Home.ActucalText + @"\bin\ec2-get-password.cmd") == false)
            {
                System.Windows.MessageBox.Show(
                    "Cannot find " + Ec2Home.ActucalText + @"\bin\ec2-get-password.cmd. Please correct EC2 Home path.",
                    "Incorrect EC2Home", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                if(Ec2Home.UsePrompt == false)
                    Ec2Home.Foreground = Brushes.Red;
                return false;
            }

            //user private key
            if (string.IsNullOrEmpty(Ec2UserPrivateKey.ActucalText) == true)
            {
                System.Windows.MessageBox.Show(
                    "EC2 User Private Key field cannot be empty. Please enter the correct value.",
                    "Incorrect EC2 User Private Key File",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (File.Exists(Ec2UserPrivateKey.ActucalText) == false)
            {
                System.Windows.MessageBox.Show(
                    "Cannot find " + Ec2UserPrivateKey.ActucalText + ". Please correct EC2 User Private Key field.",
                    "Incorrect EC2 User Private Key File", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                if (Ec2UserPrivateKey.UsePrompt == false)
                    Ec2UserPrivateKey.Foreground = Brushes.Red;
                return false;
            }

            //java home
            if (string.IsNullOrEmpty(JavaHome.ActucalText) == true)
            {
                System.Windows.MessageBox.Show(
                    "Java Home field cannot be empty. Please enter Java Home path.",
                    "Incorrect JavaHome",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (File.Exists(JavaHome.ActucalText + @"\bin\java.exe") == false)
            {
                System.Windows.MessageBox.Show(
                    "Cannot find " + JavaHome.ActucalText + @"\bin\java.exe. Please correct Java Home path.",
                    "Incorrect JavaHome",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                if (JavaHome.UsePrompt == false)
                    JavaHome.Foreground = Brushes.Red;
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(verifyConfiguration() == true)
                saveModificationAndClose();
        }

        private void certPathBt_Click(object sender, RoutedEventArgs e)
        {
            string path = getFilePath("Please select your EC2 certificate file");
            if (string.IsNullOrEmpty(path) == false)
            {
                Ec2CertPath.Focus();
                Ec2CertPath.Text = path;
                //move the cursor to the end
                Ec2CertPath.SelectionStart = Ec2CertPath.Text.Length;
                Ec2CertPath.SelectionLength = 0;
            }
        }

        private void ec2HomeBt_Click(object sender, RoutedEventArgs e)
        {            
            string dir = getDirectoryPath("Please select the directory where EC2 was installed");
            if (string.IsNullOrEmpty(dir) == false)
            {
                if (File.Exists(dir + @"\bin\ec2-get-password.cmd") == false)
                {
                    dir = getDirectoryPath("Invalid EC2 Home folder. Please select the directory where EC2 was installed.");
                }

                if (File.Exists(dir + @"\bin\ec2-get-password.cmd") == true)
                {
                    Ec2Home.Focus();
                    Ec2Home.Text = dir;
                    //move the cursor to the end
                    Ec2Home.SelectionStart = Ec2Home.Text.Length;
                    Ec2Home.SelectionLength = 0;
                }
            }
        }

        private void ec2UserPrivKeyBt_Click(object sender, RoutedEventArgs e)
        {
            string path = getFilePath("Please select your EC2 perivate key file");
            if (string.IsNullOrEmpty(path) == false)
            {
                Ec2UserPrivateKey.Focus();
                Ec2UserPrivateKey.Text = path;
                //move the cursor to the end
                Ec2UserPrivateKey.SelectionStart = Ec2UserPrivateKey.Text.Length;
                Ec2UserPrivateKey.SelectionLength = 0;
            }
        }

        private void javaHomeBt_Click(object sender, RoutedEventArgs e)
        {
            string dir = getDirectoryPath("Please select the directory where Java was installed.");
            if (string.IsNullOrEmpty(dir) == false)
            {
                if (File.Exists(dir + @"\bin\java.exe") == false)
                {
                    dir = getDirectoryPath("Invalid Java Home folder. Please select the directory where Java was installed.");
                }

                if (File.Exists(dir + @"\bin\java.exe") == true)
                {
                    JavaHome.Focus();
                    JavaHome.Text = dir;
                    //move the cursor to the end
                    JavaHome.SelectionStart = JavaHome.Text.Length;
                    JavaHome.SelectionLength = 0;
                }
            }
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
            SaveButton.IsEnabled =
                string.Compare(CAwsConfig.Instance.read("AwsAccessKey"), AwsAccessKey.ActucalText) != 0 ||
                string.Compare(CAwsConfig.Instance.read("AwsSecretKey"), AwsSecretKey.ActucalText) != 0 ||
                string.Compare(CAwsConfig.Instance.read("Ec2CertPath"), Ec2CertPath.ActucalText) != 0 ||
                string.Compare(CAwsConfig.Instance.read("Ec2Home"), Ec2Home.ActucalText) != 0 ||
                string.Compare(CAwsConfig.Instance.read("Ec2UserPrivateKey"), Ec2UserPrivateKey.ActucalText) != 0 ||
                string.Compare(CAwsConfig.Instance.read("JavaHome"), JavaHome.ActucalText) != 0;

            CueBannerTextBox tb = (CueBannerTextBox)sender;
            if (tb.UsePrompt)
                tb.Foreground = Brushes.Gray;
            else
                tb.Foreground = Brushes.Black;
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
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            btnClose_Click(sender, e);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (SaveButton.IsEnabled == true)
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("You have made changes to the configurations. Do you want to save your changes?",
                    "Configuration", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (verifyConfiguration() == true)
                        saveModificationAndClose();
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
        private void TitleBarGloss_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}