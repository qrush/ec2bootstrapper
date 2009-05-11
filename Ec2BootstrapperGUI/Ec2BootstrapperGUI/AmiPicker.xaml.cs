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
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media.Animation;

namespace Ec2BootstrapperGUI
{
    /// <summary>
    /// Interaction logic for CreateInstance.xaml
    /// </summary>
    public partial class AmiPicker : Window
    {
        List<Border> _lstBorders = new List<Border>();
        List<CEc2Ami> _commAmis;
        List<CEc2Ami> _quickAmis;
        List<CEc2Ami> _myAmis;
        CBeginInvokeOC<CEc2Ami> _amis;
        Dashboard _dashboard;

        public AmiPicker(Dashboard db)
        {
            this.InitializeComponent();
            _amis = new CBeginInvokeOC<CEc2Ami>(this.Dispatcher);
            setQuickAmis();

            NextButton.IsEnabled = false;
            AmiProgBar.Visibility = Visibility.Hidden;
            _dashboard = db;
        }

        public CBeginInvokeOC<CEc2Ami> amis
        {
            get { return _amis; }
            set { _amis = value; }
        }

        private delegate void StopProgressbarCallback();

        void fetchCommunityAmis()
        {
            if (_commAmis == null || _commAmis.Count == 0)
            {
                CEc2Service serv = new CEc2Service();
                _commAmis = serv.describeImages(null);
            }
        }
               
        void fetchQuickAmis()
        {
            if (_quickAmis == null || _quickAmis.Count == 0)
            {
                if (_quickAmis == null)
                    _quickAmis = new List<CEc2Ami>();

                CEc2Ami ami = new CEc2Ami();
                ami.imageId = CEc2Instance.deployableAmiImageId;
                ami.platform = "windows";
                _quickAmis.Add(ami);
            }
        }

        void fetchMyAmis()
        {
            if (_myAmis == null || _myAmis.Count == 0)
            {
                CEc2Service serv = new CEc2Service();
                _myAmis = serv.describeImages("self");
            }
        }
        
        private void border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = (Border)sender;
            border.Width = Amis.ActualWidth - 15;
            _lstBorders.Add(border);            
        }

        private void amis_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Border b in _lstBorders)
            {
                b.Width = Amis.ActualWidth - 15;
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Amis.SelectedItem != null)
            {
                InstanceLauncher launcher = new InstanceLauncher(_dashboard);
                launcher.amiPicker = this;
                launcher.amiId = ((CEc2Ami)Amis.SelectedItem).imageId;

                launcher.ShowDialog();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void amis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Amis.SelectedItem != null)
                NextButton.IsEnabled = true;
        }

        private void setQuickAmis()
        {
            fetchQuickAmis();
            foreach (CEc2Ami item in _quickAmis)
                _amis.Add(item);
        }

        //change the listview binding here
        private void quickStartAmis_Click(object sender, RoutedEventArgs e)
        {
            if (QuickStartAmis.IsChecked == true)
            {
                OwnAmis.IsChecked = false;
                Community.IsChecked = false;
                NextButton.IsEnabled = false;
                _amis.Clear();

                setQuickAmis();
            }
        }

        private void setOwnAmis()
        {
            try
            {
                fetchMyAmis();
                foreach (CEc2Ami item in _myAmis)
                    _amis.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Dispatcher.Invoke(new StopProgressbarCallback(disableProgressBar));
        }

        private void ownAmis_Click(object sender, RoutedEventArgs e)
        {
            if (OwnAmis.IsChecked == true)
            {
                QuickStartAmis.IsChecked = false;
                Community.IsChecked = false;
                NextButton.IsEnabled = false;
                _amis.Clear();

                Thread oThread = new Thread(new ThreadStart(setOwnAmis));
                oThread.Start();
                enableProgressBar();
            }
        }

        private void setCommunityAmis()
        {
            try
            {
                fetchCommunityAmis();
                foreach (CEc2Ami item in _commAmis)
                    _amis.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Dispatcher.Invoke(new StopProgressbarCallback(disableProgressBar));
        }

        private void community_Click(object sender, RoutedEventArgs e)
        {
            if (Community.IsChecked == true)
            {
                QuickStartAmis.IsChecked = false;
                OwnAmis.IsChecked = false;
                NextButton.IsEnabled = false;

                _amis.Clear();

                Thread oThread = new Thread(new ThreadStart(setCommunityAmis));
                oThread.Start();
                enableProgressBar();
            }
        }

        private void enableProgressBar()
        {
            StatusDesc.Content = ConstantString.ContactAmazon;
            AmiProgBar.Visibility = Visibility.Visible;
            AmiProgBar.IsIndeterminate = true;
            Duration duration = new Duration(TimeSpan.FromSeconds(10));
            DoubleAnimation doubleanimation = new DoubleAnimation(200.0, duration);
            AmiProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, doubleanimation);
        }

        private void disableProgressBar()
        {
            AmiProgBar.IsIndeterminate = false;
            AmiProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, null);
            AmiProgBar.Visibility = Visibility.Hidden;

            if (_amis == null || _amis.Count == 0)
                StatusDesc.Content = ConstantString.NoAmi;
            else
                StatusDesc.Content = ConstantString.Done;
        }
    }
}