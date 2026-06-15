using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MudShip.LipSync
{
    [TrackColor(0.23f, 0.78f, 0.95f)]
    [TrackClipType(typeof(VowelDataPlayableAsset))]
    [TrackBindingType(typeof(VowelMorphApplier))]
    public class VowelTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
            => ScriptPlayable<VowelMixerBehaviour>.Create(graph, inputCount);
    }
}
