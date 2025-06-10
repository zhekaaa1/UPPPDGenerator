using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using UPPPDGenerator.Managers;

namespace UPPPDGenerator.Windows.Elements
{
    /// <summary>
    /// Логика взаимодействия для PasswordBoxOnMainScr.xaml
    /// </summary>
    public partial class PasswordBoxOnMainScr : UserControl
    {
        public static readonly DependencyProperty PasswordPlaceholderProperty =
        DependencyProperty.Register("PasswordPlaceholder", typeof(string), typeof(PasswordBoxOnMainScr), new PropertyMetadata(""));
        public PasswordBoxOnMainScr()
        {
            InitializeComponent();
            VisiblePasswordBox.TextChanged += (s, e) => UpdatePlaceholderVisibility();
            HiddenPasswordBox.PasswordChanged += (s, e) => UpdatePlaceholderVisibility();
            VisiblePasswordBox.LostFocus += (s, e) => UpdatePlaceholderVisibility();
            HiddenPasswordBox.LostFocus += (s, e) => UpdatePlaceholderVisibility();
        }
        public string PasswordPlaceholder
        {
            get => (string)GetValue(PasswordPlaceholderProperty);
            set => SetValue(PasswordPlaceholderProperty, value);
        }
        private void UpdatePlaceholderVisibility()
        {
            bool isEmpty = IsPasswordEmpty();
            PlaceholderText.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        private bool IsPasswordEmpty()
        {
            if (VisiblePasswordBox.Visibility == Visibility.Visible)
                return string.IsNullOrEmpty(VisiblePasswordBox.Text);
            else
                return string.IsNullOrEmpty(HiddenPasswordBox.Password);
        }
        private void ToggleVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (HiddenPasswordBox.Visibility == Visibility.Visible)
            {
                // Переключаем на отображение текста
                VisiblePasswordBox.Text = HiddenPasswordBox.Password;
                HiddenPasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                VisibilityImage.Source = new BitmapImage(new Uri("/resources/imgs/eye-slash-fill.png", UriKind.Relative));
                VisiblePasswordBox.Focus();
                VisiblePasswordBox.CaretIndex = VisiblePasswordBox.Text.Length;
            }
            else
            {
                // Переключаем на скрытый ввод
                HiddenPasswordBox.Password = VisiblePasswordBox.Text;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                HiddenPasswordBox.Visibility = Visibility.Visible;
                VisibilityImage.Source = new BitmapImage(new Uri("/resources/imgs/eye-fill.png", UriKind.Relative));
                HiddenPasswordBox.Focus();
            }
        }
        public string GetPassword()
        {
            string password = GetRealPassword();
            return string.IsNullOrEmpty(password)
                ? ""
                : new PasswordManager().ComputeSHA256Hash(password);
        }

        public string GetRealPassword()
        {
            return VisiblePasswordBox.Visibility == Visibility.Visible
                ? VisiblePasswordBox.Text
                : HiddenPasswordBox.Password;
        }
    }
}
