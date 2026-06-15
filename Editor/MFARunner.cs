using System;
using System.Diagnostics;

namespace MudShip.LipSync.Editor
{
    public static class MFARunner
    {
        const string Exe = "mfa";

        public static void Align(string corpusDir, string dictionary, string acousticModel, string outputDir)
            => Run($"align \"{corpusDir}\" \"{dictionary}\" \"{acousticModel}\" \"{outputDir}\" --clean --overwrite");

        public static void Transcribe(string corpusDir, string dictionary, string acousticModel, string outputDir)
            => Run($"transcribe \"{corpusDir}\" \"{dictionary}\" \"{acousticModel}\" \"{outputDir}\" --clean");

        static void Run(string args)
        {
            var psi = new ProcessStartInfo(Exe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new Exception($"MFA exited ({proc.ExitCode}):\n{stderr}");
        }
    }
}
