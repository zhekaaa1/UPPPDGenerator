using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UPPPDGenerator.Managers;
using UPPPDGenerator.Windows;

namespace UPPPDGenerator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string langCode = button.Tag as string;
            LangManager.SetLanguage(langCode);
            UpdateErrsLang();
        }
        private void UpdateErrsLang()
        {
            if (err1.Opacity != 0)
            {
                if (err1.Text.Contains("Неправильный") || err1.Text.Contains("Wrong"))
                    err1.Text = (string)Application.Current.Resources["errs/auth/wrongauthdata"];

                if (err1.Text.Contains("Обязательное") || err1.Text.Contains("field"))
                    err1.Text = (string)Application.Current.Resources["errs/reqfield"];
            }
            if (err2.Opacity != 0)
            {
                err2.Text = (string)Application.Current.Resources["errs/reqfield"];
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string to = btn.Tag as string;

            GoTo(to);
        }
        private void GoTo(string to)
        {
            switch (to)
            {
                case "tologin":
                    _ = AnimationManager.FadeOutGrid(this, maingrid);
                    _ = AnimationManager.FadeInGrid(this, authgrid);
                    break;
                case "toreg":
                    _ = AnimationManager.FadeOutGrid(this, maingrid);
                    _ = AnimationManager.FadeInGrid(this, reggrid);
                    break;
                case "haveacc":
                    _ = AnimationManager.FadeOutGrid(this, reggrid);
                    _ = AnimationManager.FadeInGrid(this, authgrid);
                    break;
                case "nhaveacc":
                    _ = AnimationManager.FadeOutGrid(this, authgrid);
                    _ = AnimationManager.FadeInGrid(this, reggrid);
                    break;
                case "toconf":
                    _ = AnimationManager.FadeOutGrid(this, reggrid);
                    _ = AnimationManager.FadeInGrid(this, confgrid);
                    break;
                case "back1":
                    _ = AnimationManager.FadeOutGrid(this, authgrid);
                    _ = AnimationManager.FadeInGrid(this, maingrid);
                    break;
                case "back2":
                    _ = AnimationManager.FadeOutGrid(this, reggrid);
                    _ = AnimationManager.FadeInGrid(this, maingrid);
                    break;
                case "back3":
                    _ = AnimationManager.FadeOutGrid(this, confgrid);
                    _ = AnimationManager.FadeInGrid(this, reggrid);
                    break;
            }
        }
        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            PasswordManager passwordManager = new PasswordManager();
            passwordManager.TogglePasswordVisibility(hidden, shown, passwordshown, passwordhidden, sender, e);
        }


        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            if (err1.Opacity != 0)
            {
                err1.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            if (err2.Opacity != 0)
            {
                err2.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            


            PasswordManager passwordManager = new PasswordManager();
            string mail = email.Text;
            string pass = passwordManager.GetPassword(this);

            bool passshown = hidden.Visibility == Visibility.Collapsed;


            UserManager userManager = new UserManager();
            User found = await userManager.Authorize(mail, pass);

            //User found = allusers.FirstOrDefault(user => user.Email == mail && user.PasswordHash == pass);


            if (found != null)
            {
                Properties.Settings.Default.userid = found.Id;
                await userManager.SetLogonUser(found.Id);

                AnimateBorder(email, Colors.Green); // Зеленая подсветка
                if (passshown) AnimateBorder(passwordshown, Colors.Green); // Зеленая подсветка
                else AnimateBorder(passwordhidden, Colors.Green);

                await Task.Delay(1000); // Ждем 1 секунду

                
                MainWin mainWin = new MainWin();
                mainWin.Show();

                Window.GetWindow(this)?.Close();
            }
            else
            {
                AnimateBorder(email, Colors.Red); // Красная подсветка
                if (passshown) AnimateBorder(passwordshown, Colors.Red); // Зеленая подсветка
                else AnimateBorder(passwordhidden, Colors.Red);
                await Task.Delay(1000); // Ждем 1 секунду
                ShowError(1);
            }
        }
        // Функция анимации рамки TextBox
        private void AnimateBorder(Control control, Color color)
        {
            if (control is TextBox textBox)
            {
                var animation = new ColorAnimation
                {
                    From = ((SolidColorBrush)textBox.BorderBrush).Color,
                    To = color,
                    Duration = TimeSpan.FromSeconds(0.5),
                    AutoReverse = true
                };

                textBox.BorderBrush = new SolidColorBrush(((SolidColorBrush)textBox.BorderBrush).Color);
                textBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            else if (control is PasswordBox passwordBox)
            {
                var animation = new ColorAnimation
                {
                    From = ((SolidColorBrush)passwordBox.BorderBrush).Color,
                    To = color,
                    Duration = TimeSpan.FromSeconds(0.5),
                    AutoReverse = true
                };

                passwordBox.BorderBrush = new SolidColorBrush(((SolidColorBrush)passwordBox.BorderBrush).Color);
                passwordBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
        }

        // Функция плавного появления ошибки
        private void ShowError(int codeErr)
        {
            FieldValidatorManager fieldValidatorManager = new FieldValidatorManager();

            bool passshown = hidden.Visibility == Visibility.Collapsed;
            Control passwordfield = passshown ? (Control)passwordshown : (Control)passwordhidden;

            List<Control> controls = new List<Control> { email, passwordfield };
            List<(Control, int)> checkedfields = fieldValidatorManager.CheckFields(controls);

            bool isAllValid = !checkedfields.Any(x => x.Item2 != 1);

            if (!isAllValid) codeErr = 2; // Если есть пустые поля

            AnimateError(codeErr, checkedfields);
        }
        private void AnimateError(int errCode, List<(Control, int)> checkedFields)
        {
            string localizedMessage = "";
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            

            switch (errCode)
            {
                case 1:
                    localizedMessage = (string)Application.Current.Resources["errs/auth/wrongauthdata"];
                    err1.Text = localizedMessage;
                    err1.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                    break;
                case 2:
                    localizedMessage = (string)Application.Current.Resources["errs/reqfield"];
                    //err1.Text = localizedMessage;
                    //err1.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                    // Подсветка полей с ошибками
                    foreach (var (control, state) in checkedFields)
                    {
                        if (state != 1) // Если поле пустое
                        {
                            if (control is TextBox textBox)
                            {
                                if (textBox.Name == "email")
                                {
                                    err1.Text = localizedMessage;
                                    err1.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                                    AnimateBorder(control, Colors.Red);
                                }
                                else
                                {
                                    err2.Text = localizedMessage;
                                    err2.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                                    AnimateBorder(control, Colors.Red);
                                }
                            }
                            if (control is PasswordBox passwordBox)
                            {
                                err2.Text = localizedMessage;
                                err2.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                                AnimateBorder(control, Colors.Red);
                            }
                        }
                    }
                    break;
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;

            string to = textBlock.Tag as string;

            GoTo(to);
        }
    }
}
