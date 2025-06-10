using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using UPPPDGenerator.DocumentSettings;
using UPPPDGenerator.Elements;
using UPPPDGenerator.Managers;

namespace UPPPDGenerator.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateTemplateWin.xaml
    /// </summary>
    public partial class CreateTemplateWin : Window
    {
        private TemplateJsonStructure TemplateJsonStructure = new TemplateJsonStructure();
        private TemplateAccessMode TemplateAccessMode = new TemplateAccessMode();
        private readonly List<EnumDTO> Strings = new List<EnumDTO>();
        public class EnumDTO
        {
            public string OneValue { get; set; }
        }
        public CreateTemplateWin(TemplateJsonStructure template, TemplateAccessMode templateAccessMode)
        {
            InitializeComponent();
            TemplateJsonStructure = template;
            TemplateAccessMode = templateAccessMode;
            templateName.Text = template.TemplateName;
            ValuesAreEmptyTextBlock.Visibility = Visibility.Visible;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWin mainWin = new MainWin();
            mainWin.Show();
            Close();
        }
        private void ElementTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                if (item != null)
                {
                    string tag = item.Tag as string;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        DataObject data = new DataObject("elementType", tag);
                        DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
                    }
                }
            }
        }

        private TreeViewItem GetNearestContainer(UIElement element)
        {
            while (element != null && !(element is TreeViewItem))
                element = VisualTreeHelper.GetParent(element) as UIElement;

            return element as TreeViewItem;
        }
        private void TitlePagePanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("elementType"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent("reorder"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void TitlePagePanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("reorder"))
            {
                UIElement movedElement = e.Data.GetData("reorder") as UIElement;
                if (movedElement == null)
                    return;
                Point position = e.GetPosition(TitlePagePanel);
                int insertIndex = TitlePagePanel.Children.Count;

                for (int i = 0; i < TitlePagePanel.Children.Count; i++)
                {
                    UIElement child = TitlePagePanel.Children[i];
                    if (child == movedElement)
                        continue;

                    Point childPos = child.TranslatePoint(new Point(0, 0), TitlePagePanel);
                    if (position.Y < childPos.Y + child.RenderSize.Height / 2)
                    {
                        insertIndex = i;
                        break;
                    }
                }
                TitlePagePanel.Children.Remove(movedElement);
                if (insertIndex > TitlePagePanel.Children.Count)
                    insertIndex = TitlePagePanel.Children.Count;
                TitlePagePanel.Children.Insert(insertIndex, movedElement);
            }
            else if (e.Data.GetDataPresent("elementType"))
            {
                string tag = e.Data.GetData("elementType") as string;
                UIElement newElement = null;
                switch (tag)
                {
                    case "defaultpara":
                        newElement = CreateDefaultParagraph("Пример обычного текста");
                        break;
                    case "emptypara":
                        newElement = CreateEmptyParagraph();
                        break;
                    case "fieldtofill":
                        newElement = CreateDataField("Название", "Данные: %data%");
                        break;
                    case "separator":
                        newElement = CreateSeparator("Solid", 2, "#FFFFFF");
                        break;
                }

                if (newElement != null)
                {
                    newElement.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;

                    Point position = e.GetPosition(TitlePagePanel);
                    int insertIndex = TitlePagePanel.Children.Count;

                    for (int i = 0; i < TitlePagePanel.Children.Count; i++)
                    {
                        UIElement child = TitlePagePanel.Children[i];
                        Point childPos = child.TranslatePoint(new Point(0, 0), TitlePagePanel);
                        if (position.Y < childPos.Y + child.RenderSize.Height / 2)
                        {
                            insertIndex = i;
                            break;
                        }
                    }
                    LogicalElements.Add((TitlePageElement)((FrameworkElement)newElement).Tag);
                    newElement.MouseRightButtonDown += Element_Click;
                    TitlePagePanel.Children.Insert(insertIndex, newElement);
                }
            }
            InsertionMarker.Visibility = Visibility.Collapsed;
            _draggedElement = null;
        }
        private void ElementTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && !(obj is TreeViewItem))
                obj = VisualTreeHelper.GetParent(obj);

            if (obj is TreeViewItem item && item.Tag is string tag)
            {
                UIElement newElement = null;
                switch (tag)
                {
                    case "defaultpara":
                        newElement = CreateDefaultParagraph("Пример обычного текста");
                        break;
                    case "emptypara":
                        newElement = CreateEmptyParagraph();
                        break;
                    case "fieldtofill":
                        newElement = CreateDataField("Название", "Данные: %data%");
                        break;
                    case "separator":
                        newElement = CreateSeparator("Solid", 2, "#FFFFFF");
                        break;
                }
                if (newElement == null)
                    return;
                newElement.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
                LogicalElements.Add((TitlePageElement)((FrameworkElement)newElement).Tag);
                newElement.MouseRightButtonDown += Element_Click;
                TitlePagePanel.Children.Add(newElement);
            }
        }


        private void TitlePagePanel_DragOver(object sender, DragEventArgs e)
        {
            // Определяем, что именно перетаскивается
            if (e.Data.GetDataPresent("reorder"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent("elementType"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            e.Handled = true;

            Point position = e.GetPosition(TitlePagePanel);
            UIElement draggedElement = e.Data.GetData("reorder") as UIElement;

            UIElement nearest = null;
            double smallestDistance = double.MaxValue;

            foreach (UIElement child in TitlePagePanel.Children)
            {
                if (child == draggedElement) continue; // исключаем сам себя

                Point childPos = child.TranslatePoint(new Point(0, 0), TitlePagePanel);
                double dist = Math.Abs(position.Y - (childPos.Y + child.RenderSize.Height / 2));
                if (dist < smallestDistance)
                {
                    nearest = child;
                    smallestDistance = dist;
                }
            }

            double insertY;

            if (nearest != null)
            {
                Point nearestPos = nearest.TranslatePoint(new Point(0, 0), TitlePagePanel);
                insertY = position.Y < nearestPos.Y + nearest.RenderSize.Height / 2
                    ? nearestPos.Y
                    : nearestPos.Y + nearest.RenderSize.Height;
            }
            else
            {
                insertY = TitlePagePanel.ActualHeight;
            }

            InsertionMarker.Margin = new Thickness(0, insertY, 0, 0);
            InsertionMarker.Visibility = Visibility.Visible;
        }
        private UIElement _draggedElement;
        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggedElement = sender as UIElement;
            if (_draggedElement != null)
            {
                DragDrop.DoDragDrop(_draggedElement, new DataObject("reorder", _draggedElement), DragDropEffects.Move);
            }
        }
        private UIElement CreateDefaultParagraph(string content)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(89, 89, 89)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(10),
                Child = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = content,
                            FontSize = 16,
                            Foreground = Brushes.White,
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                },
                Tag = new TitlePageElement()
                {
                    ElementType = ElementType.DefaultParagraph,
                    ElementProperties = new DefaultFieldProperties()
                    {
                        Content = content,
                        TextFieldSettings = new TextFieldSettings()
                        {
                            FontFamily = "Times New Roman",
                            FontSize = 14,
                            FirstLineIndentation = 1.5,
                            Alignment = "Center",
                            MarginRight = 0,
                            MarginBottom = 0,
                            MarginLeft = 0,
                            MarginTop = 0,
                            LineSpacingType = "OneAndHalf",
                            LineSpacingMultiplier = 1,
                        }
                    }
                }
            };
        }
        private UIElement CreateEmptyParagraph()
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(89, 89, 89)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(10),
                Child = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "", // Пустой текст
                            FontSize = 14,
                            Foreground = Brushes.White
                        }
                    }
                },
                Tag = new TitlePageElement
                {
                    ElementType = ElementType.EmptyParagraph,
                    ElementProperties = null
                }
            };
        }
        private UIElement CreateDataField(string name, string content)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(89, 89, 89)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(10),
                Child = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock { Text = "[", Foreground = Brushes.HotPink, FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                                new TextBlock { Text = name, Foreground = Brushes.HotPink, FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                                new TextBlock { Text = "]", Foreground = Brushes.HotPink, FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                                new TextBlock { Text = content.Replace("%data%", ""), Foreground = Brushes.White, FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                                new TextBlock { Text = "заполняется", Background = Brushes.LightGray, Foreground = Brushes.Black, Padding = new Thickness(3), FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                                new TextBlock { Text = "", Foreground = Brushes.White, FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                            }
                        }
                    }
                },
                Tag = new TitlePageElement
                {
                    ElementType = ElementType.DataField,
                    ElementProperties = new DataFieldProperties()
                    {
                        Name = name,
                        DataElements = new List<string>(),
                        Content = content,
                        ContentType = UPPPDGenerator.Elements.ContentType.String,
                        TextProperties = new TextFieldSettings()
                        {
                            FontFamily = "Times New Roman",
                            FontSize = 14,
                            FirstLineIndentation = 1.5,
                            Alignment = "Center",
                            MarginRight = 0,
                            MarginBottom = 0,
                            MarginLeft = 0,
                            MarginTop = 0,
                            LineSpacingType = "OneAndHalf",
                            LineSpacingMultiplier = 1,
                        }
                    }
                }
            };
        }
        private UIElement CreateSeparator(string lineType, int lineHeight, string colorHex)
        {
            return new Border
            {
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex)),
                Height = lineHeight * 2,
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                Tag = new TitlePageElement()
                {
                    ElementType = ElementType.Separator,
                    ElementProperties = new SeparatorProperties()
                    {
                        LineColor = colorHex,
                        LineType = LineType.Solid,
                        LineHeight = lineHeight
                    }
                }
            };
        }
        private void E1LineSpacingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((E1LineSpacingComboBox.SelectedItem as ComboBoxItem).Tag.ToString() == "multiply")
                E1LineSpacingMultiplierSl.Visibility = Visibility.Visible;
            else
                E1LineSpacingMultiplierSl.Visibility = Visibility.Collapsed;
        }

        private async void ContentPlace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ContentPlace.SelectedIndex)
            {
                case 0:
                    await AnimationManager.FadeOut(this, IfDataBefore);
                    await AnimationManager.FadeOut(this, IfInsideContent);
                    await AnimationManager.FadeIn(this, IfContentBefore);
                    break;
                case 1:
                    await AnimationManager.FadeOut(this, IfContentBefore);
                    await AnimationManager.FadeOut(this, IfInsideContent);
                    await AnimationManager.FadeIn(this, IfDataBefore);
                    break;
                case 2:
                    await AnimationManager.FadeOut(this, IfContentBefore);
                    await AnimationManager.FadeOut(this, IfDataBefore);
                    await AnimationManager.FadeIn(this, IfInsideContent);
                    break;
                case 3:
                    await AnimationManager.FadeOut(this, IfContentBefore);
                    await AnimationManager.FadeOut(this, IfDataBefore);
                    await AnimationManager.FadeOut(this, IfInsideContent);
                    break;
            }
        }

        private async void ContentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ContentType.SelectedIndex)
            {
                case 0:
                    await AnimationManager.FadeOut(this, AddEnumsSl);
                    break;
                case 1:
                    await AnimationManager.FadeOut(this, AddEnumsSl);
                    break;
                case 2:
                    await AnimationManager.FadeIn(this, AddEnumsSl);
                    break;
            }
        }
        private void AddEnumsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(E3EnumText.Text.Trim()))
            {
                ErrorContainer.Show("Ошибка: заполните поле \"Значение\"", Elements.ErrorType.Warning);
                return;
            }
            Strings.Add(new EnumDTO() { OneValue = E3EnumText.Text.Trim() });
            EnumsValuesListView.ItemsSource = Strings;
            ValuesAreEmptyTextBlock.Visibility = Visibility.Collapsed;
        }

        private void EnumsValuesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnumsValuesListView.SelectedItem is EnumDTO enumDTO)
            {
                Strings.Remove(enumDTO);
                EnumsValuesListView.SelectedItem = null;
                EnumsValuesListView.ItemsSource = Strings;
                if (Strings.Count == 0)
                ValuesAreEmptyTextBlock.Visibility = Visibility.Visible;
            }
            else return;
        }

        private void E3LineSpacingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((E3LineSpacingComboBox.SelectedItem as ComboBoxItem).Tag.ToString() == "multiply")
                E3LineSpacingMultiplierSl.Visibility = Visibility.Visible;
            else
                E3LineSpacingMultiplierSl.Visibility = Visibility.Collapsed;
        }
        private List<TitlePageElement> LogicalElements = new List<TitlePageElement>();
        private List<TitlePageElement> CollectLogicalElements()
        {
            var elements = new List<TitlePageElement>();
            foreach (UIElement child in TitlePagePanel.Children)
            {
                if (child is FrameworkElement fe && fe.Tag is TitlePageElement el)
                {
                    elements.Add(el);
                }
            }
            return elements;
        }
        private int ClicksCount = 0;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<TitlePageElement> elements = CollectLogicalElements();
            if (elements.Count == 0 && ClicksCount == 0)
            {
                ErrorContainer.Show("Вы уверены, что не хотите добавить ни одного элемента? Титульный лист останется пустым. Кликните еще раз, чтобы продолжить.", Elements.ErrorType.Warning);
                ClicksCount++;
            }
            else if (elements.Count == 0 && ClicksCount == 1)
            {
                new CreateWithoutTitleTemplateWin(TemplateJsonStructure, TemplateAccessMode).Show();
                Close();
            }
            else if (elements.Count > 0)
            {
                TemplateJsonStructure.TitlePageElements = elements;
                new CreateWithoutTitleTemplateWin(TemplateJsonStructure, TemplateAccessMode).Show();
                Close();
            }
        }
        private Border _currentlySelectedElementBorder = null;
        private async void Element_Click(object sender, MouseButtonEventArgs e)
        {
            var element = (sender as FrameworkElement)?.Tag as TitlePageElement;
            if (element == null) return;
            if (_currentlySelectedElementBorder != null)
            {
                _currentlySelectedElementBorder.BorderBrush = Brushes.Transparent;
                _currentlySelectedElementBorder.BorderThickness = new Thickness(0);
            }
            var selectedBorder = sender as Border;
            _currentlySelectedElementBorder = selectedBorder;
            await AnimationManager.FadeOut(this, DefaultParagraphPropertyPanel);
            await AnimationManager.FadeOut(this, DataPropertyPanel);
            await AnimationManager.FadeOut(this, SeparatorPropertyPanel);
            await AnimationManager.FadeIn(this, GeneralPropertyPanel);
            (sender as Border).BorderBrush = Brushes.DeepSkyBlue;
            (sender as Border).BorderThickness = new Thickness(2);
            switch (element.ElementType)
            {
                case ElementType.DefaultParagraph:
                    await AnimationManager.FadeIn(this, SaveButton);
                    ShowDefaultParagraphProperties(element);
                    break;
                case ElementType.DataField:
                    await AnimationManager.FadeIn(this, SaveButton);
                    ShowDataFieldProperties(element);
                    break;
                case ElementType.Separator:
                    await AnimationManager.FadeIn(this, SaveButton);
                    ShowSeparatorProperties(element);
                    break;
                case ElementType.EmptyParagraph:
                    await AnimationManager.FadeOut(this, DefaultParagraphPropertyPanel);
                    await AnimationManager.FadeOut(this, DataPropertyPanel);
                    await AnimationManager.FadeOut(this, SeparatorPropertyPanel);
                    await AnimationManager.FadeOut(this, SaveButton);
                    break;
            }
        }
        private void DeleteElement()
        {
            if (!(_currentlySelectedElementBorder is Border border)) return;
            if (!(border.Tag is TitlePageElement element)) return;
            if (TitlePagePanel.Children.Contains(border))
            {
                TitlePagePanel.Children.Remove(border);
                _currentlySelectedElementBorder = null;
            }
        }
        private async void ShowDefaultParagraphProperties(TitlePageElement element)
        {
            if (element.ElementType != ElementType.DefaultParagraph)
                return;
            await AnimationManager.FadeIn(this, DefaultParagraphPropertyPanel);
            var props = element.ElementProperties as DefaultFieldProperties;
            if (props == null)
                return;
            var textSettings = props.TextFieldSettings;
            E1TitleTextBox.Text = props.Content;
            E1FontFamilyComboBox.SelectedItem = GetComboBoxItemByContent(E1FontFamilyComboBox, textSettings.FontFamily);
            FontSizeComboBox.SelectedItem = GetComboBoxItemByContent(FontSizeComboBox, textSettings.FontSize.ToString());
            E1AlignmentComboBox.SelectedItem = GetComboBoxItemByContent(E1AlignmentComboBox, textSettings.Alignment);
            E1MarginLeftTextBox.Text = textSettings.MarginLeft.ToString();
            E1MarginRightTextBox.Text = textSettings.MarginRight.ToString();
            E1MarginTopTextBox.Text = textSettings.MarginTop.ToString();
            E1MarginBottomTextBox.Text = textSettings.MarginBottom.ToString();
            E1FirstLineIndentationTextBox.Text = textSettings.FirstLineIndentation.ToString();
            E1LineSpacingComboBox.SelectedItem = GetComboBoxItemByContent(E1LineSpacingComboBox, GetLineSpacingDisplayName(textSettings.LineSpacingType));
            E1LineSpacingMultiplierTextBox.Text = textSettings.LineSpacingMultiplier.ToString();
            E1LineSpacingMultiplierSl.Visibility = textSettings.LineSpacingType == "multiply" ? Visibility.Visible : Visibility.Collapsed;
        }

        private ComboBoxItem GetComboBoxItemByContent(ComboBox comboBox, string content)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if ((item.Content as string)?.Equals(content, StringComparison.OrdinalIgnoreCase) == true)
                    return item;
            }
            return null;
        }
        private string GetLineSpacingDisplayName(string spacingType)
        {
            switch (spacingType.ToLower())
            {
                case "1": return "Одинарный";
                case "1.5": return "Полуторный";
                case "2": return "Двойной";
                case "multiply": return "Множитель";
                default: return "Одинарный";
            }
        }
        private async void ShowDataFieldProperties(TitlePageElement element)
        {
            var props = element.ElementProperties as DataFieldProperties;
            if (props == null) return;
            await AnimationManager.FadeIn(this, DataPropertyPanel);
            E2NameTextBox.Text = props.Name;
            string[] strings = props.Content.Split( new[] { "%data%" },StringSplitOptions.None);
            if (strings.Count() > 1)
            {
                if (strings[0].Length == 0)
                {
                    IfContentBefore.Visibility = Visibility.Visible;
                    TextAfterData.Text = strings[1];
                }
                else if (strings[1].Length == 0)
                {
                    IfDataBefore.Visibility = Visibility.Visible;
                    TextBeforeData.Text = strings[0];
                }
                else if (strings[0].Length != 0 && strings[1].Length != 0)
                {
                    IfInsideContent.Visibility = Visibility.Visible;
                    TextBefore.Text = strings[0];
                    TextAfter.Text = strings[1];
                }
                else
                {
                    IfContentBefore.Visibility = Visibility.Collapsed;
                    IfDataBefore.Visibility = Visibility.Collapsed;
                    IfInsideContent.Visibility = Visibility.Collapsed;
                }
            }

            switch (props.ContentType)
            {
                case UPPPDGenerator.Elements.ContentType.Int:
                    ContentType.SelectedIndex = 0;
                    break;
                case UPPPDGenerator.Elements.ContentType.String:
                    ContentType.SelectedIndex = 1;
                    break;
                case UPPPDGenerator.Elements.ContentType.Enumerable:
                    ContentType.SelectedIndex = 2;
                    AddEnumsSl.Visibility = Visibility.Visible;
                    EnumsValuesListView.ItemsSource = props.DataElements.Select(x => new { OneValue = x });
                    break;
            }

            var text = props.TextProperties;
            E3FontFamilyComboBox.SelectedItem = FindComboBoxItem(E3FontFamilyComboBox, text.FontFamily);
            E3FontSizeComboBox.SelectedItem = FindComboBoxItem(E3FontSizeComboBox, text.FontSize.ToString());
            E3AlignmentComboBox.SelectedIndex = AlignmentToIndex(text.Alignment);
            E3MarginLeftTextBox.Text = text.MarginLeft.ToString();
            E3MarginRightTextBox.Text = text.MarginRight.ToString();
            E3MarginTopTextBox.Text = text.MarginTop.ToString();
            E3MarginBottomTextBox.Text = text.MarginBottom.ToString();
            E3FirstLineIndentationTextBox.Text = text.FirstLineIndentation.ToString();

            if (text.LineSpacingType == "multiply")
            {
                E3LineSpacingComboBox.SelectedIndex = 3;
                E3LineSpacingMultiplierSl.Visibility = Visibility.Visible;
                E3LineSpacingMultiplierTextBox.Text = text.LineSpacingMultiplier.ToString();
            }
            else
            {
                int index = 0;
                switch (text.LineSpacingType)
                {
                    case "1":
                        index = 0;
                        break;
                    case "1.5":
                        index = 1;
                        break;
                    case "2":
                        index = 2;
                        break;
                    default:
                        index = 0;
                        break;
                }
                E3LineSpacingComboBox.SelectedIndex = index;
                E3LineSpacingMultiplierSl.Visibility = Visibility.Collapsed;
            }
        }
        private ComboBoxItem FindComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if ((item.Content as string)?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)
                    return item;
            }
            return null;
        }

        private int AlignmentToIndex(string alignment)
        {
            switch (alignment)
            {
                case "По левому краю": return 0;
                case "По правому краю": return 1;
                case "По центру": return 2;
                case "По ширине": return 3;
            };
            return 0;
        }



        private async void ShowSeparatorProperties(TitlePageElement element)
        {
            var props = element.ElementProperties as SeparatorProperties;
            if (props == null) return;

            await AnimationManager.FadeIn(this, SeparatorPropertyPanel);

            switch (props.LineType)
            {
                case LineType.Solid:
                    SeparatorLineTypeComboBox.SelectedIndex = 0;
                    break;
                case LineType.Double:
                    SeparatorLineTypeComboBox.SelectedIndex = 1;
                    break;
                case LineType.Bold:
                    SeparatorLineTypeComboBox.SelectedIndex = 2;
                    break;
                case LineType.Dotted:
                    SeparatorLineTypeComboBox.SelectedIndex = 3;
                    break;
                case LineType.Wavy:
                    SeparatorLineTypeComboBox.SelectedIndex = 4;
                    break;
            }

            switch (props.LineColor.ToLower())
            {
                case "#000000":
                    SeparatorColorComboBox.SelectedIndex = 0;
                    break;
                case "#ff0000":
                    SeparatorColorComboBox.SelectedIndex = 1;
                    break;
                case "#008000":
                    SeparatorColorComboBox.SelectedIndex = 2;
                    break;
                default:
                    SeparatorColorComboBox.SelectedIndex = 0;
                    break;
            }
            int index = (props.LineHeight > 5 || props.LineHeight < 1) ? 0 : props.LineHeight - 1;
            SeparatorLineHeightComboBox.SelectedIndex = index;
        }








        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(_currentlySelectedElementBorder?.Tag is TitlePageElement element))
                return;
            switch ((_currentlySelectedElementBorder.Tag as TitlePageElement).ElementType)
            {
                case ElementType.DefaultParagraph:
                    SaveDefaultParagraphSettings(_currentlySelectedElementBorder);
                    break;
                case ElementType.DataField:
                    SaveDataFieldSettings(_currentlySelectedElementBorder);
                    break;
                case ElementType.Separator:
                    SaveSeparatorSettings(_currentlySelectedElementBorder);
                    break;
            }
        }
        private void SaveSeparatorSettings(UIElement visualElement)
        {
            var element = (visualElement as FrameworkElement)?.Tag as TitlePageElement;
            if (element == null || element.ElementType != ElementType.Separator)
                return;

            var props = element.ElementProperties as SeparatorProperties;
            if (props == null) return;

            props.LineType = (LineType)SeparatorLineTypeComboBox.SelectedIndex; // Индексы соответствуют enum
            props.LineHeight = SeparatorLineHeightComboBox.SelectedIndex + 1;

            switch (SeparatorColorComboBox.SelectedIndex)
            {
                case 0: props.LineColor = "#000000"; break;
                case 1: props.LineColor = "#ff0000"; break;
                case 2: props.LineColor = "#008000"; break;
            }
        }








        private void SaveDefaultParagraphSettings(Border visual)
        {
            if (!(visual.Tag is TitlePageElement element)
                || element.ElementType != ElementType.DefaultParagraph
                || !(element.ElementProperties is DefaultFieldProperties props))
                return;

            if (string.IsNullOrEmpty(E1TitleTextBox.Text))
            {
                ErrorContainer.Show("Заполните текст. Он не должен быть пустым.",Elements.ErrorType.Critical);
                return;
            }
            props.Content = E1TitleTextBox.Text;

            var txt = props.TextFieldSettings;
            txt.FontFamily = (E1FontFamilyComboBox.SelectedItem as ComboBoxItem)?.Content as string ?? txt.FontFamily;
            txt.FontSize = int.TryParse((FontSizeComboBox.SelectedItem as ComboBoxItem)?.Content as string, out var fs) ? fs : txt.FontSize;

            var alignItem = E1AlignmentComboBox.SelectedItem as ComboBoxItem;
            string alignmentText = (alignItem?.Content as string);
            switch (alignmentText)
            {
                case "По левому краю":
                    txt.Alignment = "Left";
                    break;
                case "По правому краю":
                    txt.Alignment = "Right";
                    break;
                case "По центру":
                    txt.Alignment = "Center";
                    break;
                case "По ширине":
                    txt.Alignment = "Justify";
                    break;
                default:
                    break;
            }

            txt.MarginLeft = double.TryParse(E1MarginLeftTextBox.Text, out var ml) ? ml : txt.MarginLeft;
            txt.MarginRight = double.TryParse(E1MarginRightTextBox.Text, out var mr) ? mr : txt.MarginRight;
            txt.MarginTop = double.TryParse(E1MarginTopTextBox.Text, out var mt) ? mt : txt.MarginTop;
            txt.MarginBottom = double.TryParse(E1MarginBottomTextBox.Text, out var mb) ? mb : txt.MarginBottom;

            txt.FirstLineIndentation = double.TryParse(E1FirstLineIndentationTextBox.Text, out var fi) ? fi : txt.FirstLineIndentation;

            var lineItem = E1LineSpacingComboBox.SelectedItem as ComboBoxItem;
            var tag = lineItem?.Tag as string;
            if (tag == "multiply")
            {
                txt.LineSpacingType = "multiply";
                txt.LineSpacingMultiplier = double.TryParse(E1LineSpacingMultiplierTextBox.Text, out var mul) ? mul : txt.LineSpacingMultiplier;
            }
            else if (double.TryParse(tag, out var mult))
            {
                txt.LineSpacingType = mult.ToString();
                txt.LineSpacingMultiplier = mult;
            }
            else
            {
                txt.LineSpacingType = "1";
                txt.LineSpacingMultiplier = 1;
            }
            UpdateDefaultParagraphUi(visual);
        }
        private void UpdateDefaultParagraphUi(Border visual)
        {
            if (visual == null) return;

            if (!(visual.Tag is TitlePageElement element)) return;
            if (element.ElementType != ElementType.DefaultParagraph) return;

            if (!(element.ElementProperties is DefaultFieldProperties props)) return;

            var stackPanel = visual.Child as StackPanel;
            if (stackPanel == null || stackPanel.Children.Count == 0) return;

            if (stackPanel.Children[0] is TextBlock tb)
            {
                tb.Text = props.Content;

                var settings = props.TextFieldSettings;
                tb.Foreground = Brushes.White;
                tb.TextWrapping = TextWrapping.Wrap;

                switch (settings.Alignment?.ToLower())
                {
                    case "center":
                        tb.TextAlignment = TextAlignment.Center;
                        break;
                    case "right":
                        tb.TextAlignment = TextAlignment.Right;
                        break;
                    default:
                        tb.TextAlignment = TextAlignment.Left;
                        break;
                }
            }
        }

        private void SaveDataFieldSettings(Border visual)
        {
            // Получаем модель из Tag
            if (!(visual.Tag is TitlePageElement element)
                || element.ElementType != ElementType.DataField
                || !(element.ElementProperties is DataFieldProperties props))
                return;

            // Сохраняем имя и содержимое
            props.Name = E2NameTextBox.Text;
            if (string.IsNullOrEmpty(E2NameTextBox.Text))
            {
                ErrorContainer.Show("Заполните название данных. Они будут отображаться при заполнении.", Elements.ErrorType.Critical);
                return;
            }

            switch (ContentPlace.SelectedIndex)
            {
                case 0:
                    props.Content = TextBeforeData.Text + "%data%";
                    break;
                case 1:
                    props.Content = "%data%" + TextAfterData.Text;
                    break;
                case 2: 
                    props.Content = TextBefore.Text + "%data%" + TextAfter.Text;
                    break;
                case 3: 
                    props.Content = "%data%"; 
                    break;
            }

            switch (ContentType.SelectedIndex)
            {
                case 0:
                    props.ContentType = UPPPDGenerator.Elements.ContentType.Int;
                    break;
                case 1:
                    props.ContentType = UPPPDGenerator.Elements.ContentType.String;
                    break;
                case 2:
                    props.ContentType = UPPPDGenerator.Elements.ContentType.Enumerable;
                    break;
                default:
                    break;
            }

            if (props.ContentType == UPPPDGenerator.Elements.ContentType.Enumerable)
            {
                props.DataElements = EnumsValuesListView.Items
                    .OfType<dynamic>()
                    .Select(x => (string)x.OneValue)
                    .ToList();
            }
            else
            {
                props.DataElements.Clear();
            }

            var txt = props.TextProperties;
            txt.FontFamily = (E3FontFamilyComboBox.SelectedItem as ComboBoxItem)?.Content as string ?? txt.FontFamily;
            txt.FontSize = int.TryParse((E3FontSizeComboBox.SelectedItem as ComboBoxItem)?.Content as string, out var fs) ? fs : txt.FontSize;

            txt.Alignment = (E3AlignmentComboBox.SelectedItem as ComboBoxItem)?.Content as string ?? txt.Alignment;

            txt.MarginLeft = double.TryParse(E3MarginLeftTextBox.Text, out var ml) ? ml : txt.MarginLeft;
            txt.MarginRight = double.TryParse(E3MarginRightTextBox.Text, out var mr) ? mr : txt.MarginRight;
            txt.MarginTop = double.TryParse(E3MarginTopTextBox.Text, out var mt) ? mt : txt.MarginTop;
            txt.MarginBottom = double.TryParse(E3MarginBottomTextBox.Text, out var mb) ? mb : txt.MarginBottom;
            txt.FirstLineIndentation = double.TryParse(E3FirstLineIndentationTextBox.Text, out var fi) ? fi : txt.FirstLineIndentation;

            var lineItem = E3LineSpacingComboBox.SelectedItem as ComboBoxItem;
            var tag = lineItem?.Tag as string;
            if (tag == "multiply")
            {
                txt.LineSpacingType = "multiply";
                txt.LineSpacingMultiplier = double.TryParse(E3LineSpacingMultiplierTextBox.Text, out var mul) ? mul : txt.LineSpacingMultiplier;
            }
            else if (double.TryParse(tag, out var mult))
            {
                txt.LineSpacingType = mult.ToString();
                txt.LineSpacingMultiplier = mult;
            }
            else
            {
                txt.LineSpacingType = "1";
                txt.LineSpacingMultiplier = 1;
            }
            UpdateDataFieldUi(visual);

        }
        private void UpdateDataFieldUi(Border visual)
        {
            if (visual == null) return;

            if (!(visual.Tag is TitlePageElement titlePageElement)) return;
            if (!(titlePageElement.ElementProperties is DataFieldProperties props)) return;

            var stackPanel = visual.Child as StackPanel;
            if (stackPanel == null || stackPanel.Children.Count == 0) return;

            var innerStackPanel = stackPanel.Children[0] as StackPanel;
            if (innerStackPanel == null) return;

            if (innerStackPanel.Children.Count < 5) return;
            if (innerStackPanel.Children[1] is TextBlock nameTb)
            {
                nameTb.Text = props.Name;
            }

            string beforeText = "";
            string afterText = "";
            int dataIndex = ContentPlace.SelectedIndex;

            if (dataIndex == 0)
            {
                beforeText = TextAfterData.Text;
                afterText = "";
            }
            else if (dataIndex == 1)
            {
                afterText = TextBeforeData.Text;
                beforeText = "";
            }
            else if (dataIndex == 2)
            {
                beforeText = TextAfter.Text;
                afterText = TextBefore.Text;
            }
            else
            {
                beforeText = "";
                afterText = "";
            }            
            if (innerStackPanel.Children[3] is TextBlock tbBefore)
                tbBefore.Text = afterText;
            if (innerStackPanel.Children[4] is TextBlock tbData)
                tbData.Text = "заполняется";
            if (innerStackPanel.Children[5] is TextBlock tbAfter)
                tbAfter.Text = beforeText;

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedElement();
        }
        private void DeleteSelectedElement()
        {
            if (!(_currentlySelectedElementBorder is Border border)) return;
            if (!(border.Tag is TitlePageElement element)) return;
            if (TitlePagePanel.Children.Contains(border))
            {
                TitlePagePanel.Children.Remove(border);
                _currentlySelectedElementBorder = null;
            }
        }
    }
}
