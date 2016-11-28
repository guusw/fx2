// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Timing point, it defines the BPM and timing signature for the following measures
    /// TODO: This class could cache some divisions
    /// </summary>
    public class TimingPoint
    {
        /// <summary>
        /// Beatmap which contains this timing point
        /// </summary>
        internal Beatmap Beatmap;

        private List<Measure> measures = new List<Measure>();
        private double beatDuration;
        private double bpm = 60;
        private int numerator = 4;
        private int denominator = 4;
        private double firstMeasureOffsetPercentage;
        private double actualOffset;
        private double offset;
        
        /// <summary>
        /// Offset of the timing point, in seconds
        /// </summary>
        public double Offset
        {
            get { return offset; }
            set { offset = value; Update(); }
        }

        /// <summary>
        /// Offset that moves the first measure into the end of the last timing point, this is used to create the case where KSM changes the BPM halfway into a measure but it doesn't reset the measure.
        /// </summary>
        public double FirstMeasureOffsetPercentage
        {
            get { return firstMeasureOffsetPercentage; }
            set { firstMeasureOffsetPercentage = value; Update(); }
        }

        /// <summary>
        /// The BPM of the timing point
        /// </summary>
        public double BPM
        {
            get { return bpm; }
            set { bpm = value; Update(); }
        }

        /// <summary>
        /// Upper part of the time signature
        /// how many beats per bar
        /// </summary>
        public int Numerator
        {
            get { return numerator; }
            set { numerator = value; Update(); }
        }

        /// <summary>
        /// Lower part of the time signature
        /// the note value (4th, 3th, 8th notes, etc.) for a beat
        /// </summary>
        public int Denominator
        {
            get { return denominator; }
            set { denominator = value; Update(); }
        }

        /// <summary>
        /// Duration of a beat (4th note)
        /// </summary>
        public double BeatDuration
        {
            get { return beatDuration; }
            set { BPM = 60.0 / value; }
        }

        /// <summary>
        /// Duration of a whole note (1th)
        /// </summary>
        public double WholeNoteDuration { get; private set; }

        /// <summary>
        /// The duration of a single measure
        /// </summary>
        public double MeasureDuration { get; private set; }

        /// <summary>
        /// Measures in this timing point
        /// </summary>
        public IReadOnlyList<Measure> Measures => measures;

        /// <summary>
        /// Previous timing point in the beatmap
        /// </summary>
        public TimingPoint Previous
        {
            get
            {
                if(Beatmap == null)
                    return null;

                int index = Beatmap.TimingPoints.IndexOf(this);
                if(index > 0)
                    return Beatmap.TimingPoints[index - 1];

                return null;
            }
        }

        /// <summary>
        /// Next timing point in the beatmap
        /// </summary>
        public TimingPoint Next
        {
            get
            {
                if(Beatmap == null)
                    return null;

                int index = Beatmap.TimingPoints.IndexOf(this) + 1;
                if(index < Beatmap.TimingPoints.Count)
                    return Beatmap.TimingPoints[index];

                return null;
            }
        }

        public TimingPoint()
        {
            Update();
        }

        public double GetMeasureOffset(int index)
        {
            return actualOffset + MeasureDuration * index;
        }

        public double GetDivisionDuration(TimeDivision division)
        {
            return MeasureDuration * ((double)division.DivisionIndex / division.Division);
        }

        /// <summary>
        /// Add a new measure
        /// </summary>
        /// <returns></returns>
        public Measure AddMeasure()
        {
            var measure = new Measure() {Index = measures.Count, TimingPoint = this};
            measures.Add(measure);
            return measure;
        }

        /// <summary>
        /// Remove a measure from this timing point
        /// </summary>
        /// <param name="measure"></param>
        public void RemoveMeasure(Measure measure)
        {
            measures.Remove(measure);
            measure.TimingPoint = null;

            // Correct measure index after removing
            for(int i = measure.Index; i < measures.Count; i++)
            {
                measures[i].Index--;
            }
        }

        /// <summary>
        /// Insert a measure into the timing point at a specified position
        /// </summary>
        /// <param name="where"></param>
        /// <param name="measure"></param>
        public void InsertMeasure(int where, Measure measure)
        {
            if(where < 0 || where > measures.Count) throw new ArgumentOutOfRangeException(nameof(where));

            measure.TimingPoint = this;
            measure.Index = where;
            measures.Insert(where, measure);

            // Correct measure index after removing
            for(int i = measure.Index + 1; i < measures.Count; i++)
            {
                measures[i].Index++;
            }
        }

        public void Clear()
        {
            measures.Clear();
        }

        public override string ToString()
        {
            return
                $"Timing {nameof(offset)}: {Offset}, {nameof(BPM)}: {BPM}, Signature: {Numerator}/{Denominator}, {nameof(firstMeasureOffsetPercentage)}: {FirstMeasureOffsetPercentage}";
        }

        /// <summary>
        /// Updates cached values for faster offset calculation
        /// </summary>
        private void Update()
        {
            // The duration of a beat (4th note)
            beatDuration = 60.0 / BPM;
            
            // A whole note consists of 4 fourths, which is 4 times the beat duration
            WholeNoteDuration = BeatDuration * 4;

            // Take a nth note a certain amount of times to get the measure duration
            MeasureDuration = WholeNoteDuration / Denominator * Numerator;

            actualOffset = Offset - FirstMeasureOffsetPercentage * MeasureDuration;
        }
    }
}