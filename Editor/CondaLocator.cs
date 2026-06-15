using System;
using System.IO;

namespace MudShip.LipSync.Editor
{
    public static class CondaLocator
    {
        static readonly string[] kNames = { "miniforge3", "miniconda3", "anaconda3", "mambaforge" };

        // Returns a conda install root containing Scripts/activate.bat, or "" if none found.
        public static string Detect()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] bases =
            {
                home,
                local,
                string.IsNullOrEmpty(local) ? null : Path.Combine(local, "Programs"),
                @"C:\ProgramData",
                @"C:\",
            };

            foreach (var b in bases)
            {
                if (string.IsNullOrEmpty(b)) continue;
                foreach (var name in kNames)
                {
                    var root = Path.Combine(b, name);
                    if (File.Exists(Path.Combine(root, "Scripts", "activate.bat")))
                        return root;
                }
            }
            return "";
        }
    }
}
