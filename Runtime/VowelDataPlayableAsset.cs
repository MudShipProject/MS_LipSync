using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MudShip.LipSync
{
    [Serializable]
    public class VowelDataPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public VowelData vowelData;
        [Tooltip("Optional. Used only to draw the waveform on the clip. Falls back to VowelData.sourceClip.")]
        public AudioClip audioClip;

        public ClipCaps clipCaps =>
            ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

        public override double duration =>
            vowelData != null && vowelData.totalDuration > 0 ? vowelData.totalDuration : base.duration;

        public AudioClip GetAudioClip() =>
            audioClip != null ? audioClip : (vowelData != null ? vowelData.sourceClip : null);

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<VowelDataPlayableBehaviour>.Create(graph);
            playable.GetBehaviour().vowelData = vowelData;
            return playable;
        }
    }
}
