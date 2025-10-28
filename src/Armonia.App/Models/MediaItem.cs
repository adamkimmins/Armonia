using System;

namespace Armonia.App.Models
{
    public class MediaItem
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Type { get; set; } = "audio";
        public double Duration { get; set; }
        public string? Tags { get; set; }
        public DateTime ImportedOn { get; set; } = DateTime.Now;
    }
}
