using System;
using System.Collections.Generic;

namespace Armonia.App.Models
{
    public class ProjectData
    {
        public string ProjectName { get; set; } = "Untitled";
        public string CreatedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string Lyrics { get; set; } = "";
        public List<string> ImportedAudioPaths { get; set; } = new();
        public string RecordedFilePath { get; set; } = "";
    }
}
