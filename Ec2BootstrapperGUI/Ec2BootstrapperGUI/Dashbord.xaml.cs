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
                showInstructions();
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

        private void showInstructions()
        {
            UserControl userControl = this.GetType().Assembly.CreateInstance(typeof(RegisterOrConfig).FullName) as UserControl;

            if (userControl != null)
            {
                userControl.VerticalAlignment = VerticalAlignment.Stretch;
                userControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.clientR.Children.Insert(this.clientR.Children.Count, userControl);
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
                            PasswordPrompt pw = new PasswordPrompt();
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
            config.Show();
        }

        private void remoteConnect_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo procStartInfo =
                 new System.Diagnostics.ProcessStartInfo("mstsc.exe ");

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
        }

        private void layoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StatusBar.Width = LayoutRoot.ActualWidth;            
        }
    }
}