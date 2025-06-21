using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UPPPDGenerator.DocumentSettings;
using UPPPDGenerator.Managers;
using Microsoft.Win32;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace UPPPDGenerator.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateWithoutTitleTemplateWin.xaml
    /// </summary>
    public partial class CreateWithoutTitleTemplateWin : Window
    {
        private TemplateJsonStructure TemplateJsonStructure { get; set; }
        private TemplateAccessMode TemplateAccessMode { get; set; }
        private static readonly Dictionary<string, PageSettings> PresetPageSettings = new Dictionary<string, PageSettings>
        {
            { "Обычные", new PageSettings { Name = "Обычные", TopMargin = 2, BottomMargin = 2, LeftMargin = 3, RightMargin = 1.5 } },
            { "Узкие", new PageSettings { Name = "Узкие", TopMargin = 1.27, BottomMargin = 1.27, LeftMargin = 1.27, RightMargin = 1.27 } },
            { "Средние", new PageSettings { Name = "Средние", TopMargin = 2.54, BottomMargin = 2.54, LeftMargin = 1.91, RightMargin = 1.91 } },
            { "Широкие", new PageSettings { Name = "Широкие", TopMargin = 2.54, BottomMargin = 2.54, LeftMargin = 5.08, RightMargin = 5.08 } }
        };
        private string SelectedFilePath = string.Empty;
        private Settings SelectedFileSettings { get; set; } = new Settings();
        public CreateWithoutTitleTemplateWin(TemplateJsonStructure preparingTemplate, TemplateAccessMode templateAccessMode)
        {
            InitializeComponent();
            TemplateJsonStructure = preparingTemplate;
            TemplateAccessMode = templateAccessMode;
            LoadTextFieldSettings();
            LineSpacingComboBox.SelectedIndex = 0;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MainWin().Show();
            Close();
        }

        private async void useDefaultParameters_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this,textInsideTableParameters);
        }
        private async void useDefaultParameters_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeIn(this, textInsideTableParameters);
        }
        private async void colorizeHeaders_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeIn(this,tableHeadersColor);
        }
        private async void colorizeHeaders_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeOut(this,tableHeadersColor);
        }
        private async void Generate_Click(object sender, RoutedEventArgs e)
        {            
            // Собираем настройки
            var settings = new Settings
            {
                PageSettings = GetPageSettings(),
                TextFieldSettings = GetTextFieldSettings(),
                ImageSettings = GetImageSettings(),
                TableSettings = GetTableSettings()
            };
            TemplateJsonStructure.DocumentSettings = settings;

            await TemplateGENERATOR.GenerateTemplate(TemplateJsonStructure, TemplateAccessMode);
        }
        private TextFieldSettings GetTextFieldSettings() => textFieldSettings;
        private PageSettings GetPageSettings()
        {
            if (listParamsChoise.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedName = selectedItem.Content.ToString();

                if (PresetPageSettings.ContainsKey(selectedName))
                {
                    return PresetPageSettings[selectedName];
                }
                else
                {
                    return new PageSettings
                    {
                        Name = "Custom",
                        TopMargin = double.Parse(listPaddingTop.Text),
                        BottomMargin = double.Parse(listPaddingBottom.Text),
                        LeftMargin = double.Parse(listPaddingLeft.Text),
                        RightMargin = double.Parse(listPaddingRight.Text)
                    };
                }
            }
            return null;
        }
        private bool isUpdating = false;
        private void listParamsChoise_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listPaddingTop == null)
            {
                return;
            }
            if (listParamsChoise.SelectedItem is ComboBoxItem selectedItem)
            {
                isUpdating = true;
                string selectedName = selectedItem.Content.ToString();

                if (PresetPageSettings.ContainsKey(selectedName))
                {
                    PageSettings preset = PresetPageSettings[selectedName];

                    listPaddingTop.Text = preset.TopMargin.ToString();
                    listPaddingBottom.Text = preset.BottomMargin.ToString();
                    listPaddingLeft.Text = preset.LeftMargin.ToString();
                    listPaddingRight.Text = preset.RightMargin.ToString();
                }
                isUpdating = false;
            }
        }
        private void CheckIfCustomMargins()
        {
            if (listPaddingTop == null) return;
            if (listPaddingBottom == null) return;
            if (listPaddingLeft == null) return;
            if (listPaddingRight == null) return;
            if (isUpdating) return;
            double.TryParse(listPaddingTop.Text, out double top);
            double.TryParse(listPaddingBottom.Text, out double bottom);
            double.TryParse(listPaddingLeft.Text, out double left);
            double.TryParse(listPaddingRight.Text, out double right);

            bool isPreset = PresetPageSettings.Any(preset =>
                preset.Value.TopMargin == top &&
                preset.Value.BottomMargin == bottom &&
                preset.Value.LeftMargin == left &&
                preset.Value.RightMargin == right
            );

            if (!isPreset)
            {
                listParamsChoise.SelectedIndex = listParamsChoise.Items.Count - 1;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isUpdating)
            {
                CheckIfCustomMargins();
            }
            if (sender is TextBox textBox)
            {
                TextBoxMustReturnDouble(textBox);
            }
        }
        private void TextBoxMustReturnDouble(TextBox textBox)
        {
            string oldText = textBox.Text;
            StringBuilder newText = new StringBuilder();
            bool hasDecimalPoint = false;

            foreach (char c in oldText)
            {
                if (char.IsDigit(c))
                {
                    newText.Append(c);
                }
                else if ((c == '.' || c == ',') && !hasDecimalPoint)
                {
                    newText.Append(',');
                    hasDecimalPoint = true;
                }
            }

            if (oldText != newText.ToString())
            {
                int caretPosition = textBox.SelectionStart; 
                textBox.Text = newText.ToString();
                textBox.SelectionStart = Math.Min(caretPosition, textBox.Text.Length); 
            }
        }


        public Dictionary<string, (double Min, double Max, string Description, bool UseDecimals)> FieldValidationRules = new Dictionary<string, (double Min, double Max, string Description, bool UseDecimals)>()
        {
            ["listPaddingTop"] = (0, 8, "отступа страницы (верхний)", true),
            ["listPaddingBottom"] = (0, 8, "отступа страницы (нижний)", true),
            ["listPaddingLeft"] = (0, 8, "отступа страницы (левый)", true),
            ["listPaddingRight"] = (0, 8, "отступа страницы (правый)", true),

            ["MarginLeftTextBox"] = (0, 8, "отступа абзаца (слева)", true),
            ["MarginRightTextBox"] = (0, 8, "отступа абзаца (справа)", true),
            ["MarginTopTextBox"] = (0, 8, "интервала до абзаца (сверху)", true),
            ["MarginBottomTextBox"] = (0, 8, "интервала после абзаца (снизу)", true),
            ["FirstLineIndentationTextBox"] = (0, 4, "отступа первой строки", true),
            ["LineSpacingMultiplierTextBox"] = (0.5, 3, "межстрочного интервала", true),

            ["tableText_MarginLeftTextBox"] = (0, 8, "отступа таблицы (слева)", true),
            ["tableText_MarginRightTextBox"] = (0, 8, "отступа таблицы (справа)", true),
            ["tableText_MarginTopTextBox"] = (0, 8, "интервала до строки таблицы (сверху)", true),
            ["tableText_MarginBottomTextBox"] = (0, 8, "интервала после строки таблицы (снизу)", true),
            ["tableText_FirstLineIndentationTextBox"] = (0, 4, "отступа первой строки в таблице", true),
            ["tableText_LineSpacingMultiplierTextBox"] = (0.5, 3, "межстрочного интервала в таблице", true)
        };
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            string name = textBox.Name;
            string input = textBox.Text.Trim().Replace('.', ',');

            if (string.IsNullOrWhiteSpace(input) || input == "," || input == ".")
            {
                textBox.Text = "0";
                return;
            }

            if (input.StartsWith(",")) input = "0" + input;
            if (input.EndsWith(",")) input += "0";

            if (!double.TryParse(input, out double value))
            {
                textBox.Text = "0";
                return;
            }

            if (FieldValidationRules.TryGetValue(name, out var rule))
            {
                if (value < rule.Min || value > rule.Max)
                {
                    ErrorContainer.Show($"Обнаружено аномальное значение для {rule.Description}. Допустимо: от {rule.Min} до {rule.Max}.",Elements.ErrorType.Warning);
                    value = Clamp(value, rule.Min, rule.Max);
                }

                textBox.Text = rule.UseDecimals ? value.ToString("0.00") : ((int)value).ToString();
            }
            else
            {
                textBox.Text = value.ToString("0.00");
            }
        }
        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void LoadTextFieldSettings()
        {
            FontFamilyComboBox.SelectedItem = FontFamilyComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == textFieldSettings.FontFamily);

            FontSizeComboBox.SelectedItem = FontSizeComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == textFieldSettings.FontSize.ToString());

            ParagraphAlignmentComboBox.SelectedItem = ParagraphAlignmentComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == textFieldSettings.Alignment);

            MarginLeftTextBox.Text = textFieldSettings.MarginLeft.ToString();
            MarginRightTextBox.Text = textFieldSettings.MarginRight.ToString();
            MarginTopTextBox.Text = textFieldSettings.MarginTop.ToString();
            MarginBottomTextBox.Text = textFieldSettings.MarginBottom.ToString();
            FirstLineIndentationTextBox.Text = textFieldSettings.FirstLineIndentation.ToString();

            LineSpacingComboBox.SelectedItem = LineSpacingComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == textFieldSettings.LineSpacingType);

            if (textFieldSettings.LineSpacingType == "Множитель")
            {
                LineSpacingMultiplierLabel.Visibility = Visibility.Visible;
                LineSpacingMultiplierTextBox.Visibility = Visibility.Visible;
                LineSpacingMultiplierTextBox.Text = textFieldSettings.LineSpacingMultiplier.ToString();
            }
            else
            {
                LineSpacingMultiplierLabel.Visibility = Visibility.Collapsed;
                LineSpacingMultiplierTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private TextFieldSettings textFieldSettings = new TextFieldSettings();

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                textFieldSettings.FontFamily = selectedItem.Content.ToString();
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                textFieldSettings.FontSize = int.Parse(selectedItem.Content.ToString());
            }
        }

        private void ParagraphAlignmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParagraphAlignmentComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                textFieldSettings.Alignment = selectedItem.Content.ToString();
            }
        }

        private void MarginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                TextBoxMustReturnDouble(textBox);
                double.TryParse(textBox.Text, out double value);

                if (textBox == MarginLeftTextBox) textFieldSettings.MarginLeft = value;
                else if (textBox == MarginRightTextBox) textFieldSettings.MarginRight = value;
                else if (textBox == MarginTopTextBox) textFieldSettings.MarginTop = value;
                else if (textBox == MarginBottomTextBox) textFieldSettings.MarginBottom = value;
                else if (textBox == FirstLineIndentationTextBox) textFieldSettings.FirstLineIndentation = value;
            }
        }

        private void LineSpacingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LineSpacingComboBox == null) return;
            if (LineSpacingMultiplierTextBox == null) return;

            if (LineSpacingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                textFieldSettings.LineSpacingType = selectedItem.Content.ToString();

                if (textFieldSettings.LineSpacingType == "Множитель")
                {
                    LineSpacingMultiplierLabel.Visibility = Visibility.Visible;
                    LineSpacingMultiplierTextBox.Visibility = Visibility.Visible;
                }
                else
                {
                    LineSpacingMultiplierLabel.Visibility = Visibility.Collapsed;
                    LineSpacingMultiplierTextBox.Visibility = Visibility.Collapsed;
                    textFieldSettings.LineSpacingMultiplier = 1;
                }
            }
        }

        private void LineSpacingMultiplierTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            TextBoxMustReturnDouble(textBox);
            double.TryParse(LineSpacingMultiplierTextBox.Text, out double value);
            textFieldSettings.LineSpacingMultiplier = value;
        }


        private ImageSettings GetImageSettings()
        {
            return new ImageSettings
            {
                EnableNumbering = (bool) (EnableNumberingCheckBox.IsChecked ?? false), // Нумерация изображений
                EnableDescriptions = (bool) (giveDescriptionsForimages_CheckBox.IsChecked ?? false), // Описание изображений
                Alignment = (ImageAlignmentComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "По левому краю" // Выравнивание
            };
        }
        private TableSettings GetTableSettings()
        {
            bool useDefaultParagraphSettings = useDefaultParameters_CheckBox.IsChecked ?? false;
            return new TableSettings
            {
                NumberColumns = giveNumbersOfColsForTable_CheckBox.IsChecked ?? false,
                NameTables = giveNamesOfColsForTable_CheckBox.IsChecked ?? false,
                ColorizeHeaders = colorizeHeaders_CheckBox.IsChecked ?? false,
                EnableNumbering = numerizeTables_CheckBox.IsChecked ?? false,
                HeaderColor = (tableHeadersColor.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Голубой",

                UseDefaultParagraphSettings = useDefaultParagraphSettings,

                FontFamily = useDefaultParagraphSettings
            ? (FontFamilyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Times New Roman"
            : (tableText_FontFamilyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Times New Roman",

                FontSize = useDefaultParagraphSettings
            ? int.TryParse((FontSizeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), out int defaultFontSize) ? defaultFontSize : 12
            : int.TryParse((tableText_FontSizeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), out int tableFontSize) ? tableFontSize : 12,

                ParagraphAlignment = useDefaultParagraphSettings
            ? (ParagraphAlignmentComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "По левому краю"
            : (tableText_ParagraphAlignmentComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "По левому краю",

                MarginLeft = useDefaultParagraphSettings
            ? double.TryParse(MarginLeftTextBox.Text, out double defaultMarginLeft) ? defaultMarginLeft : 0.0
            : double.TryParse(tableText_MarginLeftTextBox.Text, out double tableMarginLeft) ? tableMarginLeft : 0.0,

                MarginRight = useDefaultParagraphSettings
            ? double.TryParse(MarginRightTextBox.Text, out double defaultMarginRight) ? defaultMarginRight : 0.0
            : double.TryParse(tableText_MarginRightTextBox.Text, out double tableMarginRight) ? tableMarginRight : 0.0,

                MarginTop = useDefaultParagraphSettings
            ? double.TryParse(MarginTopTextBox.Text, out double defaultMarginTop) ? defaultMarginTop : 0.0
            : double.TryParse(tableText_MarginTopTextBox.Text, out double tableMarginTop) ? tableMarginTop : 0.0,

                MarginBottom = useDefaultParagraphSettings
            ? double.TryParse(MarginBottomTextBox.Text, out double defaultMarginBottom) ? defaultMarginBottom : 0.0
            : double.TryParse(tableText_MarginBottomTextBox.Text, out double tableMarginBottom) ? tableMarginBottom : 0.0,

                FirstLineIndentation = useDefaultParagraphSettings
            ? double.TryParse(FirstLineIndentationTextBox.Text, out double defaultFirstLineIndentation) ? defaultFirstLineIndentation : 0.0
            : double.TryParse(tableText_FirstLineIndentationTextBox.Text, out double tableFirstLineIndentation) ? tableFirstLineIndentation : 0.0,

                LineSpacing = useDefaultParagraphSettings
            ? (LineSpacingComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Одинарный"
            : (tableText_LineSpacingComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Одинарный",

                LineSpacingMultiplier = useDefaultParagraphSettings
            ? double.TryParse(LineSpacingMultiplierTextBox.Text, out double defaultMultiplier) ? defaultMultiplier : 1.0
            : double.TryParse(tableText_LineSpacingMultiplierTextBox.Text, out double tableMultiplier) ? tableMultiplier : 1.0,

                VerticalAlignment = (tableText_VerticalAlignment.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Сверху"
            };
        }

        public void ApplySettingsToUI(Settings settings)
        {
            if (settings == null) return;

            switch (settings.PageSettings.Name)
            {
                case "Обычные": listParamsChoise.SelectedIndex = 0; break;
                case "Узкие": listParamsChoise.SelectedIndex = 1; break;
                case "Средние": listParamsChoise.SelectedIndex = 2; break;
                case "Широкие": listParamsChoise.SelectedIndex = 3; break;
                case "Другие": listParamsChoise.SelectedIndex = 4; break;
                default: listParamsChoise.SelectedIndex = 4; break;
            }
            listPaddingTop.Text = settings.PageSettings.TopMargin.ToString("0.00");
            listPaddingBottom.Text = settings.PageSettings.BottomMargin.ToString("0.00");
            listPaddingLeft.Text = settings.PageSettings.LeftMargin.ToString("0.00");
            listPaddingRight.Text = settings.PageSettings.RightMargin.ToString("0.00");

            switch (settings.TextFieldSettings.FontFamily)
            {
                case "Times New Roman":
                    FontFamilyComboBox.SelectedIndex = 0;
                    break;
                case "Arial":
                    FontFamilyComboBox.SelectedIndex = 0;
                    break;
                case "Calibri":
                    FontFamilyComboBox.SelectedIndex = 0;
                    break;
                default : FontFamilyComboBox.SelectedIndex = 0; break;
            }
            FontSizeComboBox.Text = settings.TextFieldSettings.FontSize.ToString();
            switch (settings.TextFieldSettings.Alignment)
            {
                case "По левому краю":
                    ParagraphAlignmentComboBox.SelectedIndex = 0;
                    break;
                case "По правому краю":
                    ParagraphAlignmentComboBox.SelectedIndex = 1;
                    break;
                case "По центру":
                    ParagraphAlignmentComboBox.SelectedIndex = 2;
                    break;
                case "По ширине":
                    ParagraphAlignmentComboBox.SelectedIndex = 3;
                    break;
                    
            }
            FirstLineIndentationTextBox.Text = settings.TextFieldSettings.FirstLineIndentation.ToString("0.00");
            switch (settings.TextFieldSettings.LineSpacingType)
            {
                case "Одинарный":
                    LineSpacingComboBox.SelectedIndex = 0;
                    break;
                case "Полуторный":
                    LineSpacingComboBox.SelectedIndex = 1;
                    break;
                case "Двойной":
                    LineSpacingComboBox.SelectedIndex = 2;
                    break;
                case "Множитель":
                    LineSpacingComboBox.SelectedIndex = 3;
                    LineSpacingMultiplierTextBox.Text = settings.TextFieldSettings.LineSpacingMultiplier.ToString();
                    break;
            }

            EnableNumberingCheckBox.IsChecked = settings.ImageSettings.EnableNumbering;
            giveDescriptionsForimages_CheckBox.IsChecked = settings.ImageSettings.EnableDescriptions;
            switch (settings.ImageSettings.Alignment)
            {
                case "По левому краю":
                    ImageAlignmentComboBox.SelectedIndex = 0;
                    break;
                case "По правому краю":
                    ImageAlignmentComboBox.SelectedIndex = 1;
                    break;
                case "По центру":
                    ImageAlignmentComboBox.SelectedIndex = 2;
                    break;
                case "По ширине":
                    ImageAlignmentComboBox.SelectedIndex = 3;
                    break;
            }

            numerizeTables_CheckBox.IsChecked = settings.TableSettings.NumberColumns;
            giveNamesOfColsForTable_CheckBox.IsChecked = settings.TableSettings.NameTables;
            colorizeHeaders_CheckBox.IsChecked = settings.TableSettings.ColorizeHeaders;
            giveNumbersOfColsForTable_CheckBox.IsChecked = settings.TableSettings.EnableNumbering;
            switch (settings.TableSettings.HeaderColor)
            {
                case "Голубой":
                    tableHeadersColor.SelectedIndex = 0;
                    break;
                case "Желтоватый":
                    tableHeadersColor.SelectedIndex = 1;
                    break;
                case "Зеленый":
                    tableHeadersColor.SelectedIndex = 2;
                    break;
                case "Зелёный":
                    tableHeadersColor.SelectedIndex = 2;
                    break;
                default:
                    tableHeadersColor.SelectedIndex = 0;
                    break;
            }

            useDefaultParameters_CheckBox.IsChecked = settings.TableSettings.UseDefaultParagraphSettings;

            tableText_FontFamilyComboBox.SelectedItem = settings.TableSettings.FontFamily;
            tableText_FontSizeComboBox.SelectedItem = settings.TableSettings.FontSize.ToString();
            tableText_ParagraphAlignmentComboBox.SelectedItem = settings.TableSettings.ParagraphAlignment;
            tableText_MarginBottomTextBox.Text = settings.TableSettings.MarginBottom.ToString("0.00");
            tableText_MarginTopTextBox.Text = settings.TableSettings.MarginTop.ToString("0.00");
            tableText_MarginLeftTextBox.Text = settings.TableSettings.MarginLeft.ToString("0.00");
            tableText_MarginRightTextBox.Text = settings.TableSettings.MarginRight.ToString("0.00");
            tableText_FirstLineIndentationTextBox.Text = settings.TableSettings.FirstLineIndentation.ToString("0.00");
            tableText_LineSpacingComboBox.SelectedItem = settings.TableSettings.LineSpacing;
            tableText_LineSpacingMultiplierLabel.Text = settings.TableSettings.LineSpacingMultiplier.ToString("0.00");

            tableText_VerticalAlignment.SelectedItem = settings.TableSettings.VerticalAlignment;
            switch (settings.TableSettings.VerticalAlignment)
            {
                case "Сверху":
                    tableHeadersColor.SelectedIndex = 0;
                    break;
                case "По центру":
                    tableHeadersColor.SelectedIndex = 1;
                    break;
                case "Снизу":
                    tableHeadersColor.SelectedIndex = 2;
                    break;
            }
        }

        private async void ChoiseFileForTemplate_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Шаблоны документов (*.ugt)|*.ugt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                templateName.Text = Path.GetFileName(openFileDialog.FileName);
                SelectedFilePath = openFileDialog.FileName;
                SelectedFileSettings = LoadPreview();
                await AnimationManager.FadeIn(this,choosenFile);
                await AnimationManager.FadeIn(this,preView);
            }
        }
        private async void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedFilePath = string.Empty;
            SelectedFileSettings = new Settings();
            await AnimationManager.FadeOut(this, preView);
            await AnimationManager.FadeOut(this, choosenFile);
        }
        public Settings GOSTSettings = new Settings()
        {
            PageSettings = new PageSettings()
            {
                Name = "Другие",
                LeftMargin = 3,
                RightMargin = 1,
                TopMargin = 2,
                BottomMargin = 2
            },
            TextFieldSettings = new TextFieldSettings()
            {
                FontFamily = "Times New Roman",
                FontSize = 14,
                MarginBottom = 0,
                MarginTop = 0,
                MarginRight = 0,
                MarginLeft = 0,
                FirstLineIndentation = 1.25,
                Alignment = "По ширине",
                LineSpacingType = "Полуторный",
                LineSpacingMultiplier = 0
            },
            ImageSettings = new ImageSettings()
            {
                EnableDescriptions = true,
                EnableNumbering = true,
                Alignment = "По центру"
            },
            TableSettings = new TableSettings()
            {
                NumberColumns = true,
                ColorizeHeaders = false,
                HeaderColor = "Голубоватый",
                NameTables = true,
                EnableNumbering = true,
                VerticalAlignment = "Сверху",
                UseDefaultParagraphSettings = true,

                FontFamily = "Times New Roman",
                FontSize = 14,
                MarginBottom = 0,
                MarginTop = 0,
                MarginRight = 0,
                MarginLeft = 0,
                FirstLineIndentation = 1.25,
                ParagraphAlignment = "По ширине",
                LineSpacing = "Полуторный",
                LineSpacingMultiplier = 0
            }
        };
        private Settings LoadPreview()
        {
            try
            {
                Settings settings;
                if (SelectedFilePath == string.Empty)
                {
                    settings = SelectedFileSettings;
                }
                else
                {
                    TemplateJsonStructure templateJsonStructure = new TemplateManager().DecryptData(SelectedFilePath);
                    settings = templateJsonStructure.DocumentSettings;
                }
                if (settings != null)
                {
                    // Страница
                    textMarginTop_TextBlock.Text = $"{settings.PageSettings.TopMargin} см";
                    textMarginBottom_TextBlock.Text = $"{settings.PageSettings.BottomMargin} см";
                    textMarginLeft_TextBlock.Text = $"{settings.PageSettings.LeftMargin} см";
                    textMarginRight_TextBlock.Text = $"{settings.PageSettings.RightMargin} см";

                    // Абзацы
                    textFontFamily.Text = settings.TextFieldSettings.FontFamily;
                    textFontSize.Text = $"{settings.TextFieldSettings.FontSize} pt";
                    textAlignment.Text = settings.TextFieldSettings.Alignment;
                    textMarginLeft.Text = $"{settings.TextFieldSettings.MarginLeft} см";
                    textMarginRight.Text = $"{settings.TextFieldSettings.MarginRight} см";
                    textMarginTop.Text = $"{settings.TextFieldSettings.MarginTop} см";
                    textMarginBottom.Text = $"{settings.TextFieldSettings.MarginBottom} см";
                    textFirstLineIndentation.Text = $"{settings.TextFieldSettings.FirstLineIndentation} см";
                    textLineSpacingType.Text = settings.TextFieldSettings.LineSpacingType;
                    textLineSpacingMultiplier.Text = $"{settings.TextFieldSettings.LineSpacingMultiplier}";

                    // Изображения
                    imageNumbering.Text = settings.ImageSettings.EnableNumbering ? "Да" : "Нет";
                    imageDescriptions.Text = settings.ImageSettings.EnableDescriptions ? "Да" : "Нет";
                    imageAlignment.Text = settings.ImageSettings.Alignment;

                    // Таблицы
                    tableNumberColumns.Text = settings.TableSettings.NumberColumns ? "Да" : "Нет";
                    tableNameTables.Text = settings.TableSettings.NameTables ? "Да" : "Нет";
                    tableColorizeHeaders.Text = settings.TableSettings.ColorizeHeaders ? "Да" : "Нет";
                    tableEnableNumbering.Text = settings.TableSettings.EnableNumbering ? "Да" : "Нет";
                    tableHeaderColor.Text = settings.TableSettings.HeaderColor;
                    tableUseDefaultParagraphSettings.Text = settings.TableSettings.UseDefaultParagraphSettings ? "Да" : "Нет";

                    tableFontFamily.Text = settings.TableSettings.FontFamily;
                    tableFontSize.Text = $"{settings.TableSettings.FontSize} pt";
                    tableParagraphAlignment.Text = settings.TableSettings.ParagraphAlignment;
                    tableMarginLeft.Text = $"{settings.TableSettings.MarginLeft} см";
                    tableMarginRight.Text = $"{settings.TableSettings.MarginRight} см";
                    tableMarginTop.Text = $"{settings.TableSettings.MarginTop} см";
                    tableMarginBottom.Text = $"{settings.TableSettings.MarginBottom} см";
                    tableFirstLineIndentation.Text = $"{settings.TableSettings.FirstLineIndentation} см";
                    tableLineSpacing.Text = settings.TableSettings.LineSpacing;
                    tableLineSpacingMultiplier.Text = $"{settings.TableSettings.LineSpacingMultiplier}";
                    tableVerticalAlignment.Text = settings.TableSettings.VerticalAlignment;
                }
                return settings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private async void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            ApplySettingsToUI(SelectedFileSettings);
            await AnimationManager.FadeOut(this, preView);
            SelectedFileSettings = new Settings();
        }
        private async void useGOSTSettings_Button_Click(object sender, RoutedEventArgs e)
        {
            await AnimationManager.FadeIn(this,preView);
            SelectedFilePath = string.Empty;
            SelectedFileSettings = GOSTSettings;
            LoadPreview();
        }

        private async void tableText_LineSpacingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tableText_LineSpacingComboBox.SelectedValue.ToString() == "Множитель")
            {
                await AnimationManager.FadeIn(this, tableText_LineSpacingSl);
            }
            else
            {
                await AnimationManager.FadeOut(this, tableText_LineSpacingSl);
            }
        }
    }
}
