using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using UPPPDGenerator.Managers;
using UPPPDGenerator.Windows.Elements;

namespace UPPPDGenerator.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWin.xaml
    /// </summary>
    public partial class MainWin : Window
    {
        public string SelectedFilePath { get; set; } = string.Empty;
        public MainWin()
        {
            InitializeComponent();
            ButtonOnMainScr.ButtonClicked += OnButtonClicked;
            int langIndex = LangManager.GetLangNum();
            langComboBox.SelectedIndex = langIndex > 1 ? 0 : 1;

            if (langComboBox.SelectedIndex == 1)
            {
                warningLabel.Visibility = Visibility.Visible;
                FadeIn(warningLabel, UIElement.OpacityProperty, 0.3);
            }
            else
            {
                warningLabel.Visibility = Visibility.Collapsed;
                warningLabel.Opacity = 0;
            }

            logonUserName.Text = LogonUser.FullName;
            TemplateLoader loader = new TemplateLoader(chooseTemplates, LogonUser.Id);
            loader.LoadTemplates();
        }

        private void defence_Checked(object sender, RoutedEventArgs e)
        {
            FadeIn(passfortemplate, UIElement.OpacityProperty, 0.5);
        }

        private void defence_Unchecked(object sender, RoutedEventArgs e)
        {
            FadeOut(passfortemplate, UIElement.OpacityProperty, 0.5);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            n1.CollapseButton();
            FadeOut(createb, UIElement.OpacityProperty, 0.5);
            ButtonOnMainScr.ResetActiveButton(); // сбрасываем активную кнопку
        }
        private void FadeIn(UIElement uIElement, DependencyProperty dependencyProperty, double seconds)
        {
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(seconds)
            };
            fadeIn.Completed += (s, e) => uIElement.Opacity = 1;
            uIElement.BeginAnimation(dependencyProperty, fadeIn);
        }
        private void FadeOut(UIElement uIElement, DependencyProperty dependencyProperty, double seconds)
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(seconds)
            };
            fadeOut.Completed += (s, e) => uIElement.Opacity = 0;
            uIElement.BeginAnimation(dependencyProperty, fadeOut);
        }

        private void OnButtonClicked(ButtonOnMainScr clickedButton)
        {
            FadeOut(createb, UIElement.OpacityProperty, 0.5);
            FadeOut(fillb, UIElement.OpacityProperty, 0.5);
            FadeOut(settingsb, UIElement.OpacityProperty, 0.5);
            FadeOut(mytemplatesb, UIElement.OpacityProperty, 0.5);
            FadeOut(logoutb, UIElement.OpacityProperty, 0.5);
            // Скрываем все контейнеры
            createb.Visibility = Visibility.Collapsed;
            fillb.Visibility = Visibility.Collapsed;
            settingsb.Visibility = Visibility.Collapsed;
            mytemplatesb.Visibility = Visibility.Collapsed;
            logoutb.Visibility = Visibility.Collapsed;

            // Показываем соответствующий контейнер
            switch (clickedButton.Tag)
            {
                case "b1":
                    Console.WriteLine("Нажат b1");
                    createb.Visibility = Visibility.Visible;
                    createb.Opacity = 0;
                    FadeIn(createb, UIElement.OpacityProperty, 0.5);
                    break;
                case "b2":
                    Console.WriteLine("Нажат b2");
                    fillb.Visibility = Visibility.Visible;
                    fillb.Opacity = 0;
                    FadeIn(fillb, UIElement.OpacityProperty, 0.5);
                    break;
                case "b3":
                    Console.WriteLine("Нажат b3");
                    mytemplatesb.Visibility = Visibility.Visible;
                    mytemplatesb.Visibility = Visibility.Visible;
                    FadeIn(mytemplatesb, UIElement.OpacityProperty, 0.5);
                    break;
                case "b4":
                    Console.WriteLine("Нажат b4");
                    settingsb.Visibility = Visibility.Visible;
                    settingsb.Opacity = 0;
                    FadeIn(settingsb, UIElement.OpacityProperty, 0.5);
                    break;
                case "b5":
                    Console.WriteLine("Нажат b5");
                    logoutb.Visibility = Visibility.Visible;
                    logoutb.Opacity = 0;
                    FadeIn(logoutb, UIElement.OpacityProperty, 0.5);
                    break;
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (useExternalTitle.IsChecked == true && SelectedFilePath != string.Empty)
            {
                if (string.IsNullOrEmpty(templateName.Text))
                {
                    errorname.Text = "* Назовите свой документ!";
                    ShowErrorMessage(errorname);
                    return;
                }
                PreparingTemplate.Createdat = DateTime.Now;
                PreparingTemplate.Name = templateName.Text;
                PreparingTemplate.CreatedByAuthorId = Properties.Settings.Default.userid;
                PreparingTemplate.Description = description.Text;
                PreparingTemplate.PasswordHash = password.Text;


                CreateWithoutTitleTemplateWin createWithoutTitleTemplateWin = new CreateWithoutTitleTemplateWin(SelectedFilePath);
                createWithoutTitleTemplateWin.Show();
                Window.GetWindow(this)?.Close();
            }
            else
            {
                if (string.IsNullOrEmpty(templateName.Text))
                {
                    errorname.Text = "* Назовите свой документ!";
                    ShowErrorMessage(errorname);
                    return;
                }
                PreparingTemplate.Createdat = DateTime.Now;
                PreparingTemplate.Name = templateName.Text;
                PreparingTemplate.CreatedByAuthorId = Properties.Settings.Default.userid;
                PreparingTemplate.Description = description.Text;
                PreparingTemplate.PasswordHash = password.Text;


                CreateTemplateWin createTemplateWin = new CreateTemplateWin();
                createTemplateWin.Show();

                Window.GetWindow(this)?.Close();
            }
        }

        private void langComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (langComboBox.SelectedItem != null)
            {
                if (langComboBox.SelectedIndex == 0)
                {
                    LangManager.SetLanguage("ru-RU");
                    warningLabel.Visibility = Visibility.Collapsed;
                    warningLabel.Opacity = 0;
                }
                else
                {
                    LangManager.SetLanguage("en-US");
                    warningLabel.Visibility = Visibility.Visible;
                    FadeIn(warningLabel, UIElement.OpacityProperty, 0.3);
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserManager userManager = new UserManager();
            userManager.Logout();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            Window.GetWindow(this)?.Close();
        }

        private void useExternalTitle_Checked(object sender, RoutedEventArgs e)
        {
            openFileWithTitle.Visibility = Visibility.Visible;
            FadeIn(openFileWithTitle, UIElement.OpacityProperty, 0.5);
        }

        private void useExternalTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            FadeOut(openFileWithTitle, UIElement.OpacityProperty, 0.5);
            openFileWithTitle.Visibility = Visibility.Collapsed;
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Документы Word (*.docx)|*.docx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFileName.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                SelectedFilePath = openFileDialog.FileName;

                selected.Visibility = Visibility.Visible;
                FadeIn(selected, UIElement.OpacityProperty, 0.5);
            }
        }
        private const int MaxFilenameLength = 219; // Максимальная длина имени файла в Windows
        private const int MaxDescriptionLength = 499; // Максимальная длина описания
        private static readonly char[] InvalidChars = { '\\', '/', ':', '*', '"', '<', '>', '|' };
        private void templateName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            string text = textBox.Text;
            bool hasInvalidChars = text.IndexOfAny(InvalidChars) >= 0;
            bool isTooLong = textBox.Name == "templateName" ? text.Length > MaxFilenameLength : text.Length > MaxDescriptionLength;

            if (hasInvalidChars || isTooLong)
            {
                // Определяем нужный TextBlock для ошибки
                TextBlock errorTextBlock = textBox.Name == "templateName" ? errorname : errorname1;

                // Меняем текст ошибки
                errorTextBlock.Text = hasInvalidChars
                    ? "* Нежелательно использовать этот символ."
                    : "* Слишком длинное название. Подумайте над названием покороче.";

                // Удаляем недопустимые символы
                textBox.Text = new string(text.Where(c => !InvalidChars.Contains(c)).ToArray());

                // Устанавливаем курсор в конец
                textBox.CaretIndex = textBox.Text.Length;

                // Запускаем анимацию ошибки
                ShowErrorMessage(errorTextBlock);
            }
        }
        private void ShowErrorMessage(TextBlock errorTextBlock)
        {
            // Анимация появления ошибки на 3 секунды
            DoubleAnimation fadeInOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(3)))
            {
                BeginTime = TimeSpan.FromSeconds(0.5),
                AutoReverse = false
            };

            errorTextBlock.BeginAnimation(UIElement.OpacityProperty, fadeInOut);
        }
    }
    public class OneTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
    }
}
