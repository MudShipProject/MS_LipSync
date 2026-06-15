using System;
using System.Diagnostics;
using System.IO;

namespace MudShip.LipSync.Editor
{
    public static class AudioConverter
    {
        public static string ToWav(string inputPath, string outputDir, string ffmpegPath = "ffmpeg")
        {
            var outPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(inputPath) + ".wav");
            var ext = Path.GetExtension(inputPath).ToLowerInvariant();

            if (ext == ".wav")
            {
                File.Copy(inputPath, outPath, overwrite: true);
                return outPath;
            }

            var psi = new ProcessStartInfo(ffmpegPath,
                $"-y -i \"{inputPath}\" -ar 16000 -ac 1 -sample_fmt s16 \"{outPath}\"")
            {
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new Exception($"ffmpeg failed:\n{stderr}");

            return outPath;
        }
    }
}
