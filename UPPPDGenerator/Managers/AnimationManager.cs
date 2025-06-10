using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace UPPPDGenerator.Managers
{
    public static class AnimationManager
    {
        public static async Task FadeOut(Window window, FrameworkElement animatedGrid)
        {
            if (animatedGrid == null) return;
            if (animatedGrid.Visibility == Visibility.Collapsed) return;
            Storyboard sb = ((Storyboard)window.FindResource("FadeOutAnimation")).Clone();
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
            animatedGrid.Visibility = Visibility.Collapsed;
        }
        public static async Task FadeIn(Window window, FrameworkElement animatedGrid)
        {
            if (animatedGrid == null) return;
            if (animatedGrid.Visibility == Visibility.Visible) return;
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
