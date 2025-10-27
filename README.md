# A.R.M.O.N.I.A.
Advanced Recording &amp; Media Organizer for Natural Integrated Audio, shortened to Armonia, is a music composition envirnoment designed to offer a better balance for people attempting to compose music. WIP

---
# Path to creation >> Documentation >> Journal

This is my first real solo project where I dig low into the system to use something like a mic and speaker. To do this, I'm going to use a WinUI structure and create a proper executable, in an attempt to create an industry level product (for free) that is unique and useful. This also means I can integrate my designs I've worked with, and be able to display more than just systems.

---
To start with the design:
---
I create my splash screen and "logo" which is just going to be earthy. I'm going to use a wavey blue and black "A" for my icon, and the full name "Armonia" as my screen. Armonia will include a banjo, guitar, mics, drums, and a maybe a violin in the mountains. Will probably apply an oil filter to a picture I took in the Tetons.

---
Next I create my file structure(should've maybe done this first):
---
<img width="319" height="798" alt="image" src="https://github.com/user-attachments/assets/1d10a646-6389-458e-88ed-11d92ce803a2" />

---
As you can see from the image, this project is going to be built on C# and XML(xaml). Felt it was easier to become better at C# than it was to create a UI framework with C++. And XMLs, or XAMLs in this case are pretty simple and straightforward. I use the XAML to declare my logic in C#. Unfortunate downside with using C# is I'm going to use .NET(Downside???) to run the program.

---
I'll break down the important notes for my stub files, not everything for the reason of space and time. Learned quite a lot of new information regarding mic import and converting to .wav files

app.xaml
Entry
*public partial class App : Application*
partial for multiple refs, this file is for UI, the other file
app.xaml.cs
is for logic.
MainWindow.xaml is my main user interface
Moving forward, AudioCaptureService.cs is my way of capturing audio, I use .NETs framework NAudio, that wraps wasapi. It converts mic input to .wav files. Using WasapiCapture then allows me to use .net framework to easily record audio, thanks to NAudio for their documentation-
https://github.com/naudio/NAudio

*_writer  = new WaveFileWriter(outputPath, _capture.WaveFormat);*
I use WaveFileWriter to write to a new .wav file using the format specified by capture device, then use _capture.WaveFormat to define the format(sample rate, channels, bit depth). Basically streams the PCM data into a placeholder .wav. Now we're getting somewhere, like 5% of the way somewhere. But now we have the hard part. The live data pump

*_capture.DataAvailable += (s, a) =>
{
    _writer.Write(a.Buffer, 0, a.BytesRecorded);
    float level = BitConverter.ToInt16(a.Buffer, 0) / 32768f;
    LevelChanged?.Invoke(this, Math.Abs(level));
};*

Took a bit of research but ultimately found a pretty useful explanation on stack overflow about subscribing the lambda to event,
using _capture.DataAvailable I fire an event by NAudio every 10ms(or default), sending to AudioDataEventArgs. The hard part was the bit converter, which I ended up finding the answer from stack overflow. This is a vital part of a modern audio interface so I didn't want to just scratch it. Its working, but not as fast as I want so I will revisit once I have the general app running smoothly.
