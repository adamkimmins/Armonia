using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace Armonia.App.Services
{
    public sealed class AudioCaptureService : IDisposable
    {
        // Events
        public event EventHandler<double>? LevelChanged;   // 0..1 for bester UI meters

        public event EventHandler<string>? RecordingCompleted; //stopped
        // State
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private volatile bool _isRecording;
        private volatile bool _isPaused;
        private readonly object _lock = new();

        private string? _currentFilePath;

        // Config – tweak as you like
        public int SampleRate { get; set; } = 44100;
        public int Channels { get; set; } = 1;

        public bool IsRecording => _isRecording;
        public bool IsPaused    => _isPaused;
        public string? CurrentFilePath => _currentFilePath;

        // Start to a single WAV file
        public void StartRecording(string filePath)
        {
            StopRecording(); // safety

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            _currentFilePath = filePath;

            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(SampleRate, 16, Channels),
                BufferMilliseconds = 20 // lower latency = smoother meters
            };
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _writer = new WaveFileWriter(_currentFilePath, _waveIn.WaveFormat);
            _isPaused = false;
            _isRecording = true;

            _waveIn.StartRecording();
        }

        public void PauseRecording()
        {
            if (!_isRecording || _isPaused) return;
            _isPaused = true;
        }

        public void ResumeRecording()
        {
            if (!_isRecording || !_isPaused) return;
            _isPaused = false;
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            try
            {
                _waveIn?.StopRecording(); // triggers RecordingStopped -> cleanup
                //  RecordingCompleted?.Invoke(this, _currentFilePath!); 
            }
            catch
            {
                Cleanup(); // fallback
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            // Compute RMS (Root Mean Square) amplitude instead of instantaneous peak
            double sumSquares = 0;
            int samples = e.BytesRecorded / 2;

            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)(e.Buffer[i] | (e.Buffer[i + 1] << 8));
                double normalized = sample / 32767.0;
                sumSquares += normalized * normalized;
            }

            // RMS provides smoother and more realistic amplitude tracking
            double rms = Math.Sqrt(sumSquares / samples);

            // Optional minor scaling boost (~1.5x) to increase visual sensitivity
            double displayLevel = Math.Min(1.0, rms * 1.5);

            // Always fire LevelChanged, even if tiny, so UI gets consistent ticks
            LevelChanged?.Invoke(this, displayLevel);


            if (_isPaused) return; // << core of pause: discard buffers while paused

            lock (_lock)
            {
                _writer?.Write(e.Buffer, 0, e.BytesRecorded);
                _writer?.Flush();
            }
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (_currentFilePath != null)
                RecordingCompleted?.Invoke(this, _currentFilePath);
            Cleanup();
        }

        private void Cleanup()
        {
            _isRecording = false;
            _isPaused = false;

            try { _waveIn?.Dispose(); } catch { }
            _waveIn = null;

            try { _writer?.Dispose(); } catch { }
            _writer = null;
        }

        public void Dispose() => Cleanup();
    }
}
