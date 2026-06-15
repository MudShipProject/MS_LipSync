using System;
using System.Diagnostics;

namespace MudShip.LipSync.Editor
{
    public static class MFARunner
    {
        public static void Align(string corpusDir, string dictPath, string acousticModel,
            string outputDir, string mfaExecutable = "mfa")
            => Run(mfaExecutable,
                $"align \"{corpusDir}\" \"{dictPath}\" \"{acousticModel}\" \"{outputDir}\" --clean --overwrite");

        public static void Transcribe(string corpusDir, string dictPath, string acousticModel,
            string outputDir, string mfaExecutable = "mfa")
            => Run(mfaExecutable,
                $"transcribe \"{corpusDir}\" \"{dictPath}\" \"{acousticModel}\" \"{outputDir}\" --clean");

        static void Run(string exe, string args)
        {
            var psi = new ProcessStartInfo(exe, args)
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
