using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Armonia.App.Services;

namespace Armonia.App.Views
{
    public partial class RecordPage : Page
    {
        private readonly AudioCaptureService _recorder = new();
        private readonly string _outputDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Armonia");

        public RecordPage()
        {
            InitializeComponent();
            _recorder.LevelChanged += (_, level) =>
            {
                Dispatcher.Invoke(() => AudioLevelBar.Value = level);
            };
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(_outputDir);
            var filePath = Path.Combine(_outputDir, $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
            _recorder.StartRecording(filePath);
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _recorder.StopRecording();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }
}
