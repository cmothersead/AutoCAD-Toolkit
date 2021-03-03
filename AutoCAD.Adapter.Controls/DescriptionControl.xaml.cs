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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DescriptionControl : UserControl
    {
        public List<string> Text
        {
            get
            {
                List<string> text = new List<string>();
                foreach (TextBox textBox in Description_StackPanel.Children)
                {
                    text.Add(textBox.Text);
                }
                return text;
            }

            set
            {
                Description_StackPanel.Children.RemoveRange(0, Description_StackPanel.Children.Count);
                if (value.Count == 0)
                {
                    AddTextbox();
                }
                else
                {
                    foreach (string text in value)
                    {
                        AddTextbox(text);
                    }
                    AddTextbox();
                }
            }
        }

        public bool? IsChecked
        {
            get { return Description_Checkbox.IsChecked; }
            set { Description_Checkbox.IsChecked = value; }
        }

        public DescriptionControl()
        {
            InitializeComponent();
            AddTextbox();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox thisTextBox = e.Source as TextBox;
            int index = Description_StackPanel.Children.IndexOf(thisTextBox);

            if (!String.IsNullOrWhiteSpace(thisTextBox.Text))
            {
                if (Description_StackPanel.Children.Count == index + 1)
                {
                    AddTextbox();
                }
            }
            else
            {
                Description_StackPanel.Children.Remove(Description_StackPanel.Children[index]);
                Description_StackPanel.Children[index].Focus();
            }
        }

        private void AddTextbox(string text = "")
        {
            Description_StackPanel.Children.Add(new TextBox
            {
                Text = text,
                Height = 28,
                Margin = new Thickness(2),
                VerticalContentAlignment = VerticalAlignment.Center,
                CharacterCasing = CharacterCasing.Upper
            });
        }
    }
}
