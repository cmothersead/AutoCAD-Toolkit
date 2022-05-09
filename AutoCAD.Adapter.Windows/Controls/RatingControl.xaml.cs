using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register(
                nameof(Rating),
                typeof(string),
                typeof(RatingControl));
        public string Rating
        {
            get => (string)GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(RatingControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register(
                nameof(CharacterCasing),
                typeof(CharacterCasing),
                typeof(RatingControl));
        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set => SetValue(CharacterCasingProperty, value);
        }

        public RatingControl()
        {
            InitializeComponent();
        }
    }
}
