// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Runtime beatmap format for FX2
    /// </summary>
    public partial class Beatmap
    {
        /// <summary>
        /// Additional data about the beatmap
        /// </summary>
        public BeatmapMetadata Metadata;

        /// <summary>
        /// The root of a beatmap consists of a timing point, containing measures, containing objects
        /// </summary>
        public readonly List<TimingPoint> TimingPoints = new List<TimingPoint>();

        public Beatmap()
        {

        }

        /// <summary>
        /// Creates annotations in every measure about the laser segments that cross it
        /// </summary>
        public void UpdateLaserIntervals()
        {
            List<ObjectReference> activeObjects = new List<ObjectReference>();
            foreach(var tp in TimingPoints)
            {
                foreach(var measure in tp.Measures)
                {
                    measure.CrossingObjects.Clear();
                    measure.CrossingObjects.AddRange(activeObjects);
                    foreach(var obj in measure.Objects)
                    {
                        var laser = obj as Laser;
                        if(laser != null)
                        {
                            var reference = new ObjectReference(laser, measure);
                            if(laser.Previous != null)
                                activeObjects.Remove(laser.Previous);
                            if(laser.Next == null)
                            {
                                activeObjects.Remove(reference);
                            }
                            else
                            {
                                activeObjects.Add(reference);
                            }
                            continue;
                        }

                        var hold = obj as Hold;
                        if(hold != null)
                        {
                            activeObjects.Add(new ObjectReference(hold, measure));
                            continue;
                        }

                        var holdEnd = obj as HoldEnd;
                        if(holdEnd != null)
                        {
                            activeObjects.Remove(holdEnd.StartingPoint);
                            continue;
                        }
                    }
                }
            }   
        }
    }
}