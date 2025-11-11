using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Armonia.App.Views
{
    public partial class AudioLibraryPage : UserControl
    {
        private readonly string _rootDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Armonia");

        public AudioLibraryPage()
        {
            InitializeComponent();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            ProjectList.Items.Clear();
            if (!Directory.Exists(_rootDir))
                Directory.CreateDirectory(_rootDir);

            foreach (var dir in Directory.GetDirectories(_rootDir))
            {
                var name = Path.GetFileName(dir);
                ProjectList.Items.Add(new
                {
                    ProjectName = name,
                    CreatedDate = Directory.GetCreationTime(dir).ToShortDateString()
                });
            }
        }

        private void ProjectList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Use a safe cast instead of 'dynamic' pattern
            var selectedProject = ProjectList.SelectedItem;
            if (selectedProject == null) return;

            // Use reflection to extract the ProjectName property from the anonymous type
            var projectNameProp = selectedProject.GetType().GetProperty("ProjectName");
            string projectName = projectNameProp?.GetValue(selectedProject)?.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(projectName))
                LoadProject(projectName);
        }

        private void LoadProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string projectName)
                LoadProject(projectName);
        }

        private void LoadProject(string projectName)
        {
            string projectPath = Path.Combine(_rootDir, projectName);
            string audioDir = Path.Combine(projectPath, "audio");
            string lyricsDir = Path.Combine(projectPath, "lyrics");

            AudioFileList.Items.Clear();
            LyricsFileList.Items.Clear();

            if (Directory.Exists(audioDir))
            {
                foreach (var file in Directory.GetFiles(audioDir, "*.wav"))
                    AudioFileList.Items.Add(Path.GetFileName(file));
            }

            if (Directory.Exists(lyricsDir))
            {
                foreach (var file in Directory.GetFiles(lyricsDir, "*.txt"))
                    LyricsFileList.Items.Add(Path.GetFileName(file));
            }
        }

        private void LyricsFileList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedLyric = LyricsFileList.SelectedItem as string;
            if (selectedLyric == null)
                return;

            var selectedProject = ProjectList.SelectedItem;
            if (selectedProject == null)
                return;

            var projectNameProp = selectedProject.GetType().GetProperty("ProjectName");
            string selectedProjectName = projectNameProp?.GetValue(selectedProject)?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(selectedProjectName))
                return;

            string filePath = Path.Combine(_rootDir, selectedProjectName, "lyrics", selectedLyric);

            if (File.Exists(filePath))
                MessageBox.Show(File.ReadAllText(filePath), $"Lyrics: {selectedLyric}");
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.GoHome();
        }
    }
}




// using System;
// using System.Windows.Controls;
// using System.Threading.Tasks;
// using System.Windows;
// using System.Windows.Input;
// using System.Windows.Media;
// using System.Windows.Media.Animation;
// using Armonia.App.Views;
// using System.IO;
// using Armonia.App.Services;

// namespace Armonia.App.Views
// {
//     public partial class AudioLibraryPage
//     {
//         public AudioLibraryPage()
//         {
//             InitializeComponent();
//             this.Loaded += (_, _) =>
//             {
//                 var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
//                 this.BeginAnimation(Page.OpacityProperty, fadeIn);
//             };
//         }

//         private void Back_Click(object sender, RoutedEventArgs e)
//         {
//             (Application.Current.MainWindow as MainWindow)?.GoHome();
//         }
//     }
// }
