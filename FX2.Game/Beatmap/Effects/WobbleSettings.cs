// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class WobbleSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<WobbleEffectState>(playbackContext, this);
        
        public EffectTimeParameter Duration = new DivisionTimeParameter(1, 3);

        public class WobbleEffectState : DspEffectStateDefault<BiQuadFilter>
        {
            public double CurrentTime;

            public override void Update(TimeSpan elapsedTime)
            {
                CurrentTime += elapsedTime.TotalSeconds;
                // TODO: Implement wobble
            }
        }
        
        public class FromKsh : KshEffectFactory
        {
            public override IEnumerable<EffectType> SupportedEffectTypes { get; } = new[] { EffectType.Wobble };

            public override EffectSettings GenerateEffectSettings(BeatmapKsh.EffectDefinition effectDefinition)
            {
                return new WobbleSettings();
            }
        }
    }
}