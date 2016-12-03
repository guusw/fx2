// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Audio;

namespace FX2.Game.Audio
{
    public interface IMixable
    {
        float Mix { get; set; }
    }

    public unsafe class BitCrusher : Dsp
    {
        private double samplePosition;

        private float sampleLeft;
        private float sampleRight;

        /// <summary>
        /// Make larger than 1 to stretch out samples across a longer period of samples
        /// </summary>
        public double Reduction { get; set; } = 1.0;

        public override void Process(float* buffer, int numSamples)
        {
            for(int i = 0; i < numSamples; i++)
            {
                samplePosition += 1.0;
                if(samplePosition > Reduction)
                {
                    sampleLeft = buffer[i * 2];
                    sampleRight = buffer[i * 2 + 1];
                    samplePosition -= Reduction;
                }

                buffer[i * 2] = sampleLeft;
                buffer[i * 2 + 1] = sampleRight;
            }
        }
    }
}