// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Audio;
using OpenTK;

namespace FX2.Game.Audio
{
    public unsafe class Phaser : Dsp
    {
        private const int NumBands = 6;

        private float feedback;
        private double time;
        private APF[] allPassFilters = new APF[NumBands * 2]; // 6 bands - Stereo
        private float[] feedbackBuffer = new float[2];
        private float maxmimumFrequency = 15000.0f;
        private float minimumFrequency = 5000.0f;
        private float frequencyDelta;

        public float MinimumFrequency
        {
            get { return minimumFrequency; }
            set
            {
                minimumFrequency = value;
                CalculateFrequencyDelta();
            }
        }

        public float MaxmimumFrequency
        {
            get { return maxmimumFrequency; }
            set
            {
                maxmimumFrequency = value;
                CalculateFrequencyDelta();
            }
        }

        public float Feedback
        {
            get { return feedback; }
            set { feedback = MathHelper.Clamp(value, 0.0f, 1.0f); }
        }

        public double Duration { get; set; } = 1.0;

        public override void Process(float* buffer, int numSamples)
        {
            float sampleRateFloat = (float)Track.SampleRate;
            double sampleStep = 1.0 / Track.SampleRate;
            for(int i = 0; i < numSamples; i++)
            {
                float f = (float)(time / Duration) * MathHelper.TwoPi;

                //calculate and update phaser sweep lfo...
                float d = minimumFrequency + frequencyDelta * (((float)Math.Sin(f) + 1.0f) / 2.0f);
                d /= sampleRateFloat;

                //calculate output per channel
                for(int c = 0; c < 2; c++)
                {
                    int filterOffset = c * NumBands;

                    //update filter coeffs
                    float a1 = (1.0f - d) / (1.0f + d);
                    for(int j = 0; j < NumBands; j++)
                        allPassFilters[i + filterOffset].a1 = a1;

                    // Calculate ouput from filters chained together
                    // Merry christmas!
                    float filtered = allPassFilters[0 + filterOffset].Update(
                        allPassFilters[1 + filterOffset].Update(
                            allPassFilters[2 + filterOffset].Update(
                                allPassFilters[3 + filterOffset].Update(
                                    allPassFilters[4 + filterOffset].Update(
                                        allPassFilters[5 + filterOffset].Update(buffer[i * 2 + c] + feedbackBuffer[c] * feedback))))));

                    // Store filter feedback
                    feedbackBuffer[c] = filtered;

                    // Final sample
                    buffer[i * 2 + c] = buffer[i * 2 + c] + filtered;
                }

                time += sampleStep;
            }
        }

        private void CalculateFrequencyDelta()
        {
            frequencyDelta = maxmimumFrequency - minimumFrequency;
        }

        public struct APF
        {
            public float Update(float input)
            {
                float y = input * -a1 + za;
                za = y * a1 + input;
                return y;
            }

            public float a1;
            public float za;
        };
    }
}