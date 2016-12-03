// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Audio;

namespace FX2.Game.Audio
{

    public unsafe class SideChain : Dsp
    {
        private double time;

        public float Amount { get; set; }

        public double Duration { get; set; }
        
        public override void Process(float* buffer, int numSamples)
        {
            if(Duration == 0.0)
                return;

            double step = 1/Track.SampleRate;
            for(int i = 0; i < numSamples; i++)
            {
                float r = (float)time / (float)Duration;
                // FadeIn
                const float fadeIn = 0.08f;
                if(r < fadeIn)
                    r = 1.0f - r / fadeIn;
                else
                    r = Curve((r - fadeIn) / (1.0f - fadeIn));
                float sampleGain = 1.0f - Amount * (1.0f - r);
                buffer[i * 2 + 0] *= sampleGain;
                buffer[i * 2 + 1] *= sampleGain;

                time += step;
                if(time > Duration)
                    time = 0;
            }
        }

        private float Curve(float input)
        {
            return input;
        }
    }
}