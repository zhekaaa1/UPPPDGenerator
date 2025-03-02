using System;
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
        public MainWin()
        {
            InitializeComponent();
            ButtonOnMainScr.ButtonClicked += OnButtonClicked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string langCode = button.Tag as string;
            LangManager.SetLanguage(langCode);
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
            uIElement.BeginAnimation(dependencyProperty, fadeOut);
        }

        private void OnButtonClicked(ButtonOnMainScr clickedButton)
        {
            FadeOut(createb, UIElement.OpacityProperty, 0.5);
            FadeOut(fillb, UIElement.OpacityProperty, 0.5);
            // Скрываем все контейнеры
            createb.Visibility = Visibility.Collapsed;
            fillb.Visibility = Visibility.Collapsed;

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
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CreateTemplateWin createTemplateWin = new CreateTemplateWin();
            createTemplateWin.Show();
        }
    }
}
