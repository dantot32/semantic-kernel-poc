using NAudio.Wave;
using Spectre.Console;
using System.Diagnostics;

namespace Gpt4oMini;

internal sealed class NAudioHelper
{
    public static bool RecordAudio(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            var waveFormat = new WaveFormat(44100, 24, 2);
            var waveIn = new WaveInEvent();
            waveIn.WaveFormat = waveFormat; 
            var waveFile = new WaveFileWriter(fullPath, waveIn.WaveFormat);

            waveIn.DataAvailable += (s, e) => waveFile.Write(e.Buffer, 0, e.BytesRecorded);

            waveIn.RecordingStopped += (s, e) =>
            {
                waveFile.Dispose();
                waveIn.Dispose();
            };

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            waveIn.StartRecording();
            AnsiConsole.MarkupLine($"[bold green]Registrando ... [/]");

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Modifiers == ConsoleModifiers.Alt && keyInfo.Key == ConsoleKey.R)
                {
                    waveIn.StopRecording();
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    AnsiConsole.MarkupLine($"[bold green]Registrazione terminata tempo: {ts.Seconds} [/]");
                    break;
                }
            }
            Thread.Sleep(1000);

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Errore durante la registrazione: {ex.Message}[/]");
            return false;
        }
    }

    public static bool PlayAudio(string fullPath)
    {
        try
        {
            using (var reader = new Mp3FileReader(fullPath))
            {
                using (var waveOut = new WaveOutEvent())
                {
                    waveOut.Init(reader);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                        Task.Delay(500).Wait();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Errore durante la riproduzione: {ex.Message}[/]");
            return false;
        }
    }
}
