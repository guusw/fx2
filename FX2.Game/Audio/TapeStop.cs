// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Audio;

namespace FX2.Game.Audio
{
    public class TapeStop : Dsp
    {
        private double duration = 500;
        private int samplePosition;
        private float floatSamplePosition;
        private float[] sampleBuffer = new float[0];

        public double Duration
        {
            get { return duration; }
            set { SetDuration(value); }
        }

        public override void OnAdded()
        {
            base.OnAdded();
            SetDuration(duration);
        }

        public override unsafe void Process(float* buffer, int numSamples)
        {
            int sampleDuration = sampleBuffer.Length >> 1;
            for(int i = 0; i < numSamples; i++)
            {
                float sampleRate = 1.0f - (float)samplePosition / (float)sampleDuration;
                if(sampleRate <= 0.0f)
                {
                    // Mute
                    buffer[i * 2] = 0.0f;
                    buffer[i * 2 + 1] = 0.0f;
                    continue;
                }

                // Store samples for later
                sampleBuffer[samplePosition * 2] = buffer[i * 2];
                sampleBuffer[samplePosition * 2+1] = buffer[i * 2];

                // The sample index into the stored buffer
                int i2 = (int)Math.Floor(floatSamplePosition);
                buffer[i * 2] = sampleBuffer[i2 * 2];
                buffer[i * 2 + 1] = sampleBuffer[i2 * 2 + 1];

                // Increase index
                floatSamplePosition += sampleRate;
                samplePosition++;
            }
        }

        private void SetDuration(double duration)
        {
            this.duration = duration;
            if(Track == null) return;

            int numSamples = (int)(duration * Track.SampleRate) * 2;
            Array.Resize(ref sampleBuffer, numSamples);
        }
    }
}