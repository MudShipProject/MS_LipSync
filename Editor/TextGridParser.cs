using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MudShip.LipSync.Editor
{
    public static class TextGridParser
    {
        public static VowelFrame[] Parse(string textGridPath)
        {
            var lines = File.ReadAllLines(textGridPath);
            var frames = new List<VowelFrame>();
            bool inPhones = false, inInterval = false;
            double xmin = 0, xmax = 0;

            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (!inPhones)
                {
                    if (line == "name = \"phones\"") inPhones = true;
                    continue;
                }

                if (line.StartsWith("item [")) break;

                if (line.StartsWith("intervals ["))
                {
                    inInterval = true;
                    continue;
                }

                if (!inInterval) continue;

                if (line.StartsWith("xmin = "))
                    xmin = double.Parse(line.Substring(7), CultureInfo.InvariantCulture);
                else if (line.StartsWith("xmax = "))
                    xmax = double.Parse(line.Substring(7), CultureInfo.InvariantCulture);
                else if (line.StartsWith("text = \""))
                {
                    var phone = line.Substring(8).TrimEnd('"');
                    var v = PhonemeMapper.ToVowel(phone);
                    if (v != Vowel.None)
                        frames.Add(new VowelFrame { time = (float)xmin, duration = (float)(xmax - xmin), vowel = v });
                    inInterval = false;
                }
            }

            return frames.ToArray();
        }
    }
}
