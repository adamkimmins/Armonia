using System;
using Armonia.App.Models;
using System.IO;

namespace Armonia.App.Services
{
    public class MediaImportService
    {
        public MediaItem ImportFile(string sourcePath, string destinationFolder)
        {
            Directory.CreateDirectory(destinationFolder);
            var destPath = Path.Combine(destinationFolder, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destPath, overwrite: true);

            return new MediaItem
            {
                FilePath = destPath,
                Type = Path.GetExtension(sourcePath).ToLower() == ".mp4" ? "video" : "audio",
                ImportedOn = DateTime.Now
            };
        }
    }
}
