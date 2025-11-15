using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using Microsoft.Win32;
using Armonia.App.Services;
using Armonia.App.ViewModels;
using IOPath = System.IO.Path;
using Windows.Media.Capture.Core;
using Windows.Graphics.Printing.PrintTicket; // TODO

namespace Armonia.App.Views
{
    public partial class RecordPage : UserControl
    {
        //----------------
        // Fields
        //----------------
        private readonly AudioCaptureService _recorder = new();
        private bool _isRecording = false;
        private bool _isPaused = false;
        private bool _hasRecording = false;
        private bool _isFinalizing = false; 
        private string _currentFilePath = "";

        // Toolbar & unsaved
        private bool _toolbarVisible = false;
        private bool _unsavedChanges = false;

        // 🎵 Waveform
        private readonly List<double> _levels = new(); //_audioData
        private readonly DispatcherTimer _waveformTimer;
        private bool _hasHitThreshold = false; // true = scroll
        private int _capacityBars = 0;      
        private const double UPDATE_INTERVAL = 0.03; //used for framerate and speed
        private const double BAR_SPACING = 4.0; //space between output lines
        private const double SCROLL_RATIO = 0.67; //place where scroll starts
        private const double CANVAS_OFFSET = 40.0;
        private double _cursorX = 0; //triangle??? IDK Todo
        private double _latestLevel = 0.0; // latest level from capture
        private double _dynamicGain = 1.0; //Dyamic Gain??? Ho Dimenticato ma penso "fluffy"
        private double MaxForwardX => WaveformCanvas.ActualWidth * SCROLL_RATIO;
        private double _pendingLevel = 0.0;

        //Waveform Ticks
        private readonly List<Line> _timeTicks = new();
        private readonly List<TextBlock> _timeLabels = new();
        private double _secondsElapsed = 0;
        private double _tickSpacing = 0;   // dynamically set from PIXELS_PER_SECOND
        private double PIXELS_PER_SECOND = 50;
        private int _lastDrawnSecond = -1;
        private readonly List<(double X, int Sec)> _timeMarks = new();
        private const double TICK_HEIGHT = 10.0;
        private const double LABEL_OFFSET_Y = 4.0;
        private readonly Stopwatch _recordingClock = new();
        //For pushing to Composer
        public ComposerViewModel SharedComposerVM { get; } = new();
        
        //TESTING TODO
        private const double COMPOSER_LEFT_STOP = 100.0;


        //----------------
        // Constructor
        //----------------
        public RecordPage()
        {
            InitializeComponent();

            //composer
            // ComposerSection.ViewModel = SharedComposerVM;

            Loaded += (_, _) =>
            {
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                BeginAnimation(OpacityProperty, fadeIn);

                //composer
                //TESTING TODO
                // ClampComposerPosition();
                //  SharedComposerVM.InitDefaultTracks(4);                           
            };

            _recorder.LevelChanged += OnLevelChanged;

            _waveformTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(UPDATE_INTERVAL) // ~33fps??? I still dont know how 60ms is 66fps
            };
            _waveformTimer.Tick += UpdateWaveform;

            _recorder.LevelChanged += (_, level) =>
            {
                // simple smoothing + adaptive gain
                var raw = Math.Clamp(level, 0.0, 1.0);
                _latestLevel = (_latestLevel * 0.85) + (raw * 1.00); //test// prev: _latestLevel * 0.85 ; raw * 0.15

                // 1.0 = raw, <1 = softer, >1 = louder
                _dynamicGain = 3.0;

                //TESTING TODO
                // SizeChanged += (s, e) => ClampComposerPosition();
            };

            //for pushing to composer
            _recorder.RecordingCompleted += OnRecordingFinished;
        }

        //----------------
        // Real-Time Data Logic
        //----------------
        private void OnLevelChanged(object? sender, double level)
        {
            Interlocked.Exchange(ref _pendingLevel, level);
        }


