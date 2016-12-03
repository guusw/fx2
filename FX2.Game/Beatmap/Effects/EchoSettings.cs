// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class EchoSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<EchoEffectState>(playbackContext, this);

        public EffectTimeParameter Duration = new DivisionTimeParameter(1, 4);

        public float Feedback = 0.2f;

        public class EchoEffectState : DspEffectStateDefault<Echo>
        {
            protected override void ApplyInitialSettings()
            {
                var settings = (EchoSettings)Settings;
                Dsp.Duration = settings.Duration.GetActualDuration(Context.Playback.CurrentTimingPoint);
                Dsp.Feedback = settings.Feedback;
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