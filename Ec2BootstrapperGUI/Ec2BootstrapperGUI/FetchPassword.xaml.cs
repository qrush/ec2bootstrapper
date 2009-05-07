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
using System.Windows.Media.Animation;
using System.Threading;
using Ec2Bootstrapperlib;

namespace Ec2BootstrapperGUI
{
	/// <summary>
	/// Interaction logic for FetchPassword.xaml
	/// </summary>
	public partial class FetchPassword : Window
	{
        Thread oThread = null;
		public FetchPassword(CEc2Instance ins)
		{
			this.InitializeComponent();
            StatusBk.Text = ConstantString.ContactAmazon;

            oThread = new Thread(getPassword);
            oThread.Start(ins);

            okButton.IsEnabled = false;
            enableProgressBar();
		}

        private delegate void SetPassword(string pw);
        private delegate void StopProgressbarCallback();
        private delegate void SetStatus(string status);

        private void setStatus(string status)
        {
            StatusBk.Text = status;
        }

        private void getPassword(object ins)
        {
            string pw = null;
            try
            {
                pw = ((CEc2Instance)ins).getAdministratorPassord();
                if (string.IsNullOrEmpty(pw) == true)
                    pw = "(not available)";
            }
            catch (ThreadAbortException)
            {
                Dispatcher.Invoke(new SetPassword(setStatus), ConstantString.ThreadAborted);
            }
            catch (Exception ex)
            {
                pw = "(caught exception)";
                MessageBox.Show(ex.Message);
            }

            Dispatcher.Invoke(new SetPassword(setPassword), pw);
            Dispatcher.Invoke(new StopProgressbarCallback(disableProgressBar));
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (oThread != null)
                {
                    oThread.Abort();
                    oThread.Join();
                }
            }
            catch (Exception)
            {
            }
            this.Close();
        }

        private void setPassword(string pw)
        {
            passwordTxt.Text = pw;
        }

        private void enableProgressBar()
        {
            StatusBk.Text = ConstantString.ContactAmazon;
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
            StatusBk.Text = ConstantString.Done;
            okButton.IsEnabled = true;
            oThread = null;
        }
  	}
}