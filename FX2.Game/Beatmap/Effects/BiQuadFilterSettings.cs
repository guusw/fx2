// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using FX2.Game.Audio;

namespace FX2.Game.Beatmap.Effects
{
    public enum BiQuadFilterType
    {
        HighPass,
        LowPass,
        Peaking
    }

    public class BiQuadFilterSettings : EffectSettings
    {
        public override EffectState CreateEffectState(PlaybackContext playbackContext)
            => EffectState.Instantiate<BiQuadFilterEffectState>(playbackContext, this);

        public BiQuadFilterType FilterType;

        public EffectParameterRange<double> Q = new EffectParameterRange<double>(0.5, 1);
        public EffectParameterRange<double> Frequency = new EffectParameterRange<double>(200.0, 10000.0);
        public EffectParameterRange<double> Gain = new EffectParameterRange<double>(20.0, 20.0);

        public class BiQuadFilterEffectState : DspEffectStateDefault<BiQuadFilter>
        {
            /// <summary>
            /// Samples a frequency from the settings from a 0-1 input value
            /// </summary>
            public void InterpolateFilterParameters(double input, out double frequency, out double q, out double gain)
            {
                var settings = (BiQuadFilterSettings)Settings;
                input = Math.Pow(input, 1.6f);
                frequency = settings.Frequency.Minimum + (settings.Frequency.Maximum - settings.Frequency.Minimum) * input;
                q = settings.Q.Minimum + (settings.Q.Maximum - settings.Q.Minimum) * input;
                gain = settings.Gain.Minimum + (settings.Gain.Maximum - settings.Gain.Minimum) * input;
            }
            
            public override void Modulate(float input)
            {
                var settings = (BiQuadFilterSettings)Settings;
                double frequency;
                double gain;
                double q;
                InterpolateFilterParameters(input, out frequency, out q, out gain);
                switch(settings.FilterType)
                {
                    case BiQuadFilterType.HighPass:
                        Dsp.SetHighPass(q, frequency, Context.Track.SampleRate);
                        break;
                    case BiQuadFilterType.LowPass:
                        Dsp.SetLowPass(q, frequency, Context.Track.SampleRate);
                        break;
                    case BiQuadFilterType.Peaking:
                        Dsp.SetPeaking(q, frequency, gain, Context.Track.SampleRate);
                        break;
                }
            }
        }
    }
}