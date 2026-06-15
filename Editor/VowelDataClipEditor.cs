using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace MudShip.LipSync.Editor
{
    [CustomTimelineEditor(typeof(VowelDataPlayableAsset))]
    public class VowelDataClipEditor : ClipEditor
    {
        const int kBuckets = 4096;
        static readonly Dictionary<AudioClip, float[]> _cache = new Dictionary<AudioClip, float[]>();
        static readonly Color kWave = new Color(0.23f, 0.78f, 0.95f, 0.55f);

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as VowelDataPlayableAsset;
            var audio = asset != null ? asset.GetAudioClip() : null;
            if (audio == null) return;

            var env = GetEnvelope(audio);
            double audioLen = audio.length;
            if (env == null || audioLen <= 0) return;

            var rect = region.position;
            float mid = rect.y + rect.height * 0.5f;
            float halfH = rect.height * 0.5f * 0.95f;
            int cols = Mathf.Max(1, Mathf.RoundToInt(rect.width));

            for (int x = 0; x < cols; x++)
            {
                double frac = cols <= 1 ? 0.0 : (double)x / (cols - 1);
                double clipLocal = region.startTime + (region.endTime - region.startTime) * frac;
                double audioTime = clip.clipIn + clipLocal * clip.timeScale;
                double n = audioTime / audioLen;
                if (n < 0.0 || n > 1.0) continue;

                int b = Mathf.Clamp((int)(n * (env.Length - 1)), 0, env.Length - 1);
                float h = Mathf.Max(1f, env[b] * halfH);
                EditorGUI.DrawRect(new Rect(rect.x + x, mid - h, 1f, h * 2f), kWave);
            }
        }

        static float[] GetEnvelope(AudioClip clip)
        {
            if (_cache.TryGetValue(clip, out var cached)) return cached;
            if (clip.loadType == AudioClipLoadType.Streaming) return null;

            int total = clip.samples;
            int ch = Mathf.Max(1, clip.channels);
            if (total <= 0) return null;

            var env = new float[kBuckets];
            const int chunk = 65536;
            try
            {
                int offset = 0;
                while (offset < total)
                {
                    int count = Mathf.Min(chunk, total - offset);
                    var buf = new float[count * ch];
                    if (!clip.GetData(buf, offset)) break;
                    for (int i = 0; i < count; i++)
                    {
                        int g = offset + i;
                        int b = (int)((long)g * kBuckets / total);
                        if (b >= kBuckets) b = kBuckets - 1;
                        float v = Mathf.Abs(buf[i * ch]);
                        if (v > env[b]) env[b] = v;
                    }
                    offset += count;
                }
            }
            catch { return null; }

            _cache[clip] = env;
            return env;
        }
    }
}
