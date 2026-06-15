using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MudShip.LipSync.Editor
{
    public static class VowelDataBuilder
    {
        public static VowelData Build(
            string audioPath,
            string transcriptPath,
            string model,
            string assetSavePath)
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "ms_lipsync_" + Guid.NewGuid().ToString("N"));
            var corpusDir = Path.Combine(tempRoot, "corpus");
            var speakerDir = Path.Combine(corpusDir, "speaker");
            var outputDir = Path.Combine(tempRoot, "output");

            Directory.CreateDirectory(speakerDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                // Fixed ASCII stem avoids issues with spaces / non-ASCII source file names.
                const string stem = "audio";
                File.Copy(audioPath, Path.Combine(speakerDir, stem + Path.GetExtension(audioPath)), overwrite: true);

                var hasTranscript = !string.IsNullOrEmpty(transcriptPath) && File.Exists(transcriptPath);
                if (hasTranscript)
                {
                    File.Copy(transcriptPath, Path.Combine(speakerDir, stem + ".txt"), overwrite: true);
                    MFARunner.Align(corpusDir, model, model, outputDir);
                }
                else
                {
                    MFARunner.Transcribe(corpusDir, model, model, outputDir);
                }

                var textGridPath = Path.Combine(outputDir, "speaker", stem + ".TextGrid");
                if (!File.Exists(textGridPath))
                    throw new FileNotFoundException("MFA did not produce a TextGrid file.", textGridPath);

                var frames = TextGridParser.Parse(textGridPath);
                var asset = ScriptableObject.CreateInstance<VowelData>();
                asset.frames = frames;
                asset.totalDuration = frames.Length > 0
                    ? frames[frames.Length - 1].time + frames[frames.Length - 1].duration
                    : 0f;

                AssetDatabase.CreateAsset(asset, assetSavePath);
                AssetDatabase.SaveAssets();
                return asset;
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
