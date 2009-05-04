using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using Ec2Bootstrapperlib;
using System.Windows.Media.Animation;

namespace Ec2BootstrapperGUI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        //this is the only instance of config being used.
        CAwsConfig _awsConfig;

        public Dashboard()
        {
            this.InitializeComponent();            
            checkConfig();
        }

        public CAwsConfig awsConfig
        {
            get { return _awsConfig; }
        }

        public void checkConfig()
        {
            _awsConfig = new CAwsConfig();

            if (_awsConfig.isConfigured())
            {
                showInstances();
            }
            else
            {
                ProgBar.Visibility = Visibility.Hidden;

                AwsConfiguration config = new AwsConfiguration();
                config.dashboard = this;
                config.ShowDialog();
            }
        }

        private void showInstances()
        {
            this.clientR.Children.Clear();

            InstanceList instList = new InstanceList();
            UserControl userControl = instList as UserControl;

            if (userControl != null)
            {
                userControl.VerticalAlignment = VerticalAlignment.Stretch;
                userControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.clientR.Children.Insert(this.clientR.Children.Count, userControl);
                instList.dashboard = this;
            }
        }

        //file menu
        private void launchInstance_Click(object sender, RoutedEventArgs e)
        {
            AmiPicker newInstance = new AmiPicker();
            newInstance.dashboard = this;
            newInstance.Show();
        }

        private void deploy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InstanceList inslist = this.clientR.Children[0] as InstanceList;
                if (inslist != null)
                {
                    if (inslist.instancesLV.SelectedItem != null)
                    {
                        CEc2Instance inst = inslist.instancesLV.SelectedItem as CEc2Instance;
                        if (inst != null)
                        {
                            AppDeployment pw = new AppDeployment();
                            pw.instance = inst;
                            pw.Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //view menu
        private void configureMenu_Click(object sender, RoutedEventArgs e)
        {
            AwsConfiguration config = new AwsConfiguration();
            config.dashboard = this;
            config.ShowDialog();
        }

        private void remoteConnect_Click(object sender, RoutedEventArgs e)
        {
            string argument = null;
            InstanceList inslist = this.clientR.Children[0] as InstanceList;
            if (inslist != null)
            {
                if (inslist.instancesLV.SelectedItem != null)
                {
                    CEc2Instance inst = inslist.instancesLV.SelectedItem as CEc2Instance;
                    if (inst != null)
                    {
                        if (string.IsNullOrEmpty(inst.publicDns) == false)
                        {
                            argument = "/v:" + inst.publicDns;
                        }
                    }
                }
            }


            System.Diagnostics.ProcessStartInfo procStartInfo;
            if(string.IsNullOrEmpty(argument) == true)
                 procStartInfo = new System.Diagnostics.ProcessStartInfo("mstsc.exe ");
            else
                procStartInfo = new System.Diagnostics.ProcessStartInfo("mstsc.exe ", argument);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = procStartInfo;
            process.Start();
        }

        //help menu
        private void aboutMenu_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void refreshMenu_Click(object sender, RoutedEventArgs e)
        {
            showInstances();
        }

        public void enableProgressBar()
        {
            ProgBar.Visibility = Visibility.Visible;
            ProgBar.IsIndeterminate = true;
            Duration duration = new Duration(TimeSpan.FromSeconds(10));
            DoubleAnimation doubleanimation = new DoubleAnimation(200.0, duration);
            ProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, doubleanimation);
        }

        public void disableProgressBar()
        {
            ProgBar.IsIndeterminate = false;
            ProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, null);
            ProgBar.Visibility = Visibility.Hidden; 
        }

        public void showStatus(string status)
        {
            StatusDesc.Content = status;
            if (string.Compare(status, ConstantString.NoInstance) == 0)
            {
                MessageBox.Show("No machines were enumerated. If you suspect an error, check the configuration dialog from menu Tools | AWS Configuration.",
                    "Display Instances", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}