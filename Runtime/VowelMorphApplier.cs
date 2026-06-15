using System.Collections.Generic;
using UnityEngine;

namespace MudShip.LipSync
{
    [System.Serializable]
    public class MorphTarget
    {
        public int blendShapeIndex = -1;
        [Range(0f, 1f)] public float weight = 1f;
    }

    [System.Serializable]
    public class VowelMorph
    {
        public Vowel vowel;
        public List<MorphTarget> targets = new List<MorphTarget>();
    }

    [AddComponentMenu("MS LipSync/Vowel Morph Applier")]
    public class VowelMorphApplier : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        [Tooltip("Optional. When set, the current vowel is read from this player each frame. Leave empty when driven by Timeline.")]
        public VowelPlayer player;
        public List<VowelMorph> vowelMorphs = new List<VowelMorph>();
        [Tooltip("Approach speed toward the target shape. 0 = instant.")]
        public float smoothing = 12f;

        float[] _current;
        float[] _target;
        readonly HashSet<int> _controlled = new HashSet<int>();

        void Reset() => skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        void OnEnable() => RebuildBuffers();
        void OnValidate() { if (isActiveAndEnabled) RebuildBuffers(); }

        int BlendShapeCount =>
            skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null
                ? skinnedMeshRenderer.sharedMesh.blendShapeCount
                : 0;

        public void RebuildBuffers()
        {
            int n = BlendShapeCount;
            if (_current == null || _current.Length != n)
            {
                _current = new float[n];
                _target = new float[n];
            }
            _controlled.Clear();
            foreach (var vm in vowelMorphs)
                foreach (var t in vm.targets)
                    if (t.blendShapeIndex >= 0 && t.blendShapeIndex < n)
                        _controlled.Add(t.blendShapeIndex);
        }

        void Update()
        {
            if (_current == null || _current.Length != BlendShapeCount) RebuildBuffers();
            if (player != null) SetVowel(player.Current);

            float t = smoothing <= 0f ? 1f : 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            foreach (int i in _controlled)
                _current[i] = Mathf.Lerp(_current[i], _target[i], t);
            Apply();
        }

        // Runtime: request a vowel; Update() smooths toward it.
        public void SetVowel(Vowel vowel)
        {
            if (_target == null) RebuildBuffers();
            foreach (int i in _controlled) _target[i] = 0f;
            Accumulate(_target, vowel, 1f);
        }

        // Timeline / scrubbing: set the shape instantly at the given intensity.
        public void ApplyImmediate(Vowel vowel, float intensity)
        {
            if (_current == null) RebuildBuffers();
            foreach (int i in _controlled) { _current[i] = 0f; _target[i] = 0f; }
            Accumulate(_current, vowel, Mathf.Clamp01(intensity));
            foreach (int i in _controlled) _target[i] = _current[i];
            Apply();
        }

        void Accumulate(float[] buffer, Vowel vowel, float intensity)
        {
            var morph = Find(vowel);
            if (morph == null) return;
            foreach (var t in morph.targets)
            {
                if (t.blendShapeIndex < 0 || t.blendShapeIndex >= buffer.Length) continue;
                // Normalized: each blend shape is clamped so it never exceeds 1 (max).
                buffer[t.blendShapeIndex] = Mathf.Clamp01(buffer[t.blendShapeIndex] + t.weight * intensity);
            }
        }

        VowelMorph Find(Vowel vowel)
        {
            for (int i = 0; i < vowelMorphs.Count; i++)
                if (vowelMorphs[i].vowel == vowel) return vowelMorphs[i];
            return null;
        }

        void Apply()
        {
            if (skinnedMeshRenderer == null) return;
            foreach (int i in _controlled)
                skinnedMeshRenderer.SetBlendShapeWeight(i, Mathf.Clamp01(_current[i]) * 100f);
        }
    }
}
