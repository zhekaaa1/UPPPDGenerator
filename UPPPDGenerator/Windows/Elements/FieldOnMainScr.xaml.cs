using System.Windows;
using System.Windows.Controls;

namespace UPPPDGenerator.Windows.Elements
{
    /// <summary>
    /// Логика взаимодействия для FieldOnMainScr.xaml
    /// </summary>
    public partial class FieldOnMainScr : UserControl
    {
        public static readonly DependencyProperty FieldPlaceholderProperty =
        DependencyProperty.Register("FieldPlaceholder", typeof(string), typeof(FieldOnMainScr), new PropertyMetadata(""));

        
        public string FieldPlaceholder
        {
            get => (string)GetValue(FieldPlaceholderProperty);
            set => SetValue(FieldPlaceholderProperty, value);
        }
        public static readonly DependencyProperty TextHorizontalAlignmentProperty =
    DependencyProperty.Register("TextHorizontalAlignment", typeof(HorizontalAlignment), typeof(FieldOnMainScr),
        new PropertyMetadata(HorizontalAlignment.Left, OnTextHorizontalAlignmentChanged));

        public HorizontalAlignment TextHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(TextHorizontalAlignmentProperty);
            set => SetValue(TextHorizontalAlignmentProperty, value);
        }

        private static void OnTextHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FieldOnMainScr;
            if (control != null)
            {
                control.MainTextBox.HorizontalContentAlignment = (HorizontalAlignment)e.NewValue;
            }
        }
        public string Text
        {
            get => MainTextBox.Text;
            set => MainTextBox.Text = value;
        }
        public FieldOnMainScr()
        {
            InitializeComponent();
            UpdatePlaceholderVisibility();
        }
        private void Field_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }
        private void Field_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }
        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }
        private void UpdatePlaceholderVisibility()
        {
            PlaceholderTextBlock.Visibility = string.IsNullOrEmpty(MainTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
