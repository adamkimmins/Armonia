using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Armonia.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional delay before fade
            await Task.Delay(3000);

            // Fade out splash
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            fadeOut.Completed += async (_, _) =>
            {
                SplashOverlay.Visibility = Visibility.Collapsed;

                // Fade in starter content from above
                await Task.Delay(200);
                ((TranslateTransform)StarterContent.RenderTransform).Y = -80;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(290));
                var slideIn = new DoubleAnimation(-30, 0, TimeSpan.FromMilliseconds(500));
                StarterContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                ((TranslateTransform)StarterContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);

                //fullscreen
                WindowState = WindowState.Maximized;
                await Task.Delay(3000);

                //Get rid of Starter Content
                var fadeOutStarter = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(290));
                var slideDown = new DoubleAnimation(0, 80, TimeSpan.FromMilliseconds(700));
                StarterContent.BeginAnimation(UIElement.OpacityProperty, fadeOutStarter);
                ((TranslateTransform)StarterContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideDown);
                await Task.Delay(500);

                // Main
                fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1500));
                MainContent.BeginAnimation(OpacityProperty, fadeIn);
            };
            SplashOverlay.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}


