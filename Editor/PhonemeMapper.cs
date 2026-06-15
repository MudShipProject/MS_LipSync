using System.Collections.Generic;

namespace MudShip.LipSync.Editor
{
    public static class PhonemeMapper
    {
        static readonly Dictionary<string, Vowel> _map = new Dictionary<string, Vowel>(System.StringComparer.OrdinalIgnoreCase)
        {
            // English ARPAbet
            { "AA", Vowel.A }, { "AE", Vowel.A }, { "AH", Vowel.A }, { "AW", Vowel.A }, { "AY", Vowel.A },
            { "IH", Vowel.I }, { "IY", Vowel.I }, { "EY", Vowel.I },
            { "UH", Vowel.U }, { "UW", Vowel.U }, { "OW", Vowel.U },
            { "EH", Vowel.E }, { "ER", Vowel.E },
            { "AO", Vowel.O }, { "OY", Vowel.O },
            // Japanese
            { "a", Vowel.A }, { "i", Vowel.I }, { "u", Vowel.U },
            { "e", Vowel.E }, { "o", Vowel.O },
        };

        public static Vowel ToVowel(string phoneme)
        {
            if (string.IsNullOrWhiteSpace(phoneme)) return Vowel.None;
            var key = phoneme.TrimEnd('0', '1', '2');
            return _map.TryGetValue(key, out var v) ? v : Vowel.None;
        }
    }
}
