using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UPPPDGenerator.Managers;
using UPPPDGenerator.DocumentSettings;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System.Diagnostics;

namespace UPPPDGenerator.Windows
{
    /// <summary>
    /// Логика взаимодействия для FillOutTheDocument.xaml
    /// </summary>
    public partial class FillOutTheDocument : Window
    {
        private TemplateJsonStructure LoadedTemplate;

        private string selectedImagePath;
        private bool enableDescriptions = true; // Заглушка, пока не привязано к шаблону
        private bool enableImageNumbering = true; // Заглушка, пока не привязано к шаблону
        private ImageResource SelectedImageResource;
        private HorizontalAlignment MainEditorHorizontalAlignment = HorizontalAlignment.Left;

        private List<ImageResource> imageResources = new List<ImageResource>();
        public FillOutTheDocument(TemplateJsonStructure loadedTemplate)
        {
            InitializeComponent();
            LoadedTemplate = loadedTemplate;
            Title = $"Редактор: {LoadedTemplate.TemplateName}";
            ApplyDocumentSettings(LoadedTemplate.DocumentSettings);
        }
        private void ApplyDocumentSettings(Settings settings)
        {
            if (settings == null) return;

            if (settings.ImageSettings.EnableDescriptions == true)
            {
                enableDescriptions = true;
            }
            if (settings.ImageSettings.EnableNumbering == true)
            {
                enableImageNumbering = true;
            }
            switch (settings.TextFieldSettings.Alignment)
            {
                case "По левому краю":
                    MainEditorHorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case "По правому краю":
                    MainEditorHorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case "По центру":
                    MainEditorHorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case "По ширине":
                    MainEditorHorizontalAlignment = HorizontalAlignment.Left;
                    break;
                default:
                    MainEditor.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
            MainEditor.HorizontalAlignment = MainEditorHorizontalAlignment;
            if (enableDescriptions) desc_SL.Visibility = Visibility.Visible;
        }
        private void ToggleBold_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in DocumentEditor.Children)
            {
                if (child is RichTextBox rtb && rtb.IsFocused)
                {
                    ToggleTextStyle(rtb, TextElement.FontWeightProperty, FontWeights.Bold, FontWeights.Normal);
                }
            }
        }

