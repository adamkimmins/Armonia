using System.Collections.ObjectModel;

namespace Armonia.App.ViewModels
{
    public class TrackViewModel
    {
        public string Name { get; set; } = "Track";
        public bool IsMuted { get; set; }
        public bool IsSolo { get; set; }
        public double Volume { get; set; } = 0.8;
        public ObservableCollection<ClipViewModel> Clips { get; } = new();

        public TrackViewModel(string name) => Name = name;
    }
}
