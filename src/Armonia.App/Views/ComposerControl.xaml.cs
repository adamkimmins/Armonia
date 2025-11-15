using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Controls;
using Armonia.App.ViewModels;
using Armonia.App.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Armonia.App.Views
{
    public partial class ComposerControl : UserControl
    {
        private readonly Dictionary<Brush, TranslateTransform> _brushTransforms = new();
        private void Timeline_SeekRequested(object sender, double pos) => ViewModel.PlayheadX = pos;

        //TESTING TODO
        private const double COMPOSER_LEFT_STOP = 65.0; // px

        public ComposerViewModel ViewModel
        {
            get => (ComposerViewModel)DataContext;
            set
            {
                DataContext = value;
            }
        }

        public ComposerControl()
        {
            InitializeComponent();
            Loaded += (_, __) =>
            {
                // Only init if a shared VM wasn't injected yet
                if (ViewModel == null)
                    ViewModel = new ComposerViewModel();

                // ViewModel.RequestTrackRowCreation += HandleAutoTrackCreation; //Pushtocomposer

                InitMetalButtons();
                InitializeTracks();
                ClampToWindowEdge();
            };
            //Testing TODO
            SizeChanged += (s, e) => ClampToWindowEdge();
        }

        public void AttachComposerVM(ComposerViewModel vm)
        {
            if (ViewModel != null)
            ViewModel.RequestTrackRowCreation -= HandleAutoTrackCreation; //f da doub

            DataContext = vm;
            vm.RequestTrackRowCreation += HandleAutoTrackCreation;
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            ViewModel.TransportPlay();
            Timeline.Start();
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            ViewModel.TransportStop();
            Timeline.Stop();
        }
        
        private void InitializeTracks()
        {
            var addRow = new TrackAddRow();
            addRow.AddTrackRequested += OnAddTrackRequested;
            TracksPanel.Children.Add(addRow);
        }

        private void OnAddTrackRequested(object? sender, EventArgs e)
        {
            // Create a new track data model
            var newTrackVM = new TrackViewModel($"Track {ViewModel.Tracks.Count + 1}");

            // Add it to the main ComposerViewModel collection (keeps DAW state consistent)
            ViewModel.Tracks.Add(newTrackVM);

            // Create a visual row and "hook" its DataContext to the model
            // var trackRow = new TrackRow
            // {
            //     DataContext = newTrackVM
            // };
            var trackRow = new TrackRow();
            trackRow.HookTrack(newTrackVM); //TODO

            // Insert above the AddTrackRow (so the "+" stays at the bottom)
            TracksPanel.Children.Insert(TracksPanel.Children.Count - 1, trackRow);
        }

        private void HandleAutoTrackCreation(TrackViewModel vm)
        {
            var row = new TrackRow();
            row.HookTrack(vm);

            TracksPanel.Children.Insert(TracksPanel.Children.Count - 1, row);
        }
            
        private Brush StopGoldBrush()
        {
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
            };

            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(90, 45, 25), 0.00));   // base shadow
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(160, 90, 30), 0.12));  // red-brown warmth
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(220, 160, 40), 0.25)); // gold mid
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(242, 229, 194), 0.35)); // specular highlight
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(150, 100, 40), 0.52)); // internal dark line
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(230, 185, 70), 0.70)); // reflection edge
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(180, 110, 35), 0.85)); // warm base return
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(100, 50, 25), 1.00));  // deep edge
            // gradient.Freeze();
            
            // var transform = new TranslateTransform(-0.25, 0);
            // gradient.RelativeTransform = transform;

            // // store transform reference for later animation
            // _brushTransforms[gradient] = transform;
            gradient.Freeze();
            return gradient;
        }
        //straighter
        private Brush PlayGoldBrush()
        {

            var gradient = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.9, 0.5), // Start point of the gradient
                Center = new Point(0.25, 0.5), // Center of the gradient ellipse
                RadiusX = 0.4,
                RadiusY = 2.0
            };
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(140, 90, 35), 0.00));
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(160, 90, 25), 0.15));
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(240, 190, 70), 0.35)); // highlight
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(210, 160, 60), 0.50));
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(242, 229, 194), 0.70));
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(160, 90, 25), 0.85));
                gradient.GradientStops.Add(new GradientStop(Color.FromRgb(140, 90, 35), 1.00));

                
            // var transform = new TranslateTransform(-0.25, 0);
            //     gradient.RelativeTransform = transform;

            //     // store transform reference for later animation
            //     _brushTransforms[gradient] = transform;
            gradient.Freeze(); // optimization
            return gradient;
        }

        private void ApplyMetal(Shape glyph)
        {
            var Play = PlayGoldBrush();
            PlayGlyph.Fill = Play;
            var Stop = StopGoldBrush();
            StopGlyph.Fill = Stop;

            // Subtle edge to add definition
            glyph.Stroke = new SolidColorBrush(Color.FromRgb(140, 90, 35));
            glyph.StrokeThickness = 1.2;

            // Drop shadow for 3D lift
            glyph.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 14,
                ShadowDepth = 3,
                Opacity = 0.55,
                Direction = 270
            };

            // Cache for smoother animation
            glyph.CacheMode = new BitmapCache();


        }

        // Call this once when the control is ready
        private void InitMetalButtons()
        {
            ApplyMetal(PlayGlyph);
            ApplyMetal(StopGlyph);
            // StartShine(PlayGlyph); //Sideline for now, playbutton broke
            // StartShine(StopGlyph); //Stop works but not worth it
        }

        //TESTING TODO

        public void SetComposerPosition(double newX)
        {
            ClampToWindowEdge();
        }

        private async void ClampToWindowEdge()
        {
            try
            {
                if (TaskbarPanel == null)
                    return;

                var window = Window.GetWindow(this);
                if (window == null)
                    return;

                // Get absolute position of your toolbar
                GeneralTransform transform = TaskbarPanel.TransformToAncestor(window);
                Point taskbarPos = transform.Transform(new Point(0, 0));

                double leftEdge = taskbarPos.X;
                double currentX = ComposerTranslateTransform.X;

                if (leftEdge < COMPOSER_LEFT_STOP)
                {
                    // Clamp to 100px boundary (move right)
                    double correction = COMPOSER_LEFT_STOP - leftEdge;

                    var anim = new DoubleAnimation
                    {
                        To = currentX + correction,
                        Duration = TimeSpan.FromMilliseconds(1030),
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                    };
                    await Task.Delay(5);
                    ComposerTranslateTransform.BeginAnimation(TranslateTransform.XProperty, anim);
                }
                else if (currentX > 0)
                {
                    // Reverse (move back toward default when leaving boundary)
                    var anim = new DoubleAnimation
                    {
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(1500),
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                    };
                    await Task.Delay(5);
                    ComposerTranslateTransform.BeginAnimation(TranslateTransform.XProperty, anim);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clamp error: {ex.Message}");
            }

        }

    }
}