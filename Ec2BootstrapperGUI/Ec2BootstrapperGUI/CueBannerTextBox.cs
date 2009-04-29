using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ec2BootstrapperGUI
{
    public class CueBannerTextBox : TextBox
    {
        bool usePrompt;
        string _propertText;
        public static readonly DependencyProperty TextPromptProperty;
        
        static CueBannerTextBox()
        {
            TextPromptProperty = DependencyProperty.Register(
                "PromptText",
                typeof(string),
                typeof(CueBannerTextBox),
                new FrameworkPropertyMetadata("", PromptTextPropertyChanged));
        }

        public CueBannerTextBox()
            : base()
        {
        }
 
        private static void PromptTextPropertyChanged(
            DependencyObject dependency,
            DependencyPropertyChangedEventArgs e)
        {
            CueBannerTextBox cueTextBox = (CueBannerTextBox)dependency;
            cueTextBox.PromptText = dependency.GetValue(TextPromptProperty).ToString();
            cueTextBox.Loaded += CueTextBox_Loaded;
        }

        private static void CueTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            CueBannerTextBox cueTextBox = (CueBannerTextBox)sender;
            if (string.IsNullOrEmpty(cueTextBox.Text))
            {
                cueTextBox.UsePrompt = true;
                cueTextBox.ActucalText = cueTextBox.PromptText;
            }
        }
        
        public string PromptText
        {
            get { return _propertText; }
            set {_propertText = value; }
        }

        public bool UsePrompt
        {
            get { return usePrompt; }
            set
            {
                usePrompt = value;
                if (usePrompt)
                {
                    FontStyle = FontStyles.Italic;
                    Foreground = Brushes.Gray;
                }
                else
                {
                    FontStyle = FontStyles.Normal;
                    Foreground = Brushes.Black;
                }
            }
        }

        private string ActucalText
        {
            get
            {
                if (this.UsePrompt)
                {
                    return string.Empty;
                }
                return Text;
            }
            set
            {
                if (this.UsePrompt && (!string.IsNullOrEmpty(value) && value != PromptText))
                {
                    this.UsePrompt = false;
                }

                if (string.IsNullOrEmpty(value) && !IsFocused && !string.IsNullOrEmpty(PromptText))
                {
                    this.UsePrompt = true;
                    Text = PromptText;
                    return;
                }
                Text = value;
            }
        }
    }
}
