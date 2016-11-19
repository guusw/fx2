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
        /// Division of the measure
        /// </summary>
        public ushort Division;

        /// <summary>
        /// Number of divisions inside the measure to get the position of this object
        /// </summary>
        public int DivisionIndex;

        /// <summary>
        /// 0-1 value of position inside the measure
        /// </summary>
        public double Relative => (double)DivisionIndex / (double)Division;

        public TimeDivision(int divisionIndex, int division)
        {
            DivisionIndex = divisionIndex;
            Division = (ushort)division;
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
            if(Division == other.Division)
                return DivisionIndex == other.DivisionIndex;

            return Division * DivisionIndex == other.Division * other.DivisionIndex;
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
                return (Division.GetHashCode() * 397) ^ DivisionIndex;
            }
        }

        public int CompareTo(TimeDivision other)
        {
            if(Division == other.Division)
                return DivisionIndex.CompareTo(other.DivisionIndex);

            return (other.Division * DivisionIndex).CompareTo(Division * other.DivisionIndex);
        }

        public override string ToString()
        {
            return $"Division {DivisionIndex}/{Division}";
        }
    }
}