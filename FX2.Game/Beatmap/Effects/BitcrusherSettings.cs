// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public class BitCrusherSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<BitCrusherEffectState>(playbackContext, this);

        /// <summary>
        /// Reduction in sample rate
        /// </summary>
        public EffectParameterRange<double> Reduction;

        public class BitCrusherEffectState : DspEffectStateDefault<BitCrusher>
        {
            protected override void ApplyInitialSettings()
            {
                var settings = (BitCrusherSettings)Settings;
                Dsp.Reduction = settings.Reduction;
            }

            public override void ApplyObjectParameters(ObjectReference objectReference)
            {
                var hold = objectReference.Object as Hold;
                if(hold != null)
                {
                    Dsp.Reduction = hold.EffectParameter0;
                }
            }

            public override void Modulate(float input)
            {
                var settings = (BitCrusherSettings)Settings;
                Dsp.Reduction = settings.Reduction.Minimum +
                                (settings.Reduction.Maximum - settings.Reduction.Minimum) * input;
            }
        }
    }
}