using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
namespace UPPPDGenerator.Windows.Elements
{
    /// <summary>
    /// Логика взаимодействия для ErrorContainerOnMainScr.xaml
    /// </summary>
    public partial class ErrorContainerOnMainScr : UserControl
    {
        private bool isShowing = false;
        public ErrorContainerOnMainScr()
        {
            InitializeComponent();
            var transform = new TranslateTransform(0, 30);
            ErrorMessagePanel.RenderTransform = transform;
            ErrorMessagePanel.RenderTransformOrigin = new Point(0.5, 1);
        }
        public async void Show(string message, ErrorType errorType, int durationMs = 4000)
        {
            if (isShowing) return;
            switch (errorType)
            {
                case ErrorType.Critical:
                    ErrorMessagePanel.Background = new SolidColorBrush(Colors.DarkRed);
                    ErrorIcon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/imgs/x-circle-fill.png"));
                    break;
                case ErrorType.Warning:
                    ErrorMessagePanel.Background = new SolidColorBrush(Colors.Orange);
                    ErrorIcon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/imgs/exclamation-circle-fill.png"));
                    break;
                case ErrorType.Info:
                    ErrorMessagePanel.Background = new SolidColorBrush(Colors.DarkSlateBlue);
                    ErrorIcon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/imgs/info-circle-fill.png"));
                    break;
            }
            isShowing = true;
            ErrorMessageText.Text = message;
            ErrorMessagePanel.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            var slideIn = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(300));
            ErrorMessagePanel.BeginAnimation(OpacityProperty, fadeIn);
            ((TranslateTransform)ErrorMessagePanel.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);
            await Task.Delay(durationMs);
            Hide();
        }
        public void Hide()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            var slideOut = new DoubleAnimation(0, 30, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) =>
            {
                ErrorMessagePanel.Visibility = Visibility.Collapsed;
                isShowing = false;
            };
            ErrorMessagePanel.BeginAnimation(OpacityProperty, fadeOut);
            ((TranslateTransform)ErrorMessagePanel.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideOut);
        }
    }
    public enum ErrorType
    {
        Critical = 0,
        Warning = -1,
        Info = -2
    }
}
