using UnityEngine;

namespace MudShip.LipSync
{
    [CreateAssetMenu(menuName = "MS LipSync/Vowel Data")]
    public class VowelData : ScriptableObject
    {
        public VowelFrame[] frames;
        public float totalDuration;
        public AudioClip sourceClip;

        public Vowel GetVowelAt(float time)
        {
            if (frames == null || frames.Length == 0) return Vowel.None;
            int lo = 0, hi = frames.Length - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1;
                var f = frames[mid];
                if (time < f.time) hi = mid - 1;
                else if (time >= f.time + f.duration) lo = mid + 1;
                else return f.vowel;
            }
            return Vowel.None;
        }
    }
}
