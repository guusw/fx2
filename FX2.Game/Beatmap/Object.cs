// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Beatmap object, can by itself used as a time-only control point
    /// </summary>
    public class Object
    {
        /// <summary>
        /// Position of this object
        /// </summary>
        public TimeDivision Position;

        /// <summary>
        /// Returns the absolute position of this object in the measure
        /// </summary>
        /// <param name="measure"></param>
        /// <returns></returns>
        public double GetAbsolutePosition(Measure measure)
        {
            return measure.AbsolutePosition + Position.Relative * measure.TimingPoint.MeasureDuration;
        }

        public override string ToString()
        {
            return "Object";
        }
    }
}