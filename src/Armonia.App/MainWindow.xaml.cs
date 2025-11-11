using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Armonia.App.Views;
using Armonia.App.Services;

namespace Armonia.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        // 🧭 Shared Navigation Helper
        private async Task TryNavigateToAsync(UserControl targetPage)
        {
            // Prompt if leaving a RecordPage with unsaved changes
            if (MainFrame.Content is RecordPage recordPage)
            {
                bool proceed = await recordPage.ConfirmExitWithUnsavedChanges();
                if (!proceed)
                    return; // Cancel navigation
            }

            // Fade out current view
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            if (MainFrame.Visibility == Visibility.Visible && MainFrame.Content is FrameworkElement current)
                current.BeginAnimation(OpacityProperty, fadeOut);
            else
                MainContent.BeginAnimation(OpacityProperty, fadeOut);

            await Task.Delay(350);

            // Switch views
            MainContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Content = targetPage;

            // Fade in the new view
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            (MainFrame.Content as FrameworkElement)?.BeginAnimation(OpacityProperty, fadeIn);
        }

        // 🎵 Navigation Buttons
        private async void Songs_Click(object sender, RoutedEventArgs e)
        {
            await TryNavigateToAsync(new AudioLibraryPage());
        }

        private async void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            await TryNavigateToAsync(new RecordPage());
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            await TryNavigateToAsync(new SettingsPage());
        }

        // 🏠 Return Home
        public async void GoHome()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            (MainFrame.Content as FrameworkElement)?.BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(250);

            MainFrame.Content = null; // Clear previous UserControl
            MainFrame.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            MainContent.BeginAnimation(OpacityProperty, fadeIn);
        }

    
        private bool _startupSkipped = false;
        private bool _startupRunning = false;

        //bind to user settings TODO
        private bool _showStartupScreens = true;

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = SettingsService.Load();

            // for if user disabled splash/startup screens, skip straight to main content
            if (!settings.ShowStartupScreens)
            {
                SplashOverlay.Visibility = Visibility.Collapsed;
                StarterContent.Visibility = Visibility.Collapsed;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1000));
                MainContent.BeginAnimation(OpacityProperty, fadeIn);
                WindowState = WindowState.Maximized;
                return;
            }

            // run normal startup animation
            await Task.Delay(3000);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100));
            fadeOut.Completed += async (_, _) =>
            {
                SplashOverlay.Visibility = Visibility.Collapsed;

                await Task.Delay(200);
                ((TranslateTransform)StarterContent.RenderTransform).Y = -80;

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(290));
                var slideIn = new DoubleAnimation(-30, 0, TimeSpan.FromMilliseconds(500));
                StarterContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                ((TranslateTransform)StarterContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);

                WindowState = WindowState.Maximized;
                await Task.Delay(1000);

                var fadeOutStarter = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(290));
                var slideDown = new DoubleAnimation(0, 80, TimeSpan.FromMilliseconds(700));
                StarterContent.BeginAnimation(UIElement.OpacityProperty, fadeOutStarter);
                ((TranslateTransform)StarterContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideDown);
                await Task.Delay(500);

                fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(900));
                MainContent.BeginAnimation(OpacityProperty, fadeIn);
            };

            SplashOverlay.BeginAnimation(OpacityProperty, fadeOut);
        }


        private void SkipStartupAnimations()
        {
            if (!_startupRunning || _startupSkipped)
                return;

            _startupSkipped = true;
            _startupRunning = false;

            // Instantly show main content
            SplashOverlay.Visibility = Visibility.Collapsed;
            StarterContent.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
            MainContent.Opacity = 1;
            MainContent.IsEnabled = true;

            // Optional: maximize window immediately
            WindowState = WindowState.Maximized;
        }


        // 🧠 Save prompt logic for navigation
        private async Task<bool> ConfirmNavigationIfUnsavedAsync()
        {
            if (MainFrame.Content is RecordPage recordPage)
                return await recordPage.ConfirmExitWithUnsavedChanges();

            return true; // Not a RecordPage — safe to navigate
        }

        // 💾 Prompt before app close
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (MainFrame.Content is RecordPage recordPage && recordPage.HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    recordPage.SaveCurrentProject();
                }
            }

            base.OnClosing(e);
        }
    }
}
