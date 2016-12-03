// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
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
    }
}