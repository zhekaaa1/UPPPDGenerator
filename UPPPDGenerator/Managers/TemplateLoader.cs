using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using DocumentFormat.OpenXml.Vml;

namespace UPPPDGenerator.Managers
{
    public class TemplateLoader
    {
        private readonly StackPanel chooseTemplates;
        private readonly int currentUserId; // ID текущего пользователя

        public TemplateLoader(StackPanel chooseTemplates, int currentUserId)
        {
            this.chooseTemplates = chooseTemplates;
            this.currentUserId = currentUserId;
        }

        public void LoadTemplates()
        {
            chooseTemplates.Children.Clear(); // Очищаем список перед загрузкой

            string templatesDirectory = @"C:\Templates";
            if (!Directory.Exists(templatesDirectory))
            {
                Directory.CreateDirectory(templatesDirectory);
            }

            var templateFiles = Directory.GetFiles(templatesDirectory, "*.json");

            foreach (var filePath in templateFiles)
            {
                try
                {
                    var template = JsonSerializer.Deserialize<TemplateData>(File.ReadAllText(filePath));
                    if (template == null) continue;

                    // Создаем Grid (как в XAML)
                    Grid grid = new Grid
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 0, 0, 20)
                    };

                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition());

                    // Стиль для всех TextBlock
                    Style textStyle = new Style(typeof(TextBlock));
                    textStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.White));
                    textStyle.Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI")));
                    textStyle.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(5)));

                    grid.Resources.Add(typeof(TextBlock), textStyle);

                    // Блок с названием
                    StackPanel namePanel = new StackPanel { Orientation = Orientation.Vertical };
                    TextBlock nameTextBlock = new TextBlock
                    {
                        Text = template.TemplateName,
                        FontWeight = FontWeights.Bold,
                        FontSize = 16,
                        FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Comfortaa")
                    };
                    namePanel.Children.Add(nameTextBlock);

                    StackPanel authorPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    authorPanel.Children.Add(new TextBlock { Text = "Автор: ", FontSize = 14, FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Comfortaa") });
                    authorPanel.Children.Add(new TextBlock
                    {
                        Text = template.CreatedBy == currentUserId ? "Вы" : template.Author,
                        FontSize = 14,
                        Foreground = Brushes.LightGray,
                        FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Comfortaa")
                    });

                    namePanel.Children.Add(authorPanel);
                    Grid.SetColumn(namePanel, 1);
                    grid.Children.Add(namePanel);

                    DateTime parsedDate;
                    string dateToString;
                    if (DateTime.TryParse(template.CreatedAt, out parsedDate))
                    
                        dateToString = parsedDate.ToString("dd.MM.yyyy"); // 06.03.2025
                    
                    else
                        dateToString = "Неизвестная дата"; // если вдруг парсинг не удался
                    
                    // Блок с датой создания
                    StackPanel datePanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                    datePanel.Children.Add(new TextBlock { Text = "Дата создания: ", FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Comfortaa"), FontSize = 14, Foreground = Brushes.LightGray });
                    datePanel.Children.Add(new TextBlock { Text = dateToString, FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Fonts/#Comfortaa"), FontSize = 14, Foreground = Brushes.LightGray });

                    Grid.SetColumn(datePanel, 2);
                    grid.Children.Add(datePanel);

                    // Кнопка (три точки)
                    Button optionsButton = new Button
                    {
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Height = 30,
                        Width = 30,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        Style = (Style)Application.Current.Resources["threeDots"]
                    };
                    Grid.SetColumn(optionsButton, 3);
                    grid.Children.Add(optionsButton);

                    // Добавляем в StackPanel
                    chooseTemplates.Children.Add(grid);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке шаблона {filePath}: {ex.Message}");
                }
            }
        }
    }
    public class TemplateData
    {
        public string TemplateName { get; set; }
        public string Author { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedAt { get; set; }
    }
}
