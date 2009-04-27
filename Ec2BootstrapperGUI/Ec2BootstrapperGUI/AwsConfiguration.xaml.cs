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
                MessageBox.Show(ex.Message);
            }
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
                MessageBox.Show(ex.Message);
            }
        }
    }
}