        private void ToggleItalic_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in DocumentEditor.Children)
            {
                if (child is RichTextBox rtb && rtb.IsFocused)
                {
                    ToggleTextStyle(rtb, TextElement.FontStyleProperty, FontStyles.Italic, FontStyles.Normal);
                }
            }
        }

        private void ToggleUnderline_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in DocumentEditor.Children)
            {
                if (child is RichTextBox rtb && rtb.IsFocused)
                {
                    var currentValue = rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                    if (currentValue == DependencyProperty.UnsetValue || !(currentValue is TextDecorationCollection decorations) || !decorations.Contains(TextDecorations.Underline[0]))
                    {
                        rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                    }
                    else
                    {
                        rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                    }
                }
            }
        }

        private void ToggleTextStyle(RichTextBox rtb, DependencyProperty property, object enabledValue, object disabledValue)
        {
            var currentValue = rtb.Selection.GetPropertyValue(property);
            if (currentValue == DependencyProperty.UnsetValue || !currentValue.Equals(enabledValue))
            {
                rtb.Selection.ApplyPropertyValue(property, enabledValue);
            }
            else
            {
                rtb.Selection.ApplyPropertyValue(property, disabledValue);
            }
        }

        private void AddImageReference_Click(object sender, RoutedEventArgs e)
        {
            ImageResourceList.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#595959"));
            ImageResourceList.SelectionChanged += ImageSelected;
            isSelectingForReference = true;
        }
        private void ImageSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ImageResourceList.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is ImageResource image)
            {
                ImageResourceList.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343434"));
                ImageResourceList.SelectionChanged -= ImageSelected; // Убираем обработчик

                InsertImageReference(image);
                isSelectingForReference = false;
                ImageResourceList.SelectedItem = null;
            }
        }
        private void InsertImageReference(ImageResource image)
        {
            StackPanel container = new StackPanel
            {
                Tag = image
            };
            // Контейнер с изображением
            Border imageBorder = new Border
            {
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.DarkGray,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 300,
                Margin = new Thickness(10)
            };
            StackPanel imagePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Image preview = new Image
            {
                Source = new BitmapImage(new Uri(image.FilePath)),
                Height = 40
            };

            TextBlock imageName = new TextBlock
            {
                Text = image.FileName,
                Foreground = Brushes.White,
                FontSize = 16,
                Margin = new Thickness(20, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            imagePanel.Children.Add(preview);
            imagePanel.Children.Add(imageName);
            imageBorder.Child = imagePanel;
            imageBorder.MouseLeftButtonDown += (s, e) => OpenImagePreview(image);
            container.Children.Add(imageBorder);
            RichTextBox bottomEditor = new RichTextBox
            {
                Padding = new Thickness(10),
                BorderThickness = new Thickness(0),
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontFamily = (FontFamily)FindResource("Comfortaa")
            };
            switch (Properties.Settings.Default.DocumentEditorDarkTheme)
            {
                case true:
                    bottomEditor.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343434"));
                    bottomEditor.Foreground = Brushes.White;
                    break;
                case false:
                    bottomEditor.Background = Brushes.White;
                    bottomEditor.Foreground = Brushes.Black;
                    break;
            }
            DocumentEditor.Children.Add(container);
            DocumentEditor.Children.Add(bottomEditor);
        }

        private void OpenImagePreview(ImageResource image)
        {
            Window previewWindow = new Window
            {
                Title = $"Просмотр: {image.FileName}",
                Width = 600,
                Height = 600,
                Background = Brushes.Black,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            Image fullSizeImage = new Image
            {
                Source = new BitmapImage(new Uri(image.FilePath)),
                Stretch = Stretch.Uniform
            };

            previewWindow.Content = fullSizeImage;
            previewWindow.ShowDialog();
        }

        private void AddTableReference_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления ссылки на таблицу в разработке.");
        }

        private async void AddResource_Click(object sender, RoutedEventArgs e)
        {
            Back_Button.Visibility = Visibility.Visible;
            selectedImagePath = null;
            Cancel_Button.Visibility = Visibility.Collapsed;
            desc_SL.Visibility = Visibility.Collapsed;
            AddImageSourceButton.Visibility = Visibility.Collapsed;
            ImageDescription.Text = "";
            PreviewImage.Source = null;
            Save_Button.Visibility = Visibility.Collapsed;
            Delete_Button.Visibility = Visibility.Collapsed;
            await AnimationManager.FadeIn(this, addResourceDialogue);
        }

        private async void ResourceType_Checked(object sender, RoutedEventArgs e)
        {
            Back_Button.Visibility = Visibility.Collapsed;
            if (ImageRadioButton.IsChecked == true)
            {
                AddImageSourceButton.Content = "Открыть изображение...";
                await AnimationManager.FadeOut(this, TableOptions);
                Delete_Button.Visibility = Visibility.Collapsed;
                Save_Button.Visibility = Visibility.Collapsed;
                Add_Button.Visibility = Visibility.Visible;
                Cancel_Button.Visibility = Visibility.Visible;
                AddImageSourceButton.Visibility = Visibility.Visible;
                await AnimationManager.FadeIn(this, ImageOptions);
            }
            else if (TableRadioButton.IsChecked == true)
            {
                await AnimationManager.FadeOut(this, ImageOptions);
                await AnimationManager.FadeIn(this, TableOptions);
            }
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(selectedImagePath));
                Add_Button.IsEnabled = true;
                AddImageSourceButton.Content = "Изменить изображение...";
                if (enableDescriptions)
                {
                    desc_SL.Visibility = Visibility.Visible;
                }
            }
        }

        private async void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                ErrorContainer.Show("Выберите изображение!",Elements.ErrorType.Warning);
                return;
            }
            if (enableDescriptions && string.IsNullOrEmpty(ImageDescription.Text.Trim()))
            {
                ErrorContainer.Show("Описание изображения обязательно. Введите его и повторите попытку.",Elements.ErrorType.Warning);
                return;
            }

            await AnimationManager.FadeOut(this, addResourceDialogue);
            var newImage = new ImageResource
            {
                Id = imageResources.Count + 1,
                FilePath = selectedImagePath,
                Description = enableDescriptions ? ImageDescription.Text : "",
                EnableNumbering = enableImageNumbering,
                EnableDescriptions = enableDescriptions
            };
            selectedImagePath = null;
            imageResources.Add(newImage);
            Back_Button.Visibility = Visibility.Visible;
            ImageResourceList.Items.Add(new ListBoxItem { Content = newImage.DisplayName, Tag = newImage });
            Save_Button.Visibility = Visibility.Collapsed;
        }
        private bool isSelectingForReference = false;
        private async void ShowImageInResourceDesigner(ImageResource imageResource)
        {
            if (imageResource == null) return;
            if (isSelectingForReference) { return; }
            SelectedImageResource = imageResource;
            ImageRadioButton.IsChecked = true;
            Save_Button.Visibility = Visibility.Visible;
            Back_Button.Visibility = Visibility.Collapsed;
            Add_Button.Visibility = Visibility.Collapsed;
            Delete_Button.Visibility = Visibility.Visible;
            if (imageResource.Description != null)
            {
                desc_SL.Visibility = Visibility.Visible;
                ImageDescription.Text = imageResource.Description;
            }
            else
            {
                desc_SL.Visibility = Visibility.Collapsed;
                ImageDescription.Text = "";
            }
            PreviewImage.Source = new BitmapImage(new Uri(imageResource.FilePath));
            Back_Button.Visibility = Visibility.Collapsed;
            await AnimationManager.FadeIn(this,addResourceDialogue);
        }

        private async void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this, addResourceDialogue);
            Back_Button.Visibility = Visibility.Visible;
            ImageRadioButton.IsChecked = false;
            TableRadioButton.IsChecked = false;
            ImageOptions.Visibility = Visibility.Collapsed;
            TableOptions.Visibility = Visibility.Collapsed;
            if (ImageResourceList.SelectedItem != null)
            {
                ImageResourceList.SelectedItem = null;
            }
        }
        private void ResourceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageResourceList.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is ImageResource image)
            {
                ShowImageInResourceDesigner(image);
            }
        }

        private async void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this, addResourceDialogue);
        }

        private async void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedImageResource == null) return;

            // Обновляем описание
            string newDescription = ImageDescription.Text;
            SelectedImageResource.Description = newDescription;
            SelectedImageResource.FilePath = PreviewImage.Source.ToString();

            // Обновляем элемент в ListBox
            foreach (var item in ImageResourceList.Items)
            {
                if (item is ListBoxItem listBoxItem && listBoxItem.Tag is ImageResource img && img.FilePath == SelectedImageResource.FilePath)
                {
                    listBoxItem.Content = SelectedImageResource.DisplayName;
                    break;
                }
            }

            ImageResourceList.SelectedItem = null;
            // Закрываем окно редактирования
            await AnimationManager.FadeOut(this, addResourceDialogue);
        }

        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedImageResource == null) return;

            // Удаляем ресурс из списка
            imageResources.Remove(SelectedImageResource);

            // Удаляем из ListBox
            ListBoxItem itemToRemove = null;
            foreach (var item in ImageResourceList.Items)
            {
                if (item is ListBoxItem listBoxItem && listBoxItem.Tag is ImageResource img && img.FilePath == SelectedImageResource.FilePath)
                {
                    itemToRemove = listBoxItem;
                    break;
                }
            }

            if (itemToRemove != null)
            {
                ImageResourceList.Items.Remove(itemToRemove);
            }

            // Пересчет номеров изображений
            for (int i = 0; i < imageResources.Count; i++)
            {
                imageResources[i].Id = i + 1;
            }

            // Обновляем ListBox
            RefreshImageResourceList();

            // Сбрасываем текущий выбранный ресурс
            SelectedImageResource = null;

            // Закрываем окно редактирования
            await AnimationManager.FadeOut(this, addResourceDialogue);
        }
        private void RefreshImageResourceList()
        {
            ImageResourceList.Items.Clear();

            foreach (var image in imageResources)
            {
                ListBoxItem newItem = new ListBoxItem
                {
                    Content = image.DisplayName,
                    Tag = image
                };

                ImageResourceList.Items.Add(newItem);
            }
        }

        private void Generate_Button_Click(object sender, RoutedEventArgs e)
        {
            new DocumentGENERATOR().GenerateDocument(LoadedTemplate, DocumentEditor, out string outputPath);
            
            if (!Properties.Settings.Default.StartMsOfficeOnFinish)
            {
                ErrorContainer.Show($"Готово! Документ сохранен в файле: {outputPath}", Elements.ErrorType.Info, 6000);
                return;
            }
            else
            {
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWin().Show();
            Close();
        }

        private void ThemeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ChangeDocumentEditorTheme();
        }

        private void ThemeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeDocumentEditorTheme();
        }
        private void ChangeDocumentEditorTheme()
        {
            if (Properties.Settings.Default.DocumentEditorDarkTheme)
            {
                foreach (var child in DocumentEditor.Children)
                {
                    if (child is RichTextBox rtb)
                    {
                        rtb.Background = new SolidColorBrush(Colors.White);
                        rtb.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
                Properties.Settings.Default.DocumentEditorDarkTheme = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                foreach (var child in DocumentEditor.Children)
                {
                    if (child is RichTextBox rtb)
                    {
                        rtb.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#343434"));
                        rtb.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
                Properties.Settings.Default.DocumentEditorDarkTheme = true;
                Properties.Settings.Default.Save();
            }
        }
    }
    public class ImageResource
    {
        public int Id { get; set; } // Порядковый номер
        public string FilePath { get; set; } // Полный путь к файлу
        public string FileName => System.IO.Path.GetFileName(FilePath); // Только название файла
        public string Description { get; set; } // Описание изображения
        public bool EnableNumbering { get; set; } // Включена ли нумерация
        public bool EnableDescriptions { get; set; } // Включено ли описание

        public string DisplayName
        {
            get
            {
                if (EnableNumbering && EnableDescriptions)
                    return $"Рисунок {Id} – {Description}";
                if (EnableNumbering)
                    return $"Рисунок {Id}";
                if (EnableDescriptions)
                    return Description;
                return FileName;
            }
        }
    }
}
