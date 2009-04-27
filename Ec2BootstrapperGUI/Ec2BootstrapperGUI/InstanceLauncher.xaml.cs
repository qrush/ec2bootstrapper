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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Ec2Bootstrapperlib;
using System.Windows.Media.Animation;

namespace Ec2BootstrapperGUI
{
	/// <summary>
	/// Interaction logic for Launch.xaml
	/// </summary>
	public partial class InstanceLauncher
    {
        List<string> _keyPairs;
        List<string> _securityGroups;
        List<string> _zones;
        AmiPicker _amipicker;
        string _amiId;
        string _selectedKeyPair;
        string _selectedSecurityGroups;
        string _selectedZone;
        Dashboard _dashboard;

        public InstanceLauncher()
        {
            this.InitializeComponent();
            _keyPairs = new List<string>();
            _securityGroups = new List<string>();
            _zones = new List<string>();
            LaunchProgBar.Visibility = Visibility.Hidden;
        }

        public List<string> keyPairs
        {
            get { return _keyPairs; }
        }

        private void fetchInformationFromAms()
        {
            if (_securityGroups.Count == 0 ||
                _keyPairs.Count == 0 ||
                _zones.Count == 0)
            {
                enableProgressBar();
                Thread oThread = new Thread(new ThreadStart(fetchInforThread));
                oThread.Start();
            }
        }

        private delegate void StopProgressbarCallback();

        private void fetchInforThread()
        {
            CEc2Service serv = new CEc2Service(_dashboard.awsConfig);
            if (_securityGroups.Count == 0)
            {  
                List<string> sgs = serv.descrbibeSecurityGroups();
                foreach (string sg in sgs)
                    _securityGroups.Add(sg);

            }
            if(_keyPairs.Count == 0)
            {
                List<string> kps = serv.descrbibeKeyPairs();
                foreach (string kp in kps)
                    _keyPairs.Add(kp);
            }
            if (_zones.Count == 0)
            {
                List<string> zs = serv.descrbibeZones();
                foreach (string z in zs)
                    _zones.Add(z);
            }
            Dispatcher.Invoke(new StopProgressbarCallback(disableProgressBar));
        }

        public List<string> securityGroups
        {
            get { return _securityGroups; }
        }

        public List<string> zones
        {
            get { return _zones; }
        }

        public AmiPicker amiPicker
        {
            set { _amipicker = value; }
        }

        public string amiId
        {
            set { AmiIdLb.Content = value; }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (_amipicker != null)
            {
                _amipicker.Show();
                this.Hide();
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_amipicker != null)
                _amipicker.Close();
        }

        private void small_Click(object sender, RoutedEventArgs e)
        {
            Medium.IsChecked = !Small.IsChecked;
        }

        private void medium_Click(object sender, RoutedEventArgs e)
        {
            Small.IsChecked = !Medium.IsChecked;
        }

        public Dashboard dashboard
        {
            set
            {
                _dashboard = value;
                fetchInformationFromAms();
            }
        }

        private void enableProgressBar()
        {
            LaunchProgBar.Visibility = Visibility.Visible;
            LaunchProgBar.IsIndeterminate = true;
            Duration duration = new Duration(TimeSpan.FromSeconds(10));
            DoubleAnimation doubleanimation = new DoubleAnimation(200.0, duration);
            LaunchProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, doubleanimation);
        }

        private void disableProgressBar()
        {
            LaunchProgBar.IsIndeterminate = false;
            LaunchProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, null);
            LaunchProgBar.Visibility = Visibility.Hidden;
        }

        private void launch()
        {
            CEc2Instance inst = new CEc2Instance(_dashboard.awsConfig);
            inst.imageId = _amiId;
            inst.keyPairName = _selectedKeyPair;
            inst.securityGroups = _selectedSecurityGroups;
            
            inst.launch();

            Dispatcher.Invoke(new StopProgressbarCallback(disableProgressBar));
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            //other thread will access these
            _amiId = AmiIdLb.Content.ToString();
            if(KeyPairComb.SelectedValue != null)
                _selectedKeyPair = KeyPairComb.SelectedValue.ToString();
            if(SecurityGroupComb.SelectedValue != null)
                _selectedSecurityGroups = SecurityGroupComb.SelectedValue.ToString();
            if(ZoneComb.SelectedValue != null)
                _selectedZone = ZoneComb.SelectedValue.ToString(); ;
          
            Thread oThread = new Thread(new ThreadStart(launch));
            oThread.Start();
            enableProgressBar();
        }

        private void launcherLayout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LaunchProgBar.Width = LauncherLayout.ActualWidth;
        }

        //default key pair and security group if default ami
    }
}