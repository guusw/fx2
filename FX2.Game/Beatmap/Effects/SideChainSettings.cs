// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class SideChainSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<SideChainEffectState>(playbackContext, this);

        public EffectTimeParameter Duration = new DivisionTimeParameter(1, 4);
        
        public class SideChainEffectState : DspEffectStateDefault<SideChain>
        {
            protected override void ApplyInitialSettings()
            {
                var settings = (FlangerSettings)Settings;
                Dsp.Duration = settings.Duration.GetActualDuration(Context.Playback.CurrentTimingPoint);
            }

            public override void ApplyObjectParameters(ObjectReference objectReference)
            {
                var hold = objectReference.Object as Hold;
                if(hold != null)
                {
                    Dsp.Duration = 1 / (double)hold.EffectParameter0 * Context.Playback.CurrentTimingPoint.MeasureDuration;
                }
            }
        }
    }
}