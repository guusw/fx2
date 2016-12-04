// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class RetriggerSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<RetriggerEffectState>(playbackContext, this);

        /// <summary>
        /// Period samples to repeat
        /// </summary>
        public EffectTimeParameter Duration = new DivisionTimeParameter(1, 4);

        /// <summary>
        /// How many times to loop before resetting
        /// </summary>
        public int LoopCount = 0;

        /// <summary>
        /// Amout of gating to apply between retriggers
        /// </summary>
        public float Gating = 0.1f;

        public class RetriggerEffectState : DspEffectStateDefault<Retrigger>
        {
            protected override void ApplyInitialSettings()
            {
                var settings = (RetriggerSettings)Settings;
                Dsp.Duration = settings.Duration.GetActualDuration(Context.Playback.CurrentTimingPoint);
                Dsp.Gating = settings.Gating;
                Dsp.LoopCount = settings.LoopCount;
            }

            public override void ApplyObjectParameters(ObjectReference objectReference)
            {
                var hold = objectReference.Object as Hold;
                if(hold != null)
                {
                    Dsp.Duration = 1 / (double)hold.EffectParameter0 *
                                   Context.Playback.CurrentTimingPoint.MeasureDuration;
                }
            }
        }

        public class FromKsh : KshEffectFactory
        {
            public override IEnumerable<EffectType> SupportedEffectTypes { get; } = new[] {EffectType.Retrigger, EffectType.Gate};

            public override EffectSettings GenerateEffectSettings(BeatmapKsh.EffectDefinition effectDefinition)
            {
                var settings = new RetriggerSettings();
                if(effectDefinition.BaseType == EffectType.Gate)
                    settings.LoopCount = 1;
                return settings;
            }
        }
    }
}