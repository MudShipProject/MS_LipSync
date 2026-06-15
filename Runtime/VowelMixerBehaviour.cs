using UnityEngine.Playables;

namespace MudShip.LipSync
{
    public class VowelMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!(playerData is VowelMorphApplier applier)) return;

            int n = playable.GetInputCount();
            Vowel best = Vowel.None;
            float bestWeight = 0f;

            for (int i = 0; i < n; i++)
            {
                float w = playable.GetInputWeight(i);
                if (w <= 0f) continue;

                var input = (ScriptPlayable<VowelDataPlayableBehaviour>)playable.GetInput(i);
                var behaviour = input.GetBehaviour();
                if (behaviour == null || behaviour.vowelData == null) continue;

                var v = behaviour.vowelData.GetVowelAt((float)input.GetTime());
                if (v == Vowel.None) continue;

                if (w >= bestWeight) { bestWeight = w; best = v; }
            }

            applier.ApplyImmediate(best, bestWeight);
        }
    }
}
