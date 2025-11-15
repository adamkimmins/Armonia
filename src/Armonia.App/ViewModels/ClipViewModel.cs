using System;
using System.Windows.Media;

namespace Armonia.App.ViewModels
{
    public class ClipViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Audio";
        public double StartBeat { get; set; } = 0;
        public double DurationSeconds { get; set; } = 0;
        public double BeatsLength { get; set; } = 8;
        public Color Color { get; set; } = Colors.Blue;

        // Required for waveform display + playback
        public string FilePath { get; set; } = string.Empty;
        //wavefor
        public float[]? Samples { get; set; }
    }
}
