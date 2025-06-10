using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UPPPDGenerator.Windows.Elements
{
    /// <summary>
    /// Логика взаимодействия для ButtonOnMainScr.xaml
    /// </summary>
    public partial class ButtonOnMainScr : UserControl
    {
        public static event Action<ButtonOnMainScr> ButtonClicked;
        private static ButtonOnMainScr _activeButton;
        public ButtonOnMainScr()
        {
            InitializeComponent();
            ButtonClicked += OnOtherButtonClicked;
        }
        public string IconSource
        {
            get { return (string)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(string), typeof(ButtonOnMainScr), new PropertyMetadata(null));
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(ButtonOnMainScr), new PropertyMetadata("Кнопка"));
        public static void ResetActiveButton()
        {
            _activeButton = null;
        }
        private void AnimatedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_activeButton != this)
            {
                ButtonClicked?.Invoke(this);
                ExpandButton();
                _activeButton = this;
            }
        }
        public void ExpandButton()
        {
            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = 400,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            LinearGradientBrush gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#343434"), 0));
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#B2009A"), 1));
            AnimatedButton.Background = gradient;
            AnimatedButton.BeginAnimation(WidthProperty, widthAnimation);
        }
        public void CollapseButton()
        {
            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = 300,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            AnimatedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343434"));
            AnimatedButton.BeginAnimation(WidthProperty, widthAnimation);
        }
        private void OnOtherButtonClicked(ButtonOnMainScr sender)
        {
            if (this != sender)
            {
                CollapseButton();
            }
        }
    }
}
