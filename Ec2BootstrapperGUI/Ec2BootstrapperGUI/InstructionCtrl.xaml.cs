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

namespace Ec2BootstrapperGUI
{
	/// <summary>
	/// Interaction logic for RegisterOrConfig.xaml
	/// </summary>
	public partial class RegisterOrConfig
	{
		public RegisterOrConfig()
		{
			this.InitializeComponent();
		}

        private void registerHyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://aws.amazon.com");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
	}
}