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

namespace AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for TagControl.xaml
    /// </summary>
    public partial class TagControl : UserControl
    {
        public TagControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return Tag_TextBox.Text; }
            set { Tag_TextBox.Text = value; }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tag_TextBox.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Tag_TextBox.IsEnabled = false;
        }
    }
}
