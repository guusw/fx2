// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.CodeDom;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Platform;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Defines a segment of time of a single measure using a Numerator and Denominator
    /// </summary>
    public struct TimeDivision
    {
        /// <summary>
        /// Starting position
        /// </summary>
        public static readonly TimeDivision Start = new TimeDivision(0,1);

        /// <summary>
        /// Ending position
        /// </summary>
        public static readonly TimeDivision End = new TimeDivision(1, 1);
        
        /// <summary>
        /// Number of segments inside the measure to get the length of this division
        /// </summary>
        public ushort Numerator;

        /// <summary>
        /// Number of segments the measure is divided in
        /// </summary>
        public ushort Denominator;
        
        /// <summary>
        /// 0-1 value of position inside the measure
        /// </summary>
        public double Relative => (double)Numerator / (double)Denominator;

        public TimeDivision(int numerator, int denominator) : this((ushort)numerator, (ushort)denominator)
        {
        }

        public TimeDivision(ushort numerator, ushort denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static bool operator ==(TimeDivision left, TimeDivision right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeDivision left, TimeDivision right)
        {
            return !left.Equals(right);
        }

        public bool Equals(TimeDivision other)
        {
            if(Denominator == other.Denominator)
                return Numerator == other.Numerator;

            return Denominator * Numerator == other.Denominator * other.Numerator;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;

            return obj is TimeDivision && Equals((TimeDivision)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Denominator.GetHashCode() * 397) ^ Numerator;
            }
        }

        public int CompareTo(TimeDivision other)
        {
            if(Denominator == other.Denominator)
                return Numerator.CompareTo(other.Numerator);

            return (other.Denominator * Numerator).CompareTo(Denominator * other.Numerator);
        }

        public override string ToString()
        {
            return $"Division {Numerator}/{Denominator}";
        }
    }
}