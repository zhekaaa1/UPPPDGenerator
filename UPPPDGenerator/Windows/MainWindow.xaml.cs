using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UPPPDGenerator.Managers;
using UPPPDGenerator.Windows;
using UPPPDGenerator.Windows.Elements;

namespace UPPPDGenerator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int AuthState = 0;
        public User UserToConfirm = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string langCode = button.Tag as string;
            LangManager.SetLanguage(langCode);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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
                    AuthState = 1;
                    _ = AnimationManager.FadeOut(this, maingrid);
                    _ = AnimationManager.FadeIn(this, authgrid);
                    _ = AnimationManager.FadeIn(this, OnAuthorizeGrid);
                    break;
                case "toreg":
                    AuthState = 2;
                    _ = AnimationManager.FadeOut(this, maingrid);
                    _ = AnimationManager.FadeIn(this, reggrid);
                    _ = AnimationManager.FadeIn(this, OnRegisterGrid);
                    break;
                case "haveacc":
                    AuthState = 1;
                    _ = AnimationManager.FadeOut(this, reggrid);
                    _ = AnimationManager.FadeOut(this, OnRegisterGrid);
                    _ = AnimationManager.FadeIn(this, authgrid);
                    _ = AnimationManager.FadeIn(this, OnAuthorizeGrid);
                    break;
                case "nhaveacc":
                    AuthState = 2;
                    _ = AnimationManager.FadeOut(this, authgrid);
                    _ = AnimationManager.FadeOut(this, OnAuthorizeGrid);
                    _ = AnimationManager.FadeIn(this, reggrid);
                    _ = AnimationManager.FadeIn(this, OnRegisterGrid);
                    break;
                case "toconf":
                    AuthState = 3;
                    _ = AnimationManager.FadeOut(this, OnRegisterGrid);
                    _ = AnimationManager.FadeOut(this, reggrid);
                    _ = AnimationManager.FadeIn(this, confgrid);
                    break;
                case "back1":
                    AuthState = 0;
                    _ = AnimationManager.FadeOut(this, authgrid);
                    _ = AnimationManager.FadeIn(this, maingrid);
                    break;
                case "back2":
                    AuthState = 0;
                    _ = AnimationManager.FadeOut(this, reggrid);
                    _ = AnimationManager.FadeIn(this, maingrid);
                    break;
                case "back3":
                    AuthState = 0;
                    _ = AnimationManager.FadeOut(this, confgrid);
                    _ = AnimationManager.FadeIn(this, reggrid);
                    break;
                case "asguest":
                    ContinueAsGuest();
                    break;
                case "backtoreg":
                    _ = AnimationManager.FadeOut(this, confgrid);
                    _ = AnimationManager.FadeIn(this, OnRegisterGrid);
                    _ = AnimationManager.FadeIn(this, reggrid);
                    break;
            }
        }
        private int ClickCount = 0;
        private void ContinueAsGuest()
        {
            if (ClickCount == 0)
            {
                ErrorContainer.Show("Вы уверены? Некоторые возможности до авторизации будут недоступны. Нажмите ещё раз, чтобы продолжить как гость.", ErrorType.Info);
                ClickCount++;
            }
            else
            {
                ClickCount = 0;
                new MainWin().Show();
                Close();
            }
        }
        /// <summary>
        /// Обработчик события кнопки ВОЙТИ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (PasswordField.GetPassword() == "" || string.IsNullOrEmpty(PasswordField.GetPassword()) || email.Text == "" || string.IsNullOrEmpty(email.Text))
            {
                ErrorContainer.Show("Заполните все поля!", ErrorType.Critical);
                return;
            }
            string mail = email.Text;
            string password = PasswordField.GetPassword();
            UserManager userManager = new UserManager();
            var found = await userManager.Authorize(mail, password);

            if (found.Id <= 0)
            {
                string errorMessage = found.FullName;
                ErrorType errorType = found.Id == 0 ? ErrorType.Critical : ErrorType.Warning;
                ErrorContainer.Show(errorMessage, errorType);
                return;
            }
            MainWin mainWin = new MainWin();
            mainWin.Show();
            Close();
        }
        /// <summary>
        /// Обработчик события кнопки ЗАРЕГИСТРИРОВАТЬСЯ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string name = rf.Text.Trim();
            string surname = ri.Text.Trim();
            string patronymic = ro.Text.Trim();
            string email = remail.Text.Trim();
            string password = rpass.GetRealPassword();
            string confirmPassword = rretrypass.GetRealPassword();

            if (ValidateRegistrationFields(name,surname,patronymic,email,password,confirmPassword))
            {
                string fullname = string.Join(" ", surname, name, patronymic);
                UserManager userManager = new UserManager();
                var user = await userManager.Register(fullname, email, rpass.GetPassword());
                if (user.Id <= 0)
                {
                    ErrorContainer.Show(user.FullName, user.Id == 0? ErrorType.Critical : ErrorType.Warning);
                    return;
                }
                AuthState = 3;
                UserToConfirm = user;
                GoTo("toconf");
            }
        }
        /// <summary>
        /// Обработчик события нажатия кнопки ПОДТВЕРДИТЬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (VerificationCodeField.Text.Length != 6)
            {
                ErrorContainer.Show("Код подтверждения должен быть ни длиннее, ни короче 6 символов. Повторите попытку.", ErrorType.Critical);
                return;
            }
            UserManager userManager = new UserManager();
            string code = VerificationCodeField.Text.Trim().ToLower();
            VerifyResult result = await userManager.VerifyUser(UserToConfirm, code);
            if (result.Success == false)
            {
                ErrorContainer.Show(result.Message, result.User.Id == 0 ? ErrorType.Critical : ErrorType.Info);
                return;
            }
            else
            {
                AuthState = 0;
                new MainWin().Show();
                Close();
            }
        }
        private bool ValidateRegistrationFields(params string[] strings)
        {
            if (string.IsNullOrWhiteSpace(strings[0]) ||
                string.IsNullOrWhiteSpace(strings[1]) ||
                string.IsNullOrWhiteSpace(strings[2]) ||
                string.IsNullOrWhiteSpace(strings[3]) ||
                string.IsNullOrWhiteSpace(strings[4]) ||
                string.IsNullOrWhiteSpace(strings[5]))
            {
                ErrorContainer.Show("Заполните все поля.", ErrorType.Critical);
                return false;
            }
            bool IsValidEmail(string email)
            {
                try
                {
                    var addr = new MailAddress(email);
                    return addr.Address == email;
                }
                catch
                {
                    return false;
                }
            }
            if (!IsValidEmail(strings[3]))
            {
                ErrorContainer.Show("Введите корректный email.", ErrorType.Critical);
                return false;
            }
            bool PasswordNotContainsSpace(string password) => !password.Contains(" ");
            
            if (!PasswordNotContainsSpace(strings[4]))
            {
                ErrorContainer.Show("Исключите пробелы из пароля.",ErrorType.Critical);
                return false;
            }
            string ValidatePassword(string password)
            {
                if (string.IsNullOrWhiteSpace(password))
                    return "Пароль не должен быть пустым.";

                if (password.Length < 8)
                    return "Пароль должен содержать минимум 8 символов.";

                if (!Regex.IsMatch(password, "[A-Z]"))
                    return "Пароль должен содержать хотя бы одну заглавную букву.";

                if (!Regex.IsMatch(password, "[a-z]"))
                    return "Пароль должен содержать хотя бы одну строчную букву.";

                if (!Regex.IsMatch(password, "[0-9]"))
                    return "Пароль должен содержать хотя бы одну цифру.";

                if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                    return "Пароль должен содержать хотя бы один спецсимвол.";

                return null;
            }

            string passwordValidationError = ValidatePassword(strings[4]);
            if (passwordValidationError != null)
            {
                ErrorContainer.Show(passwordValidationError, ErrorType.Critical);
                return false;
            }
            if (strings[4] != strings[5])
            {
                ErrorContainer.Show("Пароли не совпадают.", ErrorType.Critical);
                return false;
            }
            return true;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AuthState == 1)
            {
                _ = AnimationManager.FadeOut(this, authgrid);
                _ = AnimationManager.FadeOut(this, OnAuthorizeGrid);
            }
            else if (AuthState == 2)
            {
                _ = AnimationManager.FadeOut(this, reggrid);
                _ = AnimationManager.FadeOut(this, OnRegisterGrid);
            }
            else if (AuthState == 3)
            {
                _ = AnimationManager.FadeOut(this, confgrid);
            }
            else return;
            AuthState = 0;
            _ = AnimationManager.FadeIn(this, maingrid);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            UserToConfirm = null;
            GoTo("backtoreg");
        }
    }
}
