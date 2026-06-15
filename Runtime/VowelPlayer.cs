using System;
using UnityEngine;

namespace MudShip.LipSync
{
    public class VowelPlayer : MonoBehaviour
    {
        public VowelData vowelData;
        [SerializeField] AudioSource _audioSource;

        Vowel _current = Vowel.None;

        public event Action<Vowel> OnVowelChanged;
        public Vowel Current => _current;

        void Update()
        {
            if (_audioSource != null && _audioSource.isPlaying)
                Seek(_audioSource.time);
        }

        public void Seek(float time)
        {
            if (vowelData == null) return;
            var v = vowelData.GetVowelAt(time);
            if (v == _current) return;
            _current = v;
            OnVowelChanged?.Invoke(_current);
        }
    }
}
