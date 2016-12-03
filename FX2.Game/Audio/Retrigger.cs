// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Audio;

namespace FX2.Game.Audio
{
    public unsafe class Retrigger : Dsp, IMixable
    {
        /// <summary>
        /// Duration of the part to retrigger (in seconds)
        /// </summary>
        public double Duration;

        /// <summary>
        /// How many times to loop before restarting the retriggered part
        /// </summary>
        public int LoopCount;

        /// <summary>
        /// Amount(0,1) of time to mute before playing again, 1 is fully gated
        /// </summary>
        public float Gating;

        private float[] retriggerBuffer = new float[0];
        private int currentSample = 0;
        private int currentLoop = 0;
        private float oneMinusMix = 0.0f;
        private float mix = 1.0f;

        public float Mix
        {
            get { return mix; }
            set { mix = value;
                oneMinusMix = 1.0f - mix;
            }
        }

        public override void Process(float* buffer, int numSamples)
        {
            double sll = Track.SampleRate * Duration;
            int sampleDuration = (int)(Track.SampleRate * Duration);
            int sampleGatingLength = (int)(sll * (1.0-Gating));

            if(retriggerBuffer.Length < (sampleDuration*2))
                Array.Resize(ref retriggerBuffer, sampleDuration * 2);

            for(int i = 0; i < numSamples; i++)
            {
                if(currentLoop == 0)
                {
                    // Store samples for later
                    if(currentSample > sampleGatingLength) // Additional gating
                    {
                        retriggerBuffer[currentSample*2] = (0.0f);
                        retriggerBuffer[currentSample*2+1] = (0.0f);
                    }
                    else
                    {
                        retriggerBuffer[currentSample*2] = buffer[i * 2];
                        retriggerBuffer[currentSample*2+1] = buffer[i * 2 + 1];
                    }
                }

                // Sample from buffer
                buffer[i * 2] = retriggerBuffer[currentSample * 2] * Mix + buffer[i * 2] * oneMinusMix;
                buffer[i * 2 + 1] = retriggerBuffer[currentSample * 2 + 1] * Mix + buffer[i * 2+1] * oneMinusMix;
		
                // Increase index
                currentSample++;
                if(currentSample >= sampleDuration)
                {
                    currentSample -= sampleDuration;
                    currentLoop++;
                    if(LoopCount != 0 && currentLoop >= LoopCount)
                    {
                        // Reset
                        currentLoop = 0;
                        currentSample = 0;
                    }
                }
            }
        }
    }
}