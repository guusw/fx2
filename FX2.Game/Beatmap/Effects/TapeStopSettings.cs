// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class TapeStopSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<TapeStopEffectState>(playbackContext, this);
        
        public class TapeStopEffectState : DspEffectStateDefault<TapeStop>
        {
            public override void ApplyObjectParameters(ObjectReference objectReference)
            {
                var hold = objectReference.Object as Hold;
                if(hold != null)
                {
                    Dsp.Duration = hold.GetAbsoluteDuration(objectReference.Measure);
                }
            }
        }
    }
}