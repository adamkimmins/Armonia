using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Armonia.App.ViewModels;
using System.Collections.Specialized;
using System.IO;
using Armonia.App.Services;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Armonia.App.Controls
{
    public partial class TrackRow : UserControl
    {
        public TrackRow()
        {
            InitializeComponent();
            //Load clip data TODO
            Loaded += (_, __) =>
            {
                if (DataContext is TrackViewModel)
                    Redraw();
                //info for the timeline pipeline TMNT
                _scrollViewer = FindParentScrollViewer(this);
            };
            // Loaded += (_, __) => Redraw();
        }

        private TrackViewModel? VM => DataContext as TrackViewModel;
        private Border? _dragTarget;
        private Point _dragOffset;

        public void SetScrollOffset(double offsetX)
        {
            Lane.RenderTransform = new TranslateTransform(-offsetX, 0);
        }

        private ScrollViewer? _scrollViewer;
        private DispatcherTimer? _autoScrollTimer;
        private double _autoScrollDirection = 0;

        private ScrollViewer? FindParentScrollViewer(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is ScrollViewer sv)
                    return sv;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        
        // Rename handling 
        private void EnterEdit()
        {
            NameText.Visibility = Visibility.Collapsed;
            NameEditor.Visibility = Visibility.Visible;
            NameEditor.Focus();
            NameEditor.SelectAll();
        }

        private void ExitEdit(bool commit)
        {
            if (!commit)
            {
                // discard edits by re-binding current text
                var bnd = NameText.GetBindingExpression(TextBlock.TextProperty);
                bnd?.UpdateTarget();
            }
            NameEditor.Visibility = Visibility.Collapsed;
            NameText.Visibility = Visibility.Visible;
        }

        private void NameText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) EnterEdit();
        }

        private void NameEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { ExitEdit(true); }
            else if (e.Key == Key.Escape) { ExitEdit(false); }
        }

        private void NameEditor_LostFocus(object sender, RoutedEventArgs e) => ExitEdit(true);

        // ===== Lane rendering (based on your existing pattern) =====
        // private ViewModels.TrackViewModel? VM => DataContext as ViewModels.TrackViewModel;

        private void Redraw()
        {
            //TODO
            if (Lane.ActualWidth == 0)
            {
                Lane.Loaded += (_, __) => Redraw();
                return;
            }

            Lane.Children.Clear();

            if (VM == null) return;

            Lane.Children.Clear();

            foreach (var clip in VM.Clips)
            {
                if (string.IsNullOrEmpty(clip.FilePath) || !File.Exists(clip.FilePath))
                {
                    // fallback rectangle
                    DrawRect(Lane, clip);
                    continue;
                }

                // Draw waveform
                var samples = AudioProcessingService.LoadWaveformSamples(clip.FilePath);

                if (samples == null || samples.Length == 0)
                {
                    DrawRect(Lane, clip); // fallback
                    continue;
                }

                DrawClip(Lane, samples, clip);
            }
        }

        private void DrawRect(Canvas lane, ClipViewModel clip)
        {
            var rect = new Rectangle
            {
                Width = (int)(clip.BeatsLength * 80),
                Height = 56,
                Fill = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(rect, 0);
            lane.Children.Add(rect);
        }

        private void DrawClip(Canvas lane, float[] samples, ClipViewModel clip)
        {
            // Lane width TODO will need reallocation for timeline
            double laneWidth = Lane.ActualWidth;
            double pixelsPerSecond = laneWidth / 20.0;     // 20 seconds visible
            double width = clip.DurationSeconds * pixelsPerSecond;
            double height = 70;

            // CLIP CONTAINER
            var border = new Border
            {
                Width = width + 6,
                Height = height,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 140, 50)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(0)
            };

            // Place container based on StartBeat (for DAW editing)
            Canvas.SetLeft(border, clip.StartBeat * 30);
            Canvas.SetTop(border, 0);

            lane.Children.Add(border);


            // DRAWING INSIDE
            var innerCanvas = new Canvas
            {
                Width = width,
                Height = height,
                ClipToBounds = true
            };

            border.Child = innerCanvas;
            //Dragging
            border.Tag = clip;
            border.MouseDown += Clip_MouseDown;
            border.MouseMove += Clip_MouseMove;
            border.MouseUp += Clip_MouseUp;

            // COMPRESS + CENTER WAVEFORM
            DrawWaveform(innerCanvas, samples, width, height);
        }
        private void DrawWaveform(Canvas canvas, float[] samples, double width, double height)
        {
            canvas.Children.Clear();

            int total = samples.Length;
            if (total < 10) return;

            // How many samples per pixel
            int stride = Math.Max(total / (int)width, 1);

            double center = height / 2;
            double compression = 1.7; // 0 = flat, 1 = full height 1.7 = ????

            for (int x = 0; x < (int)width; x++)
            {
                int index = x * stride;
                if (index >= total) break;

                double scaledAmp = Math.Abs(samples[index]) * height * compression;

                var line = new Line
                {
                    X1 = x, //offset padding, rather than messing with multiple margins
                    X2 = x,
                    Y1 = center - scaledAmp / 2,
                    Y2 = center + scaledAmp / 2,
                    Stroke = new SolidColorBrush(Color.FromRgb(230, 180, 50)),
                    StrokeThickness = 1
                };

                canvas.Children.Add(line);
            }
        }



        //Hooker
        public void HookTrack(TrackViewModel vm)
        {
            DataContext = vm;

            if (vm.Clips is INotifyCollectionChanged coll)
                coll.CollectionChanged += (_, __) => Redraw();
            
            Redraw();
        }

        //Dragging
        private void Clip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragTarget = sender as Border;
            _dragOffset = e.GetPosition(_dragTarget);
            _dragTarget.CaptureMouse();
        }

        private void Clip_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragTarget == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            var parent = (Canvas)_dragTarget.Parent;

            double x = e.GetPosition(parent).X - _dragOffset.X;

            // Prevent dragging into the left panel
            if (x < 0)
                x = 0;

            // Calculate how far the clip extends
            double rightEdge = x + _dragTarget.ActualWidth;

            // Expand canvas if clip goes past current width
            if (Lane.Width - rightEdge < 300)   // 300px buffer
            {
                Lane.Width += 600;         // grow by chipchunks
            }

            // Apply limit
            Canvas.SetLeft(_dragTarget, x);

            CheckAutoScroll(e); //always

            // Update clip beat position
            var clip = (ClipViewModel)_dragTarget.Tag;
            clip.StartBeat = x / 30.0;   // Adjust based on your current seconds → pixels scaling

            CheckAutoScroll(e);
            
        }

        private void CheckAutoScroll(MouseEventArgs e)
        {
            if (_scrollViewer == null)
                return;

            var pos = e.GetPosition(_scrollViewer);

            double margin = 80; // the "scroll zone"

            // NEAR LEFT EDGE → scroll left
            if (pos.X < margin)
            {
                StartAutoScroll(-1);
            }
            // NEAR RIGHT EDGE → scroll right
            else if (pos.X > _scrollViewer.ActualWidth - margin)
            {
                StartAutoScroll(1);
            }
            else
            {
                StopAutoScroll();
            }
        }

        private void StartAutoScroll(double direction)
        {
            _autoScrollDirection = direction;

            if (_autoScrollTimer == null)
            {
                _autoScrollTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(16) // 60fps ???? Why did I use 16
                };
                _autoScrollTimer.Tick += AutoScrollTick;
            }

            if (!_autoScrollTimer.IsEnabled)
                _autoScrollTimer.Start();
        }

        private void StopAutoScroll()
        {
            _autoScrollDirection = 0;
            _autoScrollTimer?.Stop();
        }

        private void AutoScrollTick(object? sender, EventArgs e)
        {
            if (_scrollViewer == null)
                return;

            double speed = 12; // pixels per tick

            _scrollViewer.ScrollToHorizontalOffset(
                _scrollViewer.HorizontalOffset + _autoScrollDirection * speed
            );

            // Expand the canvas if needed
            ExpandCanvas();
        }
        private void ExpandCanvas()
        {
            double currentRight = Lane.ActualWidth;
            double scrollRight = _scrollViewer!.HorizontalOffset + _scrollViewer.ViewportWidth;

            // If user drags past right edge, expand the lane
            if (scrollRight + 200 > currentRight)
            {
                Lane.Width = currentRight + 600; // grow by chunks
            }
        }


//Placeholder, will add check if top row, if not allow a sticky drag up to row above
        private void Clip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragTarget != null)
            {
                _dragTarget.ReleaseMouseCapture();
                _dragTarget = null;
            }
            //when stop click, u must also, stop scroll.
            StopAutoScroll();
        }
    }
}
