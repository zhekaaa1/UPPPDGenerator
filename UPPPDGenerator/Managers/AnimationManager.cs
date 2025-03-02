using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace UPPPDGenerator.Managers
{
    public static class AnimationManager
    {
        public static async Task FadeOutGrid(Window window, Grid animatedGrid)
        {
            // Создаем клон Storyboard-а, чтобы избежать повторного использования
            Storyboard sb = ((Storyboard)window.FindResource("FadeOutAnimation")).Clone();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // Отписка от события после завершения
            EventHandler handler = null;
            handler = (s, e) =>
            {
                tcs.TrySetResult(true);
                sb.Completed -= handler;
            };
            sb.Completed += handler;

            sb.Begin(animatedGrid);
            await tcs.Task;

            animatedGrid.Visibility = Visibility.Collapsed;
        }

        public static async Task FadeInGrid(Window window, Grid animatedGrid)
        {
            animatedGrid.Opacity = 0;
            animatedGrid.Visibility = Visibility.Visible;

            Storyboard sb = ((Storyboard)window.FindResource("FadeInAnimation")).Clone();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            EventHandler handler = null;
            handler = (s, e) =>
            {
                tcs.TrySetResult(true);
                sb.Completed -= handler;
            };
            sb.Completed += handler;

            sb.Begin(animatedGrid);
            await tcs.Task;
        }
    }
}
