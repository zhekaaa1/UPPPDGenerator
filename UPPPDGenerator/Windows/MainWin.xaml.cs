using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using UPPPDGenerator.DocumentSettings;
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
            bool isAuthorized = Properties.Settings.Default.LogonUserId != 0;
            if (isAuthorized) 
            {
                logonUserName.Text = Properties.Settings.Default.LogonUserFullName;
                n3.Visibility = Visibility.Visible;
                n5.Visibility = Visibility.Visible;
                n6.Visibility = Visibility.Collapsed;
            }
            else
            {
                logonUserName.Text = "Анонимный пользователь";
                n3.Visibility = Visibility.Collapsed;
                n5.Visibility = Visibility.Collapsed;
                n6.Visibility = Visibility.Visible;
            }
            TemplatesListView.ItemsSource = new TemplateLoader().LoadTemplates();
            if (LangManager.GetLangNum() == 2)
            TemplateAccessMode.ItemsSource = new List<string>()
            {
                "Приватный", "Публичный", "Ограниченный"
            };
            else
            TemplateAccessMode.ItemsSource = new List<string>()
            {
                "Private", "Public", "Restricted"
            };
        }
        private async void defence_Checked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeIn(this, passfortemplate);
        }

        private async void defence_Unchecked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this, passfortemplate);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            n1.CollapseButton();
            await AnimationManager.FadeOut(this, createb);
            ButtonOnMainScr.ResetActiveButton(); // сбрасываем активную кнопку
        }
        

        private async void OnButtonClicked(ButtonOnMainScr clickedButton)
        {
            // Скрываем все контейнеры
            await AnimationManager.FadeOut(this, createb);
            await AnimationManager.FadeOut(this, fillb);
            await AnimationManager.FadeOut(this, settingsb);
            await AnimationManager.FadeOut(this, mytemplatesb);
            await AnimationManager.FadeOut(this, logoutb);
            await AnimationManager.FadeOut(this, loginb);
            // Показываем соответствующий контейнер
            switch (clickedButton.Tag)
            {
                case "b1":
                    await AnimationManager.FadeIn(this, createb);
                    break;
                case "b2":
                    await AnimationManager.FadeIn(this, fillb);
                    break;
                case "b3":
                    await AnimationManager.FadeIn(this, mytemplatesb);
                    break;
                case "b4":
                    await AnimationManager.FadeIn(this, settingsb);
                    break;
                case "b5":
                    await AnimationManager.FadeIn(this, logoutb);
                    break;
                case "b6":
                    await AnimationManager.FadeIn(this, loginb);
                    break;
            }
        }
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(templateName.Text.Trim()) || templateName.Text.Trim() == " ")
            {
                ShowErrorMessage("Назовите шаблон!");
                return;
            }

            bool isAuthorized = Properties.Settings.Default.LogonUserId != 0;
            var template = new TemplateJsonStructure();
            template.TemplateName = templateName.Text.Trim();
            if (!string.IsNullOrEmpty(description.Text)) template.TemplateDescription = description.Text;
            template.TemplatePasswordHash = passwordInTemplate.GetPassword();
            //по твоей просьбе
            template.CreatedAt = DateTime.UtcNow;
            if (isAuthorized) template.CreatedBy = new AuthorInfo { Id = Properties.Settings.Default.LogonUserId, FullName = Properties.Settings.Default.LogonUserFullName };
            if (!string.IsNullOrEmpty(SelectedFilePath))
            {
                template.TitlePageXml = GetXmlFromDocx(SelectedFilePath);
            }
            TemplateAccessMode accessMode = Managers.TemplateAccessMode.Private;


            switch (TemplateAccessMode.SelectedIndex)
            {
                case 0:
                    accessMode = Managers.TemplateAccessMode.Private;
                    break;
                case 1:
                    accessMode = Managers.TemplateAccessMode.Public;
                    break;
                default:
                    accessMode = Managers.TemplateAccessMode.Restricted;
                    break;
            }
            bool isServerAvailable = await App.IsServerAvailable();
            bool isInternetAvailable = NetworkInterface.GetIsNetworkAvailable();
            if (!isInternetAvailable)
            {
                ErrorContainer.Show("Произошла ошибка: Интернет соединение отсутствует. Проверьте интернет соединение или установите уровень доступа \"Публичный\" или \"Приватный\"", ErrorType.Critical);
            }
            if (accessMode == Managers.TemplateAccessMode.Restricted && !isServerAvailable)
            {
                ErrorContainer.Show("Произошла ошибка: Сервер недоступен. Попробуйте позже или установите уровень доступа \"Публичный\" или \"Приватный\"", ErrorType.Warning);
                return;
            }
            if (accessMode == Managers.TemplateAccessMode.Restricted && !isAuthorized)
            {
                ErrorContainer.Show("Войдите в систему, чтобы устанавливать режим доступа \"Ограниченный\"", ErrorType.Info);
                return;
            }
            if (useExternalTitle.IsChecked == true && useTitlePage.IsChecked == true && SelectedFilePath != string.Empty)
            {
                if (!File.Exists(SelectedFilePath))
                {
                    ErrorContainer.Show($"Произошла ошибка: Файл {selectedFileName} был перемещен или удален. Повторите попытку с использованием другого файла.", ErrorType.Warning);
                    return;
                }
                if (GetXmlFromDocx(SelectedFilePath) == "")
                {
                    ErrorContainer.Show($"Произошла ошибка: Файл {selectedFileName} повреждён. Используйте другой файл.", ErrorType.Critical);
                    return;
                }
                template.UseTitlePage = true;
                template.TitlePageXml = GetXmlFromDocx(SelectedFilePath);
                new CreateWithoutTitleTemplateWin(template, accessMode).Show();
                Close();
            }
            else if (useTitlePage.IsChecked == true)
            {
                template.UseTitlePage = true;
                new CreateTemplateWin(template, accessMode).Show();
                Close();
            }
            else
            {
                template.UseTitlePage = false;
                new CreateWithoutTitleTemplateWin(template, accessMode).Show();
                Close();
            }
        }
        public string GetXmlFromDocx(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var mainPart = wordDoc.MainDocumentPart;
                return mainPart?.Document?.Body?.InnerXml ?? "";
            }
        }

        private async void langComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    await AnimationManager.FadeIn(this, warningLabel);
                }
            }
        }
        

        private async void useExternalTitle_Checked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeIn(this, openFileWithTitle);
        }

        private async void useExternalTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this, openFileWithTitle);
        }

        private async void selectFile_Click(object sender, RoutedEventArgs e)
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
                await AnimationManager.FadeIn(this, selected);
            }
        }
        private static readonly char[] InvalidChars = { '\\', '/', ':', '*', '"', '<', '>', '|' };
        
        private void ShowErrorMessage(string errorTextBlock)
        {
            ErrorContainer.Show(errorTextBlock, ErrorType.Warning);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserManager userManager = new UserManager();
            userManager.Logout();
            new MainWindow().Show();
            Close();
        }

        private async void TemplatesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TemplatesListView.SelectedItem is TemplateViewModel viewModel)
            {
                if (viewModel.TemplateDecoded.TemplateIdFromDatabase == 0)
                {
                    new FillOutTheDocument(viewModel.TemplateDecoded).Show();
                    Close();
                }
                else if (viewModel.TemplateDecoded.TemplateIdFromDatabase < 0)
                {
                    if (Properties.Settings.Default.LogonUserId != viewModel.TemplateDecoded.CreatedBy.Id)
                    {
                        ErrorContainer.Show("Доступ к данному шаблону запрещен.",ErrorType.Critical);
                    }
                    else
                    {
                        new FillOutTheDocument(viewModel.TemplateDecoded).Show();
                        Close();
                    }
                }
                else
                {
                    bool isServerAvailable = await App.IsServerAvailable();
                    bool isInternetAvailable = NetworkInterface.GetIsNetworkAvailable();
                    if (!isInternetAvailable || Properties.Settings.Default.LogonUserId == 0)
                    {
                        ErrorContainer.Show("Доступ к данному шаблону могут иметь авторизованные пользователи с интернет соединением.", ErrorType.Critical);
                        return;
                    }
                    if (!isServerAvailable)
                    {
                        ErrorContainer.Show("Произошла ошибка: Сервер недоступен. Попробуйте позже или установите уровень доступа \"Публичный\" или \"Приватный\"", ErrorType.Warning);
                        return;
                    }
                    SelectedTemplate = viewModel.TemplateDecoded;
                    await AnimationManager.FadeIn(this, TemplatePasswordSl);
                }
            }
        }
        private TemplateJsonStructure SelectedTemplate {  get; set; }

        private void TemplateProperties_Click(object sender, RoutedEventArgs e)
        {
            if (TemplatesListView.SelectedItem is TemplateViewModel viewModel)
            {
                MessageBox.Show($"Шаблон: {viewModel.TemplateName}\n" +
                                $"{viewModel.AuthorName}\n" +
                                $"{viewModel.CreatedAtFormatted}\n\n" +
                                $"Путь к файлу:\n{viewModel.FilePath}",
                                "Свойства шаблона",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }
        private async void useTitlePage_Checked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeIn(this, useExternalTitle);
        }
        private async void useTitlePage_Unchecked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this, useExternalTitle);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
        public bool DevModeOn = false;
        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DevModeOn = true;
            ErrorContainer.Show("Вы включили режим разработчика. Параметры доступны во вкладке \"Настройки\"",ErrorType.Info);
            await AnimationManager.FadeIn(this, ForDevelopersStackPanel);
        }
        private async void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DevModeOn = false;
            ErrorContainer.Show("Вы отключили режим разработчика.", ErrorType.Info);
            await AnimationManager.FadeOut(this, ForDevelopersStackPanel);
        }
        private void SaveJson_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveJson = true;
            Properties.Settings.Default.Save();
        }
        private void SaveJson_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveJson = false;
            Properties.Settings.Default.Save();
        }

        private void OpenOnFinish_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartMsOfficeOnFinish = true;
            Properties.Settings.Default.Save();
        }

        private void OpenOnFinish_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartMsOfficeOnFinish = false;
            Properties.Settings.Default.Save();
        }

        private async void CancelPassButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplate = null;
            await AnimationManager.FadeOut(this, TemplatePasswordSl);
        }
        private async void ConfirmPassButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTemplate.TemplatePasswordHash != templatePass.GetPassword())
            {
                ErrorContainer.Show("Ошибка: неверный пароль шаблона.",ErrorType.Critical);
                return;
            }
            else
            {
                bool success = await new TemplateManager().AddAccessToTemplate(SelectedTemplate.TemplateIdFromDatabase);
                if (success)
                {
                    new FillOutTheDocument(SelectedTemplate).Show();
                    Close();
                }
            }
        }
    }
}
