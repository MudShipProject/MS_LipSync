using System;

namespace MudShip.LipSync
{
    [Serializable]
    public struct VowelFrame
    {
        public float time;
        public float duration;
        public Vowel vowel;
    }
}
