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
            string mfaDictPath,
            string mfaAcousticModel,
            string mfaExecutable,
            string ffmpegExecutable,
            string assetSavePath)
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "ms_lipsync_" + Guid.NewGuid().ToString("N"));
            var speakerDir = Path.Combine(tempRoot, "corpus", "speaker");
            var outputDir = Path.Combine(tempRoot, "output");

            Directory.CreateDirectory(speakerDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                var wavPath = AudioConverter.ToWav(audioPath, speakerDir, ffmpegExecutable);
                var stem = Path.GetFileNameWithoutExtension(wavPath);
                var hasTranscript = !string.IsNullOrEmpty(transcriptPath) && File.Exists(transcriptPath);

                if (hasTranscript)
                {
                    File.Copy(transcriptPath, Path.Combine(speakerDir, stem + ".txt"), overwrite: true);
                    MFARunner.Align(Path.Combine(tempRoot, "corpus"), mfaDictPath, mfaAcousticModel, outputDir, mfaExecutable);
                }
                else
                {
                    MFARunner.Transcribe(Path.Combine(tempRoot, "corpus"), mfaDictPath, mfaAcousticModel, outputDir, mfaExecutable);
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