        private async void UpdateWaveform(object? sender, EventArgs e)
        {
            if (!_isRecording || _isPaused) return;

            double width = WaveformCanvas.ActualWidth;
            double height = WaveformCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            // Get most recent sample
            double newLevel = Interlocked.Exchange(ref _pendingLevel, 0.0);
            double level = Math.Clamp((_latestLevel * _dynamicGain) + newLevel * 0.5, 0.0, 1.0);

            if (_capacityBars == 0)
                _capacityBars = Math.Max(1, (int)Math.Floor((MaxForwardX + CANVAS_OFFSET) / BAR_SPACING));

            // Forward fill / scroll logic
            if (!_hasHitThreshold)
            {
                _levels.Add(level);
                _cursorX = (_levels.Count * BAR_SPACING) - CANVAS_OFFSET; //lead offset 1

                if (_cursorX >= MaxForwardX)
                {
                    _cursorX = MaxForwardX;
                    _hasHitThreshold = true;
                }
            }
            else
            {
                if (_levels.Count >= _capacityBars)
                    _levels.RemoveAt(0);

                _levels.Add(level);
                _cursorX = MaxForwardX;
            }

            // Draw bars
            WaveformCanvas.Children.Clear();
            double centerY = height / 2.0;

            for (int i = 0; i < _levels.Count; i++)
            {
                double sample = _levels[i];
                double barH = sample * height * 0.45;
                double x = (i * BAR_SPACING) - CANVAS_OFFSET; //lead offset 2

                if (x < -CANVAS_OFFSET - BAR_SPACING)
                    continue;

                var bar = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = centerY - barH,
                    Y2 = centerY + barH,
                    StrokeThickness = 2,
                    Stroke = CreatePBRGoldBrush(sample)
                };
                WaveformCanvas.Children.Add(bar);

                var coreStripe = new Line
                {
                    X1 = bar.X1,
                    X2 = bar.X2,
                    Y1 = bar.Y1,
                    Y2 = bar.Y2,
                    StrokeThickness = 0.7,
                    Stroke = Brushes.Black,
                    Opacity = 0.35
                };
                WaveformCanvas.Children.Add(coreStripe);
            }
            //--------------------------
            //--------         ------------
            //-----     TICKS     --------------
            //--------         ------------
            //--------------------------
            double elapsed = _recordingClock.Elapsed.TotalSeconds;
            int currentSecond = (int)Math.Floor(elapsed);

            // Add a new tick mark once per second (only once)
            if (_timeMarks.Count == 0 || currentSecond > _timeMarks[^1].Sec)
            {
                double tickX = !_hasHitThreshold
                    ? (_levels.Count * BAR_SPACING) - CANVAS_OFFSET
                    : MaxForwardX;

                _timeMarks.Add((tickX, currentSecond));
            }

            // Scroll ticks left if we’ve hit the scroll threshold
            if (_hasHitThreshold)
            {
                for (int i = 0; i < _timeMarks.Count; i++)
                    _timeMarks[i] = (_timeMarks[i].X - BAR_SPACING, _timeMarks[i].Sec);

                // remove off-screen ticks
                _timeMarks.RemoveAll(t => t.X < -10 - CANVAS_OFFSET);
            }

            // Draw ticks and labels
            foreach (var (x, sec) in _timeMarks)
            {
                // Tick line
                var tick = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = height + (CANVAS_OFFSET / 3.9) - TICK_HEIGHT,
                    Y2 = height + (CANVAS_OFFSET / 3.9),
                    Stroke = Brushes.Goldenrod,
                    StrokeThickness = 1.0,
                    Opacity = 0.7
                };
                WaveformCanvas.Children.Add(tick);

