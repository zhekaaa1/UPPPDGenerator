using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // Проверяем текущее состояние видимости
            if (hidden.Visibility == Visibility.Visible)
            {
                // Скрываем текстовый пароль, показываем зашифрованный
                hidden.Visibility = Visibility.Collapsed;
                shown.Visibility = Visibility.Visible;

                // Копируем значение из PasswordBox в TextBox
                passwordshown.Text = passwordhidden.Password;

                // Меняем иконку на скрытую
                ((Image)sender).Source = new BitmapImage(new Uri("/resources/imgs/eye-slash-fill.png", UriKind.Relative));
            }
            else
            {
                // Скрываем текстовый пароль, показываем зашифрованный
                shown.Visibility = Visibility.Collapsed;
                hidden.Visibility = Visibility.Visible;

                // Копируем значение из TextBox в PasswordBox
                passwordhidden.Password = passwordshown.Text;

                // Меняем иконку на раскрытую
                ((Image)sender).Source = new BitmapImage(new Uri("/resources/imgs/eye-fill.png", UriKind.Relative));
            }
        }


        public string GetPassword(Window window)
        {
            if (window is MainWindow window2)
            {
                string passfromwin = window2.hidden.Visibility == Visibility.Visible
                    ? window2.passwordhidden.Password
                    : window2.passwordshown.Text;

                return ComputeSHA256Hash(passfromwin);
            }
            return string.Empty;
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
