using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;

namespace Armonia.App.Controls
{
    public partial class TimelineControl : UserControl
    {
        public event EventHandler<double>? SeekRequested;

        private readonly DispatcherTimer _timer;
        private Stopwatch _stopwatch = new();
        private bool _isPlaying = false;
        

        public static readonly DependencyProperty BpmProperty =
            DependencyProperty.Register(nameof(Bpm), typeof(int), typeof(TimelineControl),
                new PropertyMetadata(120, (_, __) => { }));

        public int Bpm { get => (int)GetValue(BpmProperty); set => SetValue(BpmProperty, value); }

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(TimelineControl),
                new PropertyMetadata(1d, (_, __) => { }));

        public double Zoom { get => (double)GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }

        public TimelineControl()
        {
            InitializeComponent();
            Loaded += (_, __) => Draw();
            SizeChanged += (_, __) => Draw();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            _timer.Tick += Timer_Tick;
        }

        // Time between beats in seconds
        private double SecondsPerBeat => 60.0 / Bpm;

        // Width per beat in pixels (Zoom acts as multiplier)
        private double PixelsPerBeat => 100 * Zoom;

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_isPlaying) return;

            double elapsed = _stopwatch.Elapsed.TotalSeconds;
            double beatPos = elapsed / SecondsPerBeat;
            double pixelX = beatPos * PixelsPerBeat;

            // if (pixelX > ActualWidth - 100)
            //     _scrollOffset = pixelX - (ActualWidth - 100);

            UpdatePlayhead(pixelX);
            UpdateTimeDisplay(elapsed);
        }

        private void UpdateTimeDisplay(double seconds)
        {
            int mins = (int)(seconds / 60);
            double secs = seconds % 60;
            TimeText.Text = $"{mins:0}:{secs:00.00}";
        }

        private void UpdatePlayhead(double x)
        {
            Canvas.SetLeft(SeekThumb, x);
        }

        private void Draw()
        {
            if (Layer == null) return;
            Layer.Children.Clear();

            double totalWidth = ActualWidth > 0 ? ActualWidth : 1200;
            int totalBeats = (int)(totalWidth / PixelsPerBeat);

            for (int i = 0; i < totalBeats; i++)
            {
                double x = i * PixelsPerBeat;

                // Beat line
                var line = new Rectangle
                {
                    Width = 1,
                    Height = 30,
                    Fill = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0))
                };
                Canvas.SetLeft(line, x);
                Canvas.SetTop(line, 0);
                Layer.Children.Add(line);

                // Beat label
                var beatLabel = new TextBlock
                {
                    Text = ((i % 4) + 1).ToString(),
                    FontSize = 12,
                    Margin = new Thickness(0, 30, 0, 0),
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(beatLabel, x + 2);
                Layer.Children.Add(beatLabel);
            }
        }

        // === Playback Control API ===
        public void Start()
        {
            _isPlaying = true;
            // _scrollOffset = 0;
            _stopwatch.Restart();
            _timer.Start();
        }

        public void Stop()
        {
            _isPlaying = false;
            _stopwatch.Reset();
            // _scrollOffset = 0;
            _timer.Stop();
            UpdateTimeDisplay(0);
            UpdatePlayhead(0);
        }
    }
}

// //OLD
// using System;
// using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Controls.Primitives;
// using System.Windows.Media;
// using System.Windows.Shapes;

// namespace Armonia.App.Controls
// {
//     public partial class TimelineControl : UserControl
//     {
//         public event EventHandler<double>? SeekRequested;

//         public static readonly DependencyProperty BpmProperty =
//             DependencyProperty.Register(nameof(Bpm), typeof(int), typeof(TimelineControl),
//                 new PropertyMetadata(120, (_, __) => { }));

//         public int Bpm { get => (int)GetValue(BpmProperty); set => SetValue(BpmProperty, value); }

//         public static readonly DependencyProperty ZoomProperty =
//             DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(TimelineControl),
//                 new PropertyMetadata(1d, (_, __) => { }));

//         public double Zoom { get => (double)GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }

//         public static readonly DependencyProperty PlayheadPixelsProperty =
//             DependencyProperty.Register(nameof(PlayheadPixels), typeof(double), typeof(TimelineControl),
//                 new PropertyMetadata(0d, (d, e) => ((TimelineControl)d).UpdateThumb()));

//         public double PlayheadPixels { get => (double)GetValue(PlayheadPixelsProperty); set => SetValue(PlayheadPixelsProperty, value); }

//         public TimelineControl()
//         {
//             InitializeComponent();
//             Loaded += (_, __) => Draw();
//             SizeChanged += (_, __) => Draw();
//         }

//         private void Draw()
//         {
//             if (Layer == null) return;
//             Layer.Children.Clear();
//             double ppb = 80 * Zoom;
//             double w = ActualWidth > 0 ? ActualWidth : 1200;
//             int beats = (int)Math.Ceiling(w / ppb);

//             for (int b = 0; b <= beats; b++)
//             {
//                 double x = b * ppb;
//                 var rect = new Rectangle
//                 {
//                     Width = 1, Height = 28,
//                     Fill = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0))
//                 };
//                 Canvas.SetLeft(rect, x);
//                 Canvas.SetTop(rect, 0);
//                 Layer.Children.Add(rect);

//                 var label = new TextBlock { Text = ((b % 4) + 1).ToString(), Margin = new Thickness(2, 28, 0, 0) };
//                 Canvas.SetLeft(label, x + 2);
//                 Layer.Children.Add(label);
//             }
//             UpdateThumb();
//         }

//         private void UpdateThumb() => Canvas.SetLeft(SeekThumb, PlayheadPixels - SeekThumb.Width / 2);

//         private void SeekThumb_DragDelta(object sender, DragDeltaEventArgs e)
//         {
//             PlayheadPixels = Math.Max(0, PlayheadPixels + e.HorizontalChange);
//             SeekRequested?.Invoke(this, PlayheadPixels);
//             UpdateThumb();
//         }
//         private void SeekThumb_DragCompleted(object sender, DragCompletedEventArgs e) { }
//     }
// }
