// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace FX2.Game.Beatmap
{
    public class TimeDivisionReference
    {
        /// <summary>
        /// The measure that the object is contained in
        /// </summary>
        public Measure Measure;

        /// <summary>
        /// The time division inside a measure
        /// </summary>
        public TimeDivision Position;
        
        public TimeDivisionReference()
        {
        }

        public TimeDivisionReference(TimeDivision position, Measure measure)
        {
            this.Position = position;
            this.Measure = measure;
        }

        public double AbsolutePosition => Measure.AbsolutePosition + Position.Relative * Measure.TimingPoint.MeasureDuration;

        public override string ToString()
        {
            return $"Division Reference {Position.Numerator}/{Position.Denominator} - Measure {Measure.Index}";
        }
        
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;

            return Equals((TimeDivisionReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Measure != null ? Measure.GetHashCode() : 0) * 397) ^ Position.GetHashCode();
            }
        }

        protected bool Equals(TimeDivisionReference other)
        {
            return Equals(Measure, other.Measure) && Position.Equals(other.Position);
        }
    }
}