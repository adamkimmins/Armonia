using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Armonia.App.Services
{
    public class AudioCaptureService : IDisposable
    {
        private WasapiCapture? _capture;
        private WaveFileWriter? _writer;

        public event EventHandler<float>? LevelChanged;

        public void StartRecording(string outputPath)
        {
            var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            _capture = new WasapiCapture(device);
            _writer  = new WaveFileWriter(outputPath, _capture.WaveFormat);

            _capture.DataAvailable += (s, a) =>
            {
                _writer.Write(a.Buffer, 0, a.BytesRecorded);
                float level = BitConverter.ToInt16(a.Buffer, 0) / 32768f;
                LevelChanged?.Invoke(this, Math.Abs(level));
            };

            _capture.RecordingStopped += (s, a) =>
            {
                _writer?.Dispose();
                _capture?.Dispose();
            };

            _capture.StartRecording();
        }

        public void StopRecording() => _capture?.StopRecording();

        public void Dispose()
        {
            _writer?.Dispose();
            _capture?.Dispose();
        }
    }
}
