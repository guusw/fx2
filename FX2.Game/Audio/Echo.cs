// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Audio;

namespace FX2.Game.Audio
{
    public unsafe class Echo : Dsp
    {
        private double duration = 1.0;
        private int bufferOffset = 0;
        private float[] echoBuffer;
        private int loopCount = 0;

        public double Duration
        {
            get { return duration; }
            set { duration = value; UpdateDuration(); }
        }

        public float Feedback = 0.2f;

        public override void OnAdded()
        {
            base.OnAdded();
            UpdateDuration();
        }

        public override void Process(float* buffer, int numSamples)
        {
            for(int i = 0; i < numSamples; i++)
            {
                // Insert new samples before current position
                int insertPos = (bufferOffset - 2) % echoBuffer.Length;

                float l0 = echoBuffer[bufferOffset + 0];
                float l1 = echoBuffer[bufferOffset + 1];

                if(loopCount > 0)
                {
                    // Send echo to output
                    buffer[i * 2] = l0;
                    buffer[i * 2 + 1] = l1;
                }

                // Inject new sample
                echoBuffer[insertPos + 0] = buffer[i * 2] * Feedback;
                echoBuffer[insertPos + 1] = buffer[i * 2 + 1] * Feedback;

                bufferOffset += 2;
		        if(bufferOffset >= echoBuffer.Length)
		        {
                    bufferOffset = 0;
                    loopCount++;
		        }
	        }
        }

        private void UpdateDuration()
        {
            if(Track == null) return;

            int targetLength = (int)(Track.SampleRate * duration * 2);
            bufferOffset = 0;
            Array.Resize(ref echoBuffer, targetLength);
        }
    }
}