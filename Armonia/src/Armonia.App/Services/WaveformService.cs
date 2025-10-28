using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Armonia.App.Services
{
    public class WaveformService
    {
        // Generates a simple amplitude list for waveform drawing later
        public List<float> GeneratePeaks(string path, int step = 1000)
        {
            var peaks = new List<float>();
            using var reader = new AudioFileReader(path);
            float[] buffer = new float[step];
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                peaks.Add(buffer.Take(read).Select(Math.Abs).Average());
            }
            return peaks;
        }
    }
}
