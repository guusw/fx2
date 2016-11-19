// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;

namespace FX2.Game.Beatmap
{
    public class Measure
    {
        /// <summary>
        /// Index of this measure in it's parent timing
        /// </summary>
        public int Index;

        /// <summary>
        /// The timing point in which this measure is contained
        /// </summary>
        public TimingPoint TimingPoint;
        
        /// <summary>
        /// A list of objects that are not in this measure, but still go through it
        /// </summary>
        public readonly List<ObjectReference> CrossingObjects = new List<ObjectReference>();

        /// <summary>
        /// Absolute position of this measure
        /// </summary>
        public double AbsolutePosition => TimingPoint.GetMeasureOffset(Index);

        /// <summary>
        /// Objects in this measure
        /// </summary>
        public readonly List<Object> Objects = new List<Object>();

        /// <summary>
        /// The previous measure
        /// </summary>
        public Measure Previous
        {
            get
            {
                if(Index > 0)
                    return TimingPoint.Measures[Index - 1];
                return null;
            }
        }

        /// <summary>
        /// The next measure
        /// </summary>
        public Measure Next
        {
            get
            {
                var nextIndex = Index + 1;
                if(nextIndex < TimingPoint.Measures.Count)
                    return TimingPoint.Measures[nextIndex];
                return null;
            }
        }

        /// <summary>
        /// Sort the collection of buttons
        /// </summary>
        public void Sort()
        {
            Objects.Sort((l, r) =>
            {
                int c = l.Position.CompareTo(r.Position);
                if(c == 0)
                {
                    // Sort lasers based on instant order
                    var laserA = l as Laser;
                    var laserB = r as Laser;
                    if(laserA != null && laserB != null)
                        return (laserB.Previous?.Object == laserA) ? 1 : -1;
                }
                return c;
            });
        }

        public override string ToString()
        {
            return $"Measure {Index} ({Objects.Count} Objects)";
        }
    }
}