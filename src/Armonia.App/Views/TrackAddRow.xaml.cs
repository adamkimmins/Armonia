using System;
using System.Windows;
using System.Windows.Controls;

namespace Armonia.App.Controls
{
    public partial class TrackAddRow : UserControl
    {
        public event EventHandler? AddTrackRequested;

        public TrackAddRow()
        {
            InitializeComponent();
        }

        private void OnAddTrackClick(object sender, RoutedEventArgs e)
        {
            AddTrackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
