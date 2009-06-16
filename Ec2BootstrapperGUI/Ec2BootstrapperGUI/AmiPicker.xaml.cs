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
        int currentIndex = 0; 

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

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            CEc2Ami selectedItem = null;
            switch (AMITabControl.SelectedIndex)
            {
                case 0:
                    selectedItem = (CEc2Ami)QuickAmis.SelectedItem;
                    break;
                case 1:
                    selectedItem = (CEc2Ami)OwnAmis.SelectedItem;
                    break;
                case 2:
                    selectedItem = (CEc2Ami)CommunityAmis.SelectedItem;
                    break;
                default:
                    break;
            }
            if (selectedItem != null)
            {
                InstanceLauncher launcher = new InstanceLauncher(_dashboard);
                launcher.amiPicker = this;
                launcher.amiId = selectedItem.imageId;

                launcher.ShowDialog();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void amis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (AMITabControl.SelectedIndex)
            {
                case 0:
                    NextButton.IsEnabled = (QuickAmis.SelectedItem != null);
                    break;
                case 1:
                    NextButton.IsEnabled = (OwnAmis.SelectedItem != null);
                    break;
                case 2:
                    NextButton.IsEnabled = (CommunityAmis.SelectedItem != null);
                    break;
                default:
                    break;
            }
        }

        private void setQuickAmis()
        {
            fetchQuickAmis();
            foreach (CEc2Ami item in _quickAmis)
            {
                bool exist = false;
                foreach (CEc2Ami it in _amis)
                {
                    if (string.Compare(it.imageId, item.imageId) == 0)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist == false)
                    _amis.Add(item);
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

        private void enableProgressBar()
        {
            StatusDesc.Text = ConstantString.ContactAmazon;
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
                StatusDesc.Text = ConstantString.NoAmi;
            else
                StatusDesc.Text = ConstantString.Done;
        }
        private void TitleBarGloss_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AMITabControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AMITabControl.SelectedIndex == currentIndex)
                return;
            currentIndex = AMITabControl.SelectedIndex;
            switch (AMITabControl.SelectedIndex)
            {
                case 0:
                    {
                        NextButton.IsEnabled = false;
                        _amis.Clear();
                        setQuickAmis();
                        break;
                    }
                case 1:
                    {
                        NextButton.IsEnabled = false;
                        _amis.Clear();

                        Thread oThread = new Thread(new ThreadStart(setOwnAmis));
                        oThread.Start();
                        enableProgressBar();

                        break;
                    }
                case 2:
                    {
                        NextButton.IsEnabled = false;
                        _amis.Clear();
                        Thread oThread = new Thread(new ThreadStart(setCommunityAmis));
                        oThread.Start();
                        enableProgressBar();

                        break;
                    }
                default:
                    break;
            }
        }
    }
}