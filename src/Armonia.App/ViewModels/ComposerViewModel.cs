using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Armonia.App.ViewModels
{
    public class ComposerViewModel : BindableBase
    {
        // Core bindable properties
        private int _bpm = 120;
        
        public int Bpm
        {
            get => _bpm;
            set => Set(ref _bpm, value);
        }

        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set => Set(ref _zoom, value);
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
        }

        private double _playheadX;
        public double PlayheadX
        {
            get => _playheadX;
            set => Set(ref _playheadX, value);
        }

        public ObservableCollection<TrackViewModel> Tracks { get; } = new();

        // -------- Composer Methods ----------
        private const int TrackCount = 1;
        public void InitDefaultTracks(int count = 4)
        {
            Tracks.Clear();
            string[] defaults = { "Voice", "Guitar", "Piano", "Drums" };

            for (int i = 0; i < count; i++)
            {
                string name = i < defaults.Length ? defaults[i] : $"Track {i + 1}";
                Tracks.Add(new TrackViewModel(name));
            }
            StatusText = "Tracks initialized.";
        }

        public void TransportPlay()
        {
            StatusText = "Playing";
        }

        public void TransportStop()
        {
            StatusText = "Stopped";
        }

        public void AddClipToTrack(string trackName, ClipViewModel clip)
        {
            var track = Tracks.FirstOrDefault(t => t.Name.Equals(trackName, System.StringComparison.OrdinalIgnoreCase));
            if (track == null)
            {
                track = new TrackViewModel(trackName);
                Tracks.Add(track);
            }
            track.Clips.Add(clip);
            StatusText = $"Added clip to {trackName}";
        }
    }
}
