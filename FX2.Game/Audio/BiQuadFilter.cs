// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

// References:
// https://www.youtube.com/watch?v=FnpkBE4kJ6Q&list=WL&index=8
// http://www.musicdsp.org/files/Audio-EQ-Cookbook.txt for the coefficient formulas

using System;
using osu.Framework.Audio;
using OpenTK;

namespace FX2.Game.Audio
{
    public unsafe class BiQuadFilter : Dsp
    {
        private float b0 = 1.0f;
        private float b1 = 0.0f;
        private float b2 = 0.0f;
        private float a0 = 1.0f;
        private float a1 = 0.0f;
        private float a2 = 0.0f;

        // FIR and IIR Delay buffers
        private float[] feedbackBuffer = new float[8];

        public override unsafe void Process(float* buffer, int numSamples)
        {
            int singleSamples = numSamples * 2;
            int c = 0;
            for(int i = 0; i < singleSamples; i++)
            {
                // Offset in feedback buffer for this channel
                int fbOffset = c * 4;

                float filtered =
                    (b0 / a0) * buffer[i] +
                    (b1 / a0) * feedbackBuffer[fbOffset + 2] + // b buffer
                    (b2 / a0) * feedbackBuffer[fbOffset + 3] - // b buffer
                    (a1 / a0) * feedbackBuffer[fbOffset + 0] - // a buffer
                    (a2 / a0) * feedbackBuffer[fbOffset + 1];  // a buffer

                // Shift delay buffers
                feedbackBuffer[fbOffset + 3] = feedbackBuffer[fbOffset + 2];
                feedbackBuffer[fbOffset + 2] = buffer[i];

                // Feedback the calculated value into the IIR delay buffers
                feedbackBuffer[fbOffset + 1] = feedbackBuffer[fbOffset + 0];
                feedbackBuffer[fbOffset + 0] = filtered;

                buffer[i] = filtered;

                // Next channel
                if(++c == 2)
                    c = 0; 
            }
        }

        public void SetLowPass(double q, double freq, double sampleRate)
        {
            // Limit q
            q = Math.Max(q, 0.01f);

            // Sampling frequency
            double w0 = (2 * MathHelper.Pi * freq) / sampleRate;
            double cw0 = Math.Cos(w0);
            float alpha = (float)(Math.Sin(w0) / (2 * q));

            b0 = (float)((1 - cw0) / 2);
            b1 = (float)(1 - cw0);
            b2 = (float)((1 - cw0) / 2);
            a0 = 1 + alpha;
            a1 = (float)(-2 * cw0);
            a2 = 1 - alpha;
        }

        public void SetHighPass(double q, double freq, double sampleRate)
        {
            // Limit q
            q = Math.Max(q, 0.01f);
            
            double w0 = (2 * MathHelper.Pi * freq) / sampleRate;
            double cw0 = Math.Cos(w0);
            float alpha = (float)(Math.Sin(w0) / (2 * q));

            b0 = (float)((1 + cw0) / 2);
            b1 = (float)-(1 + cw0);
            b2 = (float)((1 + cw0) / 2);
            a0 = 1 + alpha;
            a1 = (float)(-2 * cw0);
            a2 = 1 - alpha;
        }

        public void SetPeaking(double q, double freq, double gain, double sampleRate)
        {
            // Limit q
            q = Math.Max(q, 0.01f);

            double w0 = (2 * MathHelper.Pi * freq) / sampleRate;
            double cw0 = Math.Cos(w0);
            float alpha = (float)(Math.Sin(w0) / (2 * q));
            double A = Math.Pow(10, (gain / 40));

            b0 = 1 + (float)(alpha * A);
            b1 = -2 * (float)cw0;
            b2 = 1 - (float)(alpha * A);
            a0 = 1 + (float)(alpha / A);
            a1 = -2 * (float)cw0;
            a2 = 1 - (float)(alpha / A);
        }
    }
}