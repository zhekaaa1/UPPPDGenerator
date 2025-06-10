using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
namespace UPPPDGenerator.Managers
{
    public class PasswordManager
    {
        public void TogglePasswordVisibility(StackPanel hidden, StackPanel shown, TextBox passwordshown, PasswordBox passwordhidden, object sender, RoutedEventArgs e)
        {
            if (hidden.Visibility == Visibility.Visible)
            {
                hidden.Visibility = Visibility.Collapsed;
                shown.Visibility = Visibility.Visible;
                passwordshown.Text = passwordhidden.Password;
                // Меняем иконку на скрытую
                ((Image)sender).Source = new BitmapImage(new Uri("/resources/imgs/eye-slash-fill.png", UriKind.Relative));
            }
            else
            {
                shown.Visibility = Visibility.Collapsed;
                hidden.Visibility = Visibility.Visible;
                passwordhidden.Password = passwordshown.Text;
                ((Image)sender).Source = new BitmapImage(new Uri("/resources/imgs/eye-fill.png", UriKind.Relative));
            }
        }
        public string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                Console.WriteLine($"ВЫЧИСЛЕН ХЕШ: ----   {BitConverter.ToString(hashBytes).Replace("-", "").ToLower()}");
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
