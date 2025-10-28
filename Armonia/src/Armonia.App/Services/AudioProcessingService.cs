using System;
using System.Linq;
using NAudio.Wave;
using System.IO;

namespace Armonia.App.Services
{
    public static class AudioProcessingService
    {
        // Example: quick normalization (placeholder logic)
        public static void NormalizeWave(string inputPath, string outputPath)
        {
            using var reader = new AudioFileReader(inputPath);
            float max = 0f;
            float[] buffer = new float[reader.WaveFormat.SampleRate];
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                max = Math.Max(max, buffer.Take(read).Max(Math.Abs));

            reader.Position = 0;
            float gain = max > 0 ? 1f / max : 1f;

            using var writer = new WaveFileWriter(outputPath, reader.WaveFormat);
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++) buffer[i] *= gain;
                writer.WriteSamples(buffer, 0, read);
            }
        }
    }
}
