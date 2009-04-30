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
using System.Windows.Media.Animation;
using System.Threading;

namespace Ec2BootstrapperGUI
{
	/// <summary>
	/// Interaction logic for PasswordPrompt.xaml
	/// </summary>
	public partial class PasswordPrompt : Window
	{
        CEc2Instance _instance;
        string _password;

		public PasswordPrompt()
		{
			this.InitializeComponent();
            ProgBar.Visibility = Visibility.Hidden; 
        }

        public CEc2Instance instance
        {
            set { _instance = value; }
        }

        private delegate void StopProgressbarCallback();
        private delegate void SetStatusDone();

        private void installRemotely()
        {
            try
            {
                _instance.uploadAndInstallMsi(_password);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Dispatcher.Invoke(new StopProgressbarCallback(disableProgressBar));
            Dispatcher.Invoke(new SetStatusDone(setStatusDone));
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OkButton.IsEnabled = false;
                AdminPassword.IsEnabled = false;
                if (_instance == null)
                {
                    throw new Exception("no valid instance");
                }

                StatusBk.Text = ConstantString.ContactAmazon;

                //access from another thread
                _password = AdminPassword.Password;
                enableProgressBar();

                Thread oThread = new Thread(new ThreadStart(installRemotely));
                oThread.SetApartmentState(ApartmentState.STA);
                oThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                OkButton.IsEnabled = true;
                AdminPassword.IsEnabled = true;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void enableProgressBar()
        {
            ProgBar.Visibility = Visibility.Visible;
            ProgBar.IsIndeterminate = true;
            Duration duration = new Duration(TimeSpan.FromSeconds(10));
            DoubleAnimation doubleanimation = new DoubleAnimation(200.0, duration);
            ProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, doubleanimation);
        }

        private void disableProgressBar()
        {
            ProgBar.IsIndeterminate = false;
            ProgBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, null);
            ProgBar.Visibility = Visibility.Hidden;
        }

        private void setStatusDone()
        {
            StatusBk.Text = ConstantString.Done;
            OkButton.IsEnabled = true;
            AdminPassword.IsEnabled = true;
        }
	}
}