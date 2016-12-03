// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Drawing.Printing;
using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class FlangerSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<FlangerEffectState>(playbackContext, this);

        public EffectTimeParameter Duration = new RelativeTimeParameter(4.0);

        public int MinimumDelay = 20;
        public int MaximumDelay = 40;

        public class FlangerEffectState : DspEffectStateDefault<Flanger>
        {
            protected override void ApplyInitialSettings()
            {
                var settings = (FlangerSettings)Settings;
                Dsp.Duration = settings.Duration.GetActualDuration(Context.Playback.CurrentTimingPoint);
                Dsp.MinimumDelay = settings.MinimumDelay;
                Dsp.MaximumDelay = settings.MaximumDelay;
            }
        }
    }
}