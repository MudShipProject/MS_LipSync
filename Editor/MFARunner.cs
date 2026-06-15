using System;
using System.Diagnostics;
using System.IO;

namespace MudShip.LipSync.Editor
{
    public static class MFARunner
    {
        public static void Align(string corpusDir, string dictionary, string acousticModel, string outputDir, string condaRoot, string condaEnv)
            => RunMfa($"align \"{corpusDir}\" \"{dictionary}\" \"{acousticModel}\" \"{outputDir}\" --clean --overwrite", condaRoot, condaEnv);

        public static void Transcribe(string corpusDir, string dictionary, string acousticModel, string outputDir, string condaRoot, string condaEnv)
            => RunMfa($"transcribe \"{corpusDir}\" \"{dictionary}\" \"{acousticModel}\" \"{outputDir}\" --clean", condaRoot, condaEnv);

        static void RunMfa(string mfaArgs, string condaRoot, string condaEnv)
        {
            var activate = ResolveActivate(condaRoot);
            if (activate == null)
                throw new Exception(
                    "conda activate.bat not found. Install Miniforge/Miniconda and set 'Conda Root' " +
                    $"to the install folder (tried: '{condaRoot}').");
            if (string.IsNullOrWhiteSpace(condaEnv))
                throw new Exception("'Conda Env' is empty. Set the env where MFA is installed (e.g. 'aligner').");

            // Activate the env (sets PATH for python + native deps), then run mfa.
            var bat = Path.Combine(Path.GetTempPath(), "ms_lipsync_run_" + Guid.NewGuid().ToString("N") + ".bat");
            File.WriteAllText(bat,
                "@echo off\r\n" +
                $"call \"{activate}\" \"{condaEnv}\"\r\n" +
                "if errorlevel 1 exit /b 1\r\n" +
                $"mfa {mfaArgs}\r\n" +
                "exit /b %errorlevel%\r\n");

            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c \"{bat}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var proc = Process.Start(psi);
                var outTask = proc.StandardOutput.ReadToEndAsync();
                var errTask = proc.StandardError.ReadToEndAsync();
                proc.WaitForExit();
                var stdout = outTask.Result;
                var stderr = errTask.Result;

                if (proc.ExitCode != 0)
                    throw new Exception($"MFA failed (exit {proc.ExitCode}).\n{Tail(stderr)}\n{Tail(stdout)}");
            }
            finally
            {
                try { if (File.Exists(bat)) File.Delete(bat); } catch { }
            }
        }

        static string ResolveActivate(string condaRoot)
        {
            if (string.IsNullOrWhiteSpace(condaRoot)) condaRoot = CondaLocator.Detect();
            if (string.IsNullOrWhiteSpace(condaRoot)) return null;

            if (File.Exists(condaRoot) && condaRoot.EndsWith("activate.bat", StringComparison.OrdinalIgnoreCase))
                return condaRoot;

            var candidate = Path.Combine(condaRoot, "Scripts", "activate.bat");
            if (File.Exists(candidate)) return candidate;
            candidate = Path.Combine(condaRoot, "condabin", "activate.bat");
            if (File.Exists(candidate)) return candidate;
            return null;
        }

        static string Tail(string s, int max = 4000)
            => string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s.Substring(s.Length - max));
    }
}
