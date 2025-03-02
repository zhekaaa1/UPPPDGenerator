using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UPPPDGenerator.Elements;

namespace UPPPDGenerator.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateTemplateWin.xaml
    /// </summary>
    public partial class CreateTemplateWin : Window
    {
        private List<UIElement> elements = new List<UIElement>(); // Список всех элементов
        private UIElement selectedElement = null; // Выбранный элемент
        public CreateTemplateWin()
        {
            InitializeComponent();
            AddHandlersToTreeViewItems(ElementTreeView);
        }

        private void AddHandlersToTreeViewItems(ItemsControl parent)
        {
            foreach (var item in parent.Items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    treeViewItem.PreviewMouseLeftButtonDown += TreeViewItem_PreviewMouseLeftButtonDown;
                    if (treeViewItem.Items.Count > 0)
                    {
                        AddHandlersToTreeViewItems(treeViewItem);
                    }
                }
            }
        }
        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is string tag
                && tag != "n1" && tag != "n2" && tag != "n3" )
            {
                DragDrop.DoDragDrop(item, tag, DragDropEffects.Copy);
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                string tag = e.Data.GetData(typeof(string)) as string;
                Point dropPosition = e.GetPosition(DropCanvas); // Получаем координаты

                UIElement newElement = CreateElementFromTag(tag, dropPosition);
                if (newElement != null)
                {
                    // Добавляем элемент в контейнер
                    ElementsContainer.Children.Add(newElement);

                    // Добавляем в список элементов
                    elements.Add(newElement);

                    // Добавляем пунктирную линию
                    Line separator = new Line
                    {
                        X1 = 0,
                        X2 = DropCanvas.ActualWidth,
                        Y1 = 0,
                        Y2 = 0,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 4, 4 }
                    };
                    ElementsContainer.Children.Add(separator);
                }
            }
        }
        
        private UIElement CreateElementFromTag(string tag, Point dropPosition)
        {
            if (tag == "defaultpara")
            {
                ParagraphElement paragraphElement = new ParagraphElement();
                TextBlock textBlock = paragraphElement.ToUIElement();
                textBlock.Tag = paragraphElement; // Сохраняем модель для этого абзаца

                // Добавляем обработчик клика
                textBlock.MouseLeftButtonDown += OnElementClick;

                return textBlock;
            }
            return null;
        }

        private void OnElementClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement clickedElement)
            {
                // Снимаем выделение со всех элементов
                foreach (var element in elements)
                {
                    if (element is TextBlock tb)
                        tb.Background = Brushes.Transparent;
                    // Аналогично можно обрабатывать и другие типы
                }

                // Выбираем текущий элемент
                selectedElement = clickedElement;

                if (selectedElement is TextBlock textBlockSelected)
                {
                    textBlockSelected.Background = Brushes.LightGray;
                    // Получаем модель из Tag
                    if (textBlockSelected.Tag is ParagraphElement model)
                    {
                        // Заполняем панель свойств данными модели
                        TitleTextBox.Text = model.Text;
                        FontSizeTextBox.Text = model.FontSize.ToString();

                        int alignmentIndex = 2; // По умолчанию центр
                        switch (textBlockSelected.TextAlignment)
                        {
                            case TextAlignment.Left:
                                alignmentIndex = 0;
                                break;
                            case TextAlignment.Right:
                                alignmentIndex = 1;
                                break;
                            case TextAlignment.Center:
                                alignmentIndex = 2;
                                break;
                            case TextAlignment.Justify:
                                alignmentIndex = 3;
                                break;
                        }
                        AlignmentComboBox.SelectedIndex = alignmentIndex;
                        MarginLeftTextBox.Text = model.MarginLeft.ToString();
                        MarginRightTextBox.Text = model.MarginRight.ToString();
                        MarginTopTextBox.Text = model.MarginTop.ToString();
                        MarginBottomTextBox.Text = model.MarginBottom.ToString();
                        FirstLineIndentationTextBox.Text = model.FirstLineIndentation.ToString();
                        switch (model.LineSpacingType)
                        {
                            case "1":
                                LineSpacingComboBox.SelectedIndex = 0;
                                break;
                            case "1.5":
                                LineSpacingComboBox.SelectedIndex = 1;
                                break;
                            case "2":
                                LineSpacingComboBox.SelectedIndex = 2;
                                break;
                            case "multiply":
                                LineSpacingComboBox.SelectedIndex = 3;
                                LineSpacingMultiplierTextBox.Text = model.LineSpacingValue.ToString();
                                break;
                        }
                        if (model.LineSpacingType == "1")
                        {
                            LineSpacingComboBox.SelectedIndex = 0;
                        }
                    }
                    ShowPropertyPanel();
                }
            }
        }
        private void ShowPropertyPanel()
        {
            PropertyPanel_Paragraphs.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            PropertyPanel_Paragraphs.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                model.Text = TitleTextBox.Text;
                textBlockSelected.Text = model.Text;
            }
        }

        private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (double.TryParse(FontSizeTextBox.Text, out double newSize))
                {
                    model.FontSize = newSize;
                    textBlockSelected.FontSize = newSize;
                }
            }
        }

        private void AlignmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                switch (AlignmentComboBox.SelectedIndex)
                {
                    case 0:
                        textBlockSelected.TextAlignment = TextAlignment.Left;
                        model.Alignment = TextAlignment.Left;
                        break;
                    case 1:
                        textBlockSelected.TextAlignment = TextAlignment.Right;
                        model.Alignment = TextAlignment.Right;
                        break;
                    case 2:
                        textBlockSelected.TextAlignment = TextAlignment.Center;
                        model.Alignment = TextAlignment.Center;
                        break;
                    case 3:
                        textBlockSelected.TextAlignment = TextAlignment.Justify;
                        model.Alignment = TextAlignment.Justify;
                        break;
                }
            }
        }

        private void MarginLeftTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (double.TryParse(MarginLeftTextBox.Text, out double marginLeft))
                {
                    model.MarginLeft = marginLeft;
                    textBlockSelected.Margin = new Thickness(marginLeft, textBlockSelected.Margin.Top,textBlockSelected.Margin.Right, textBlockSelected.Margin.Bottom);
                }
            }
        }

        private void MarginRightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (double.TryParse(MarginRightTextBox.Text, out double marginRight))
                {
                    model.MarginLeft = marginRight;
                    textBlockSelected.Margin = new Thickness(textBlockSelected.Margin.Left, textBlockSelected.Margin.Top, marginRight, textBlockSelected.Margin.Bottom);
                }
            }
        }

        private void MarginTopTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (double.TryParse(MarginTopTextBox.Text, out double marginTop))
                {
                    model.MarginLeft = marginTop;
                    textBlockSelected.Margin = new Thickness(textBlockSelected.Margin.Left, marginTop, textBlockSelected.Margin.Right, textBlockSelected.Margin.Bottom);
                }
            }
        }

        private void MarginBottomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (double.TryParse(MarginBottomTextBox.Text, out double marginBottom))
                {
                    model.MarginLeft = marginBottom;
                    textBlockSelected.Margin = new Thickness(textBlockSelected.Margin.Left, textBlockSelected.Margin.Top, textBlockSelected.Margin.Right, marginBottom);
                }
            }
        }

        private void FirstLineIndentationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (double.TryParse(FirstLineIndentationTextBox.Text, out double indentation))
                {
                    model.FirstLineIndentation = indentation;
                }
            }
        }

        private void LineSpacingMultiplierTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (int.TryParse(LineSpacingMultiplierTextBox.Text, out int multiplier))
                {
                    model.LineSpacingValue = multiplier;
                }
            }
        }

        private void LineSpacingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedElement is TextBlock textBlockSelected && textBlockSelected.Tag is ParagraphElement model)
            {
                if (LineSpacingComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedTag = selectedItem.Tag.ToString();
                    model.LineSpacingType = selectedTag;

                    // Если выбран "множитель", показываем поле ввода
                    bool isMultiply = selectedTag == "multiply";
                    LineSpacingMultiplierLabel.Visibility = isMultiply ? Visibility.Visible : Visibility.Collapsed;
                    LineSpacingMultiplierTextBox.Visibility = isMultiply ? Visibility.Visible : Visibility.Collapsed;
                    LineSpacingMultiplierTextBox.Text = isMultiply ? model.LineSpacingValue.ToString() : string.Empty;
                }
            }
        }
    }
}
