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

namespace ICA.AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for InstallationControl.xaml
    /// </summary>
    public partial class InstallationControl : UserControl
    {
        public static readonly DependencyProperty InstallationProperty =
            DependencyProperty.Register(
                "Installation",
                typeof(object),
                typeof(InstallationControl));
        public object Installation
        {
            get => (object)GetValue(InstallationProperty);
            set => SetValue(InstallationProperty, value);
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                "Location",
                typeof(object),
                typeof(InstallationControl));
        public object Location
        {
            get => GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        public bool? IsChecked
        {
            get => Installation_Checkbox.IsChecked;
            set => Installation_Checkbox.IsChecked = value;
        }

        public InstallationControl()
        {
            InitializeComponent();
        }
    }
}
