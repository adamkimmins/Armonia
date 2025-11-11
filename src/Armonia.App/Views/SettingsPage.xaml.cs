using System.Windows;
using System.Windows.Controls;
using Armonia.App.Services;

namespace Armonia.App.Views
{
    public partial class SettingsPage : UserControl
    {
        private AppSettings _settings;

        public SettingsPage()
        {
            InitializeComponent();
            _settings = SettingsService.Load();
            LoadSettingsToUI();
        }

        private void LoadSettingsToUI()
        {
            VolumeSlider.Value = _settings.MasterVolume;

            foreach (ComboBoxItem item in ThemeSelector.Items)
            {
                if (item.Content.ToString() == _settings.Theme)
                {
                    ThemeSelector.SelectedItem = item;
                    break;
                }
            }

            ShowStartupCheckBox.IsChecked = _settings.ShowStartupScreens;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.MasterVolume = VolumeSlider.Value;
            _settings.Theme = (ThemeSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Rustic";
            _settings.ShowStartupScreens = ShowStartupCheckBox.IsChecked ?? true;

            SettingsService.Save(_settings);

            MessageBox.Show("Settings saved successfully.", "Armonia", MessageBoxButton.OK);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.GoHome();
        }
    }
}
