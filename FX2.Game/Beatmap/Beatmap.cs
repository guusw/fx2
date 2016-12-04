// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FX2.Game.Beatmap.Effects;
using osu.Framework.Lists;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Runtime beatmap format for FX2
    /// </summary>
    public partial class Beatmap : IComparer<Beatmap.PrecomputedEvent>
    {
        /// <summary>
        /// Additional data about the beatmap
        /// </summary>
        public BeatmapMetadata Metadata;

        /// <summary>
        /// The root of a beatmap consists of a timing point, containing measures, containing objects
        /// </summary>
        public readonly List<TimingPoint> TimingPoints = new List<TimingPoint>();

        /// <summary>
        /// Settings for effects that are used, indexed by <see cref="EffectType"/>
        /// </summary>
        public readonly List<EffectSettings> EffectSettings = new List<EffectSettings>();

        public const int EventGroupCount = 7;
        private SortedList<PrecomputedEvent>[] precomputedEvents;

        public Beatmap()
        {
            RegisterDefaultEffectSettings();
        }

        /// <summary>
        /// Creates annotations in every measure about the laser segments that cross it and computes control point event locations as well
        /// </summary>
        public void UpdateMap()
        {
            // Recreate control point dictionary
            Array.Resize(ref precomputedEvents, EventGroupCount);
            for(int i = 0; i < EventGroupCount; i++)
                precomputedEvents[i] = new SortedList<PrecomputedEvent>(this);

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

                        var evt = obj as Event;
                        if(evt != null)
                        {
                            precomputedEvents[(int)evt.EventGroup].Add(new PrecomputedEvent
                            {
                                Event = evt,
                                Position = new TimeDivisionReference(evt.Position, measure)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets <see cref="EffectSettings"/> to contain all the default settings for the predefined effect types (anything that is not UserDefined...)
        /// </summary>
        public void RegisterDefaultEffectSettings()
        {
            RegisterEffect(EffectType.BitCrusher, new BitCrusherSettings());
            RegisterEffect(EffectType.Retrigger, new RetriggerSettings()
            {
                LoopCount = 0
            });
            RegisterEffect(EffectType.Echo, new EchoSettings());
            RegisterEffect(EffectType.Gate, new RetriggerSettings
            {
                LoopCount = 1
            });
            RegisterEffect(EffectType.Flanger, new FlangerSettings());
            RegisterEffect(EffectType.HighPassFilter, new BiQuadFilterSettings
            {
                FilterType = BiQuadFilterType.HighPass
            });
            RegisterEffect(EffectType.LowPassFilter, new BiQuadFilterSettings
            {
                FilterType = BiQuadFilterType.LowPass
            });
            RegisterEffect(EffectType.PeakingFilter, new BiQuadFilterSettings
            {
                FilterType = BiQuadFilterType.Peaking
            });
            RegisterEffect(EffectType.Phaser, new PhaserSettings());
            RegisterEffect(EffectType.SideChain, new SideChainSettings());
            RegisterEffect(EffectType.Wobble, new WobbleSettings());
            RegisterEffect(EffectType.TapeStop, new TapeStopSettings());
        }

        /// <summary>
        /// Registers a new custom event type, adding it to <see cref="EffectSettings"/>
        /// </summary>
        public void RegisterEffect(EffectType effectType, EffectSettings settings)
        {
            while(EffectSettings.Count <= (int)effectType)
                EffectSettings.Add(null);

            EffectSettings[(int)effectType] = settings;
        }

        /// <summary>
        /// Gets the effect settings for a given event type
        /// </summary>
        public EffectSettings GetEffectSettings(EffectType effectType)
        {
            if((int)effectType >= EffectSettings.Count)
                return null;

            return EffectSettings[(int)effectType];
        }

        /// <summary>
        /// Finds the active event at any give time for a certain event group using a binary search in a per event collection
        /// </summary>
        /// <remarks>This collection is updated by UpdateMap</remarks>
        public Event GetEffectAtTime(TimeDivisionReference time, EventGroup group)
        {
            var collection = precomputedEvents[(int)group];
            if(collection.Count == 0)
                return null;

            int left = 0;
            int right = collection.Count - 1;
            while(left < right)
            {
                int mid = left + (right - left) / 2;
                var middle = collection[mid];

                if(middle.Position.CompareTo(time) < 0)
                    left = mid + 1;
                else
                    right = mid;
            }

            return collection[left].Event;
        }

        public int Compare(PrecomputedEvent x, PrecomputedEvent y)
        {
            return x.CompareTo(y);
        }

        public class PrecomputedEvent : IComparable<PrecomputedEvent>
        {
            public TimeDivisionReference Position;
            public Event Event;

            public int CompareTo(PrecomputedEvent other) => Position.CompareTo(other.Position);
        }
    }
}