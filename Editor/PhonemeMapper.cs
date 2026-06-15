using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MudShip.LipSync.Editor
{
    public static class PhonemeMapper
    {
        // MFA IPA phone set (japanese_mfa / english_mfa) + ARPAbet (english_us_arpa).
        // Source file must stay UTF-8 (IPA glyphs used as literal dictionary keys).
        static readonly Dictionary<string, Vowel> _map = new Dictionary<string, Vowel>(System.StringComparer.Ordinal)
        {
            // Japanese IPA: a i ɯ ɨ e o
            { "a", Vowel.A }, { "i", Vowel.I }, { "ɯ", Vowel.U }, { "ɨ", Vowel.U }, { "e", Vowel.E }, { "o", Vowel.O },
            // English IPA: ɪ u ʊ ɛ æ ə ʌ ɑ ɒ ɔ ɜ ɝ
            { "ɪ", Vowel.I }, { "u", Vowel.U }, { "ʊ", Vowel.U }, { "ɛ", Vowel.E }, { "æ", Vowel.A },
            { "ə", Vowel.E }, { "ʌ", Vowel.A }, { "ɑ", Vowel.A }, { "ɒ", Vowel.O }, { "ɔ", Vowel.O },
            { "ɜ", Vowel.E }, { "ɝ", Vowel.E },
            // English ARPAbet
            { "AA", Vowel.A }, { "AE", Vowel.A }, { "AH", Vowel.A }, { "AW", Vowel.A }, { "AY", Vowel.A },
            { "IH", Vowel.I }, { "IY", Vowel.I }, { "EY", Vowel.I },
            { "UH", Vowel.U }, { "UW", Vowel.U }, { "OW", Vowel.U },
            { "EH", Vowel.E }, { "ER", Vowel.E }, { "AO", Vowel.O }, { "OY", Vowel.O },
        };

        public static Vowel ToVowel(string phoneme)
        {
            if (string.IsNullOrWhiteSpace(phoneme)) return Vowel.None;
            var key = Normalize(phoneme);
            if (key.Length == 0) return Vowel.None;
            if (_map.TryGetValue(key, out var v)) return v;
            // Diphthongs (aɪ, oʊ, aj, ow, ...) fall back to the leading vowel.
            if (_map.TryGetValue(key.Substring(0, 1), out var fv)) return fv;
            return Vowel.None;
        }

        // Strips ARPAbet stress digits, IPA length marks (ː ˑ) and combining
        // diacritics such as the devoicing ring on i̥ / ɯ̥ / ɨ̥.
        static string Normalize(string p)
        {
            p = p.Trim().TrimEnd('0', '1', '2');
            p = p.Replace("ː", "").Replace(":", "").Replace("ˑ", "");
            var d = p.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (var ch in d)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
