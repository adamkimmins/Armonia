using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Armonia.App.ViewModels;
using System.Collections.Specialized;

namespace Armonia.App.Controls
{
    public partial class TrackRow : UserControl
    {
        public TrackRow()
        {
            InitializeComponent();
            Loaded += (_, __) => Redraw();
        }

        private TrackViewModel? VM => DataContext as TrackViewModel;

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
        // private ViewModels.TrackViewModel? VM => DataContext as ViewModels.TrackViewModel;

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

        //Hooker
        public void HookTrack(TrackViewModel vm)
        {
            DataContext = vm;

            if (vm.Clips is INotifyCollectionChanged coll)
                coll.CollectionChanged += (_, __) => Redraw();
            
            Redraw();
        }
    }
}
