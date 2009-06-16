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
using System.Windows.Forms;
using Ec2Bootstrapperlib;

namespace Ec2BootstrapperGUI
{
	/// <summary>
	/// Interaction logic for KeyFileInputDlg.xaml
	/// </summary>
	public partial class KeyFileInputDlg : Window
	{
        string _keyName;
        public KeyFileInputDlg(string keyName)
		{
			this.InitializeComponent();

            _keyName = keyName;
            keyPathInstruction.Text = keyPathInstruction.Text + keyName + ":";
		}

        private void keyPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PEM files (*.pem)|*.pem";
            ofd.InitialDirectory = CAwsConfig.getEc2BootstrapperDirectory();
            if (System.Windows.Forms.DialogResult.OK == ofd.ShowDialog())
            {
                keyPath.Text = ofd.FileName;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_keyName) == false &&
                string.IsNullOrEmpty(keyPath.Text) == false)
            {
                CAwsConfig.Instance.setKeyFilePath(_keyName, keyPath.Text);
                CAwsConfig.Instance.commit();
            }
            this.Close();
        }
        private void TitleBarGloss_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
	}
}