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
	/// Interaction logic for Circle.xaml
	/// </summary>
	public partial class Circle : UserControl
	{
        public static readonly DependencyProperty FillInProperty;

        static Circle()
        {
            FillInProperty = DependencyProperty.Register(
                "FillIn",
                typeof(Brush),
                typeof(Circle),new UIPropertyMetadata(new PropertyChangedCallback(FillInPropertyChanged)));
        }

        private static void FillInPropertyChanged(
            DependencyObject dependency,
            DependencyPropertyChangedEventArgs e)
        {
            Circle circle = (Circle)dependency;
            circle.insideCircle.Fill = circle.FillIn;
        }

		public Circle()
		{
			this.InitializeComponent();
		}

        public Brush FillIn
        {
            set { this.SetValue(FillInProperty, value); }
            get { return (Brush)this.GetValue(FillInProperty);}
        }
	}
}