                // Label below tick
                var label = new TextBlock
                {
                    Text = TimeSpan.FromSeconds(sec).ToString(@"mm\:ss"),
                    Foreground = Brushes.LightGoldenrodYellow,
                    FontSize = 10,
                    FontFamily = new FontFamily("Consolas"),
                    Opacity = 0.9
                };
                Canvas.SetLeft(label, x - 14);
                Canvas.SetTop(label, height - (TICK_HEIGHT + 14 - (CANVAS_OFFSET / 1.1)));
                WaveformCanvas.Children.Add(label);
            }


            // --- CURSOR MOVEMENT CONTROL ---
            if (!_hasHitThreshold)
            {
                _cursorX += BAR_SPACING + CANVAS_OFFSET - 2;

                // when it hit or exceed threshold, snap once and mark it as anchored
                if (_cursorX >= MaxForwardX + CANVAS_OFFSET - 2)
                {
                    _cursorX = MaxForwardX + CANVAS_OFFSET - 2;
                    _hasHitThreshold = true;
                }
            }
            else
            {
                // anchored: remain steady
                _cursorX = MaxForwardX + CANVAS_OFFSET - 2;
            }

            // Draw cursor + line
            Canvas.SetLeft(CursorArrow, _cursorX);

            var cursorLine = new Line
            {
                X1 = _cursorX - CANVAS_OFFSET,
                X2 = _cursorX - CANVAS_OFFSET,
                Y1 = 0,
                Y2 = height + CANVAS_OFFSET,
                Stroke = Brushes.Gray,
                StrokeThickness = 0.7,
                Opacity = 0.45
            };
            WaveformCanvas.Children.Add(cursorLine);
        }

        //----------------
        // GOLDDDDDD
        // ----------------
        private Brush CreatePBRGoldBrush(double intensity)
        {
            // intensity = normalized height (0–1)
            // StackOverflow lied this feature sucks
            double brightBoost = 0.5 + (intensity * 0.5);

            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };

            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(130, 64, 34), 0.00)); // deep red
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(220, 160, 30), 0.10)); // warm gold
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(242, 229, 194), 0.22)); // bright gold
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(150, 100, 40), 0.45)); // inner reflection (dark)
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb((byte)(242 * brightBoost), (byte)(229 * brightBoost), (byte)(194 * brightBoost)), 0.65)); // mirror bright
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(148, 94, 40), 0.80)); // hot orange/red tone
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(240, 190, 60), 1.00)); // golden base tail

            gradient.Freeze(); // optimization
            return gradient;
        }

        //----------------
        // Recording Logic
        //----------------
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording && !_hasRecording)
                StartRecording();
            else if (_isRecording && !_isPaused)
                PauseRecording();
            else if (_hasRecording)
                ResetRecording();
            else if (_isPaused)
                ResumeRecording();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
                StopRecording();
            else if (_isFinalizing)
            {
                MessageBox.Show("LOL boy.");
            }
            else if (_hasRecording)
            {
                PushToComposer();
            }
        }

        private void StartRecording()
        {
            ResetWaveform();
            //Ticks
            _secondsElapsed = 0;
            _lastDrawnSecond = -1;
            _timeTicks.Clear();
            _timeLabels.Clear();
            _tickSpacing = PIXELS_PER_SECOND;

            _recordingClock.Restart();
            _timeMarks.Clear();
            //end Ticks

            Directory.CreateDirectory(IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Armonia"));
                _currentFilePath = IOPath.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Armonia", $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

            _recorder.StartRecording(_currentFilePath);

            _isRecording = true;
            _isPaused = false;
            _hasRecording = false;
            _hasHitThreshold = false;

            _capacityBars = 0; 
            _cursorX = 0;

            _waveformTimer.Start();

            LeftButton.Content = "⏸";
            LeftButton.Style = (Style)FindResource("PauseButtonStyle");
            // LeftButton.Foreground = Brushes.Black;
            // LeftButton.FontSize = 26;
            RightButton.Foreground = Brushes.Black;
            RightButton.IsEnabled = true;
        }

        private void StopRecording()
        {
            _recorder.StopRecording();
            _isRecording = false;
            _hasRecording = true;
            _isFinalizing = true; 
            _waveformTimer.Stop(); 
            _recordingClock.Restart();

            LeftButton.Content = "🔁";
            LeftButton.Style = (Style)FindResource("ReplayButtonStyle");

            RightButton.Content = "➡";
            RightButton.Style = (Style)FindResource("PushButtonStyle");
        }

        private void PauseRecording()
        {
            _recorder.PauseRecording();
            _isPaused = true;
            _waveformTimer.Stop();
            _recordingClock.Stop();

            LeftButton.Content = " ▶️";
            LeftButton.Style = (Style)FindResource("RecordButtonStyle");
            // LeftButton.Foreground = Brushes.Red;
            // LeftButton.FontSize = 27;
        }

        private void ResumeRecording()
        {
            _recorder.ResumeRecording();
            _isPaused = false;
            _waveformTimer.Start();
            _recordingClock.Start();

            LeftButton.Content = "⏸";
            LeftButton.Style = (Style)FindResource("PauseButtonStyle");
            // LeftButton.Foreground = Brushes.Black;
            // LeftButton.FontSize = 30;
        }

        private void ResetWaveform()
        {
            _levels.Clear();
            WaveformCanvas.Children.Clear();
            _cursorX = 0;
            _hasHitThreshold = false;
            _capacityBars = 0;
        }

        private void ResetRecording()
        {
            if (File.Exists(_currentFilePath)) //Doesnt delete Files properly TODO
                File.Delete(_currentFilePath);
            _isRecording = false;
            _isPaused = false;
            _hasRecording = false;
            ResetWaveform();
            _waveformTimer.Stop();

            LeftButton.Content = "⏺";
            LeftButton.Style = (Style)FindResource("RecordButtonStyle");

            RightButton.Content = "⏹";
            RightButton.Style = (Style)FindResource("StopButtonStyle");
            RightButton.Foreground = Brushes.Gray;
            RightButton.IsEnabled = false;
        }
        private async void PushToComposer()
        {
            if (!_hasRecording || !File.Exists(_currentFilePath))
            {
                MessageBox.Show("No finished recording.");
                return;
            }

            string normalizedPath = IOPath.Combine(
                IOPath.GetDirectoryName(_currentFilePath)!,
                IOPath.GetFileNameWithoutExtension(_currentFilePath) + "_cutenormalized.wav");

            // AudioProcessingService.NormalizeWave(_currentFilePath, normalizedPath);

            await Task.Run(() =>
            {
                AudioProcessingService.NormalizeWave(_currentFilePath, normalizedPath);
            });

            var clip = new ClipViewModel
            {
                Name = IOPath.GetFileNameWithoutExtension(normalizedPath),
                BeatsLength = 8
            };

            // send into composer
            SharedComposerVM.AddClipToTrack("Input", clip);

            ResetRecording(); // reset UI
        }

        private void OnRecordingFinished(object? sender, string filePath)
        {
            _isFinalizing = false;
            _hasRecording = true;
            _currentFilePath = filePath;
        }



        //----------------
        // Toolbar Logic
        //----------------
        private async void ToggleToolbar_Click(object sender, RoutedEventArgs e)
        {
            _toolbarVisible = !_toolbarVisible;

            if (_toolbarVisible)
            {
                Toolbar.Visibility = Visibility.Visible;

                var showAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                };

                var transform = Toolbar.RenderTransform as ScaleTransform;
                transform?.BeginAnimation(ScaleTransform.ScaleXProperty, showAnim);
                transform?.BeginAnimation(ScaleTransform.ScaleYProperty, showAnim);

                Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OutsideClick_CloseToolbar);
                Mouse.Capture(Toolbar);
            }
            else
            {
                await HideToolbar();
            }
        }

        private async Task HideToolbar()
        {
            _toolbarVisible = false;

            var hideAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            };

            var transform = Toolbar.RenderTransform as ScaleTransform;
            transform?.BeginAnimation(ScaleTransform.ScaleXProperty, hideAnim);
            transform?.BeginAnimation(ScaleTransform.ScaleYProperty, hideAnim);

            await Task.Delay(250);
            Toolbar.Visibility = Visibility.Collapsed;
        }

        private void OutsideClick_CloseToolbar(object sender, MouseButtonEventArgs e)
        {
            if (_toolbarVisible)
                _ = HideToolbar();

            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OutsideClick_CloseToolbar);
            Mouse.Capture(null);
        }

        //----------------
        // Toolbar Buttons
        //----------------
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string projectName = "UntitledProject";
                string latestAudio = _currentFilePath ?? "";

                ProjectService.SaveProject(projectName, latestAudio, LyricsEditor.Text);
                _unsavedChanges = false;

                MessageBox.Show($"Project '{projectName}' saved successfully.", "Armonia",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var projects = ProjectService.GetProjects();
                if (projects.Length == 0)
                {
                    MessageBox.Show("No projects found in your Armonia folder.", "Load Project",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new OpenFileDialog
                {
                    InitialDirectory = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Armonia", "projects"),
                    Filter = "Project JSON|project.json",
                    Title = "Select a Project File"
                };

                if (dialog.ShowDialog() == true)
                {
                    string projectName = new DirectoryInfo(IOPath.GetDirectoryName(dialog.FileName)!).Name;
                    var (audioPath, lyricsText) = ProjectService.LoadProject(projectName);

                    LyricsEditor.Text = lyricsText;

                    MessageBox.Show($"Project '{projectName}' loaded successfully.", "Armonia",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading project:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)!.MainFrame.Content = new SettingsPage();
        }

        //----------------
        // MIDI Add
        //----------------
        private void AddMidiInstrument_Click(object sender, RoutedEventArgs e)
        {
            string instrument = (MidiInstrumentSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Unknown";
            MessageBox.Show($"Added MIDI instrument '{instrument}' to Composer (placeholder).", "MIDI Setup");
        }

        //----------------
        // Unsaved Change Management
        //----------------
        public bool HasUnsavedChanges => _unsavedChanges;

        public void SaveCurrentProject()
        {
            _unsavedChanges = false;
            MessageBox.Show("Project saved successfully.", "Armonia");
        }

        public Task<bool> ConfirmExitWithUnsavedChanges()
        {
            if (!_unsavedChanges)
                return Task.FromResult(true);

            var result = MessageBox.Show(
                "You have unsaved changes.\nWould you like to save before leaving?",
                "Armonia", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            bool proceed;
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveCurrentProject();
                    proceed = true;
                    break;

                case MessageBoxResult.No:
                    proceed = true;
                    break;

                default: // Cancel or closed
                    proceed = false;
                    break;
            }

            return Task.FromResult(proceed);
        }

        //----------------
        // Lyrics Editor
        //----------------
        private void LyricsEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            _unsavedChanges = true;
        }

        // Open Lyrics File
        private void OpenLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Open Lyrics File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string text = File.ReadAllText(dialog.FileName);
                    LyricsEditor.Text = text;
                    _unsavedChanges = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open file:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Save Lyrics File
        private void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    Title = "Save Lyrics File",
                    FileName = "lyrics.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, LyricsEditor.Text);
                    _unsavedChanges = false;
                    MessageBox.Show("Lyrics saved successfully.", "Armonia");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save lyrics:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Toolbar Click (close when background clicked)
        private void Toolbar_Click(object sender, MouseButtonEventArgs e)
        {
            if (_toolbarVisible)
                _ = HideToolbar();
        }

        // ⬅ Back Button
        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            bool proceed = await ConfirmExitWithUnsavedChanges();
            if (!proceed)
                return;

            (Application.Current.MainWindow as MainWindow)?.GoHome();
        }
    }
}

