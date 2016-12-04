// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Audio;
using OpenTK;

namespace FX2.Game.Audio
{
    public unsafe class Flanger : Dsp
    {
        private double time;
        private int bufferOffset;

        private int minimumDelay;
        private int maximumDelay;

        private float[] sampleBuffer = new float[0];

        public double Duration { get; set; } = 1.0;

        public int MinimumDelay
        {
            get { return minimumDelay; }
            set
            {
                minimumDelay = value;
                UpdateDelayRange();
            }
        }

        public int MaximumDelay
        {
            get { return maximumDelay; }
            set
            {
                maximumDelay = value;
                UpdateDelayRange();
            }
        }

        public override unsafe void OnAdded()
        {
            base.OnAdded();
            UpdateDelayRange();
        }

        public override void Process(float* buffer, int numSamples)
        {
            if(sampleBuffer.Length == 0)
                return;
            
            double sampleStep = 1.0 / Track.SampleRate;
            for(int i = 0; i < numSamples; i++)
            {
                float f = ((float)time / (float)Duration) * MathHelper.TwoPi;
                int d = (int)(minimumDelay + ((maximumDelay - 1) - minimumDelay) * ((float)Math.Sin(f) * 0.5f + 0.5f));
                int samplePos = (int)((uint)(bufferOffset - d * 2) % sampleBuffer.Length);

                // Inject new sample
                sampleBuffer[bufferOffset + 0] = buffer[i * 2];
                sampleBuffer[bufferOffset + 1] = buffer[i * 2 + 1];

                // Apply delay
                buffer[i * 2] = (sampleBuffer[samplePos] + buffer[i * 2]) * 0.5f;
                buffer[i * 2 + 1] = (sampleBuffer[samplePos + 1] + buffer[i * 2 + 1]) * 0.5f;

                bufferOffset += 2;
                if(bufferOffset >= sampleBuffer.Length)
                    bufferOffset = 0;

                time += sampleStep;
            }
        }

        private void UpdateDelayRange()
        {
            if(Track == null) return;

            bufferOffset = 0;
            var targetBufferLength = MaximumDelay * 2;
            Array.Resize(ref sampleBuffer, targetBufferLength);
        }
    }
}