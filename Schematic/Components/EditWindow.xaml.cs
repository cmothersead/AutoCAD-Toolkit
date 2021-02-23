using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using ICA.Schematic.Data;

namespace ICA.Schematic.Components
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public Component Component;

        public EditWindow()
        {
            InitializeComponent();
        }

        private void EditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateForm();
        }

        private void PopulateForm()
        {
            DescriptionControl.IsChecked = Component.Desc[0].Invisible;
            PartControl.IsChecked = Component.Mfg.Invisible & Component.Cat.Invisible;
            InstallationControl.IsChecked = Component.Inst.Invisible;
            TagControl.Text = Component.Tag.TextString;
            List<string> Description = new List<string>();
            foreach (var description in Component.Desc)
            {
                Description.Add(description.TextString);
            }
            DescriptionControl.Text = Description;
            if (Component.Inst == null)
            {
                InstallationControl.Installation_TextBox.IsEnabled = false;
            }
            else
            {
                InstallationControl.Installation_TextBox.Text = Component.Inst.TextString;
            }
            if (Component.Loc == null)
            {
                InstallationControl.Location_TextBox.IsEnabled = false;
            }
            else
            {
                InstallationControl.Location_TextBox.Text = Component.Loc.TextString;
            }
            PartControl.Initialize(Component.Family.TextString, Component.Mfg.TextString, Component.Cat.TextString);
        }

        private void Commit()
        {
            if (DescriptionControl.IsChecked == true)
            {
                Component.Desc[0].Hide();
                Component.Desc[1].Hide();
                Component.Desc[2].Hide();
            }
            else
            {
                Component.Desc[0].Unhide();
                Component.Desc[1].Unhide();
                Component.Desc[2].Unhide();
            }
            if (PartControl.IsChecked == true)
            {
                Component.Mfg.Hide();
                Component.Cat.Hide();
            }
            else
            {
                Component.Mfg.Unhide();
                Component.Cat.Unhide();
            }
            if (InstallationControl.IsChecked == true)
            {
                Component.Inst.Hide();
                Component.Loc.Hide();
            }
            else
            {
                Component.Inst.Unhide();
                Component.Loc.Unhide();
            }
            Component.Desc[0].SetValue(DescriptionControl.Text[0]);
            Component.Desc[1].SetValue(DescriptionControl.Text[1]);
            Component.Desc[2].SetValue(DescriptionControl.Text[2]);
            Component.Mfg.SetValue(PartControl.Manufacturer.ManufacturerName);
            Component.Cat.SetValue(PartControl.Part.PartNumber);
            Component.Inst.SetValue(InstallationControl.Installation_TextBox.Text);
            Component.Loc.SetValue(InstallationControl.Location_TextBox.Text);
            Component.CollapseAttributeStack();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            Commit();
            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
