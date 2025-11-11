using System;
using System.Windows.Media;

namespace Armonia.App.ViewModels
{
    public class ClipViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Audio";
        public double StartBeat { get; set; } = 0;   // where it sits on the timeline
        public double BeatsLength { get; set; } = 8; // duration in beats
        public Color Color { get; set; } = Colors.DodgerBlue;

        // later: link to audio buffer / midi data
    }
}
