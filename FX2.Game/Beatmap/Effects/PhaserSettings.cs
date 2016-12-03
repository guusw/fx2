// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class PhaserSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<PhaserEffectState>(playbackContext, this);

        public EffectTimeParameter Duration = new RelativeTimeParameter(4.0);
        public float Feedback = 0.2f;
        public float MinimumFrequency = 15000.0f;
        public float MaximumFrequency = 5000.0f;

        public class PhaserEffectState : DspEffectStateDefault<Phaser>
        {
            protected override void ApplyInitialSettings()
            {
                var settings = (PhaserSettings)Settings;
                Dsp.Duration = settings.Duration.GetActualDuration(Context.Playback.CurrentTimingPoint);
                Dsp.Feedback = settings.Feedback;
                Dsp.MinimumFrequency = settings.MinimumFrequency;
                Dsp.MaxmimumFrequency = settings.MaximumFrequency;
            }
        }
    }
}