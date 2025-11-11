// using System;
// using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Input;
// using System.Windows.Media;
// using System.Windows.Shapes;
// using Armonia.App.ViewModels;

// namespace Armonia.App.Controls
// {
//     public partial class TrackRow : UserControl
//     {
//         public event EventHandler? DragDropCompleted;

//         public double BeatPixels { get; set; } = 80;
//         public double Zoom { get; set; } = 1;
//         public double PlayheadX { get; set; }

//         public TrackRow()
//         {
//             InitializeComponent();
//             Loaded += (_, __) => Redraw();
//         }

//         private TrackViewModel? VM => DataContext as TrackViewModel;

//         private void Redraw()
//         {
//             if (VM == null || Lane == null) return;
//             Lane.Children.Clear();

//             foreach (var clip in VM.Clips)
//             {
//                 var rect = new Rectangle
//                 {
//                     Height = 56,
//                     Width = clip.BeatsLength * BeatPixels * Zoom,
//                     Fill = new SolidColorBrush(Colors.SteelBlue),
//                     Stroke = Brushes.Black,
//                     StrokeThickness = 1
//                 };
//                 Canvas.SetLeft(rect, clip.StartBeat * BeatPixels * Zoom);
//                 Canvas.SetTop(rect, 12);

//                 rect.MouseDown += Rect_MouseDown;
//                 rect.MouseMove += Rect_MouseMove;
//                 rect.MouseUp += Rect_MouseUp;
//                 Lane.Children.Add(rect);
//             }
//         }

//         private (Rectangle rect, ClipViewModel clip, double startX)? _dragging;

//         private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
//         {
//             if (sender is Rectangle rect && VM != null)
//             {
//                 var clip = VM.Clips[0];
//                 _dragging = (rect, clip, e.GetPosition(Lane).X);
//                 rect.CaptureMouse();
//             }
//         }

//         private void Rect_MouseMove(object sender, MouseEventArgs e)
//         {
//             if (_dragging is null || e.LeftButton != MouseButtonState.Pressed) return;
//             var (rect, clip, startX) = _dragging.Value;
//             var x = e.GetPosition(Lane).X;
//             var delta = x - startX;
//             double newLeft = Math.Max(0, clip.StartBeat * BeatPixels * Zoom + delta);
//             Canvas.SetLeft(rect, newLeft);
//         }

//         private void Rect_MouseUp(object sender, MouseButtonEventArgs e)
//         {
//             if (_dragging is null) return;
//             _dragging = null;
//             (sender as Rectangle)?.ReleaseMouseCapture();
//             DragDropCompleted?.Invoke(this, EventArgs.Empty);
//         }

//         private void Lane_MouseDown(object sender, MouseButtonEventArgs e)
//         {
//             PlayheadX = e.GetPosition(Lane).X;
//         }
//     }
// }

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Armonia.App.Controls
{
    public partial class TrackRow : UserControl
    {
        public TrackRow()
        {
            InitializeComponent();
            Loaded += (_, __) => Redraw();
        }

        // ===== Rename handling =====
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
        private ViewModels.TrackViewModel? VM => DataContext as ViewModels.TrackViewModel;

        private void Redraw()
        {
            if (VM == null || Lane == null) return;
            Lane.Children.Clear();

            // draw each clip rectangle (same idea as your current code)
            foreach (var clip in VM.Clips)
            {
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Height = 56,
                    Width  = clip.BeatsLength * 80,   // replace with your BeatPixels*Zoom from elsewhere when wired
                    Fill   = new SolidColorBrush(Colors.SteelBlue),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, clip.StartBeat * 80);
                Canvas.SetTop(rect, 12);
                Lane.Children.Add(rect);
            }
        }
    }
}
