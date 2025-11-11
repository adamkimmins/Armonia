using System;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Armonia.App.Services
{
    public class ProjectService
    {
        private static readonly string RootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Armonia", "projects");

        public static void EnsureStructure()
        {
            Directory.CreateDirectory(RootPath);
        }

        public static string CreateProject(string projectName)
        {
            EnsureStructure();

            string projectPath = Path.Combine(RootPath, projectName);
            string audioPath = Path.Combine(projectPath, "audio");
            string lyricsPath = Path.Combine(projectPath, "lyrics");

            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(audioPath);
            Directory.CreateDirectory(lyricsPath);

            // Create metadata
            var metadata = new
            {
                Name = projectName,
                Created = DateTime.Now,
                LastModified = DateTime.Now
            };

            string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(projectPath, "project.json"), metadataJson);

            return projectPath;
        }

        public static string[] GetProjects()
        {
            EnsureStructure();
            return Directory.GetDirectories(RootPath);
        }

        public static string GetProjectPath(string projectName)
        {
            return Path.Combine(RootPath, projectName);
        }

        public static void SaveLyrics(string projectName, string lyricsText)
        {
            string projectPath = GetProjectPath(projectName);
            string lyricsDir = Path.Combine(projectPath, "lyrics");
            Directory.CreateDirectory(lyricsDir);

            string filePath = Path.Combine(lyricsDir, "lyrics.txt");
            File.WriteAllText(filePath, lyricsText);
        }

        public static string LoadLyrics(string projectName)
        {
            string filePath = Path.Combine(GetProjectPath(projectName), "lyrics", "lyrics.txt");
            return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
        }

        // ----------
        // projects
        // --------------
        public static void SaveProject(string projectName, string audioFilePath, string lyricsText)
        {
            EnsureStructure();
            string projectPath = GetProjectPath(projectName);

            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(Path.Combine(projectPath, "audio"));
            Directory.CreateDirectory(Path.Combine(projectPath, "lyrics"));

            // Copy audio file into /audio/
            if (File.Exists(audioFilePath))
            {
                string destAudio = Path.Combine(projectPath, "audio", Path.GetFileName(audioFilePath));
                File.Copy(audioFilePath, destAudio, overwrite: true);
            }

            // Write lyrics to /lyrics/lyrics.txt
            string lyricsPath = Path.Combine(projectPath, "lyrics", "lyrics.txt");
            File.WriteAllText(lyricsPath, lyricsText);

            // Update project.json
            string metaPath = Path.Combine(projectPath, "project.json");
            var metadata = new
            {
                Name = projectName,
                LastModified = DateTime.Now
            };
            string json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(metaPath, json);
        }

        public static (string audioPath, string lyricsText) LoadProject(string projectName)
        {
            string projectPath = GetProjectPath(projectName);
            string audioDir = Path.Combine(projectPath, "audio");
            string lyricsPath = Path.Combine(projectPath, "lyrics", "lyrics.txt");

            string latestAudio = Directory.Exists(audioDir)
                ? Directory.GetFiles(audioDir, "*.wav").OrderByDescending(File.GetCreationTime).FirstOrDefault() ?? ""
                : "";

            string lyricsText = File.Exists(lyricsPath)
                ? File.ReadAllText(lyricsPath)
                : "";

            return (latestAudio, lyricsText);
        }

    }
}
