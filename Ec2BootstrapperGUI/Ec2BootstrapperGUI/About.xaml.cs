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

namespace Ec2BootstrapperGUI
{
	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About : Window
	{
		public About()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
		}

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
	}